using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Services
{
    public class PowerBIConfiguration
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string WorkspaceId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AuthorityUrl { get; set; } = "https://login.microsoftonline.com/common/";
        public string ResourceUrl { get; set; } = "https://analysis.windows.net/powerbi/api";
        public string ApiUrl { get; set; } = "https://api.powerbi.com/v1.0/myorg/";
    }

    public interface IPowerBIWorkspaceService
    {
        Task<PowerBIWorkspaceStatus> GetWorkspaceStatusAsync();
        Task<List<PowerBIDataset>> GetDatasetsAsync();
        Task<List<PowerBIReport>> GetReportsAsync();
        Task<PowerBIRefreshResult> RefreshDatasetAsync(string datasetId);
        Task<PowerBIEmbedToken> GenerateEmbedTokenAsync(string reportId, List<string> datasetIds);
        Task<bool> CreateOrUpdateDatasetAsync(PowerBIDatasetDefinition definition);
        Task<List<PowerBIAlert>> GetAlertsAsync();
        Task<PowerBIUsageMetrics> GetUsageMetricsAsync(string reportId, DateTime startDate, DateTime endDate);
    }

    public class PowerBIWorkspaceService : IPowerBIWorkspaceService
    {
        private readonly PowerBIConfiguration _config;
        private readonly ILogger<PowerBIWorkspaceService> _logger;
        private readonly HttpClient _httpClient;
        private string? _accessToken;
        private DateTime _tokenExpiry;

        public PowerBIWorkspaceService(PowerBIConfiguration config, ILogger<PowerBIWorkspaceService> logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<PowerBIWorkspaceStatus> GetWorkspaceStatusAsync()
        {
            try
            {
                _logger.LogInformation("Getting Power BI workspace status");

                await EnsureValidTokenAsync();
                
                var client = CreatePowerBIClient();
                var workspaceId = Guid.Parse(_config.WorkspaceId);
                var workspacesResponse = await client.Groups.GetGroupsAsync();
                var workspace = workspacesResponse.Value?.FirstOrDefault(g => g.Id == workspaceId);
                
                var datasets = await client.Datasets.GetDatasetsInGroupAsync(Guid.Parse(_config.WorkspaceId));
                var reports = await client.Reports.GetReportsInGroupAsync(Guid.Parse(_config.WorkspaceId));

                var status = new PowerBIWorkspaceStatus
                {
                    WorkspaceId = _config.WorkspaceId,
                    WorkspaceName = workspace?.Name ?? "Unknown Workspace",
                    IsActive = workspace != null,
                    DatasetCount = datasets.Value?.Count ?? 0,
                    ReportCount = reports.Value?.Count ?? 0,
                    LastUpdated = DateTime.UtcNow,
                    Capacity = GetWorkspaceCapacity(workspace),
                    Region = GetWorkspaceRegion(workspace)
                };

                _logger.LogInformation("Workspace status retrieved: {DatasetCount} datasets, {ReportCount} reports", 
                    status.DatasetCount, status.ReportCount);

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace status");
                throw;
            }
        }

        public async Task<List<PowerBIDataset>> GetDatasetsAsync()
        {
            try
            {
                _logger.LogInformation("Getting Power BI datasets");

                await EnsureValidTokenAsync();
                var client = CreatePowerBIClient();
                
                var datasetsResponse = await client.Datasets.GetDatasetsInGroupAsync(Guid.Parse(_config.WorkspaceId));
                var datasets = new List<PowerBIDataset>();

                if (datasetsResponse.Value != null)
                {
                    foreach (var dataset in datasetsResponse.Value)
                    {
                        // Get refresh history for each dataset
                        var refreshHistory = await GetDatasetRefreshHistoryAsync(client, dataset.Id);
                        
                        datasets.Add(new PowerBIDataset
                        {
                            Id = dataset.Id,
                            Name = dataset.Name,
                            ConfiguredBy = dataset.ConfiguredBy ?? "System",
                            IsRefreshable = dataset.IsRefreshable ?? false,
                            LastRefresh = refreshHistory.FirstOrDefault()?.EndTime,
                            RefreshStatus = refreshHistory.FirstOrDefault()?.Status ?? "Unknown",
                            TableCount = await GetDatasetTableCountAsync(client, dataset.Id),
                            SizeInBytes = await GetDatasetSizeAsync(client, dataset.Id)
                        });
                    }
                }

                _logger.LogInformation("Retrieved {Count} datasets", datasets.Count);
                return datasets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting datasets");
                throw;
            }
        }

        public async Task<List<PowerBIReport>> GetReportsAsync()
        {
            try
            {
                _logger.LogInformation("Getting Power BI reports");

                await EnsureValidTokenAsync();
                var client = CreatePowerBIClient();
                
                var reportsResponse = await client.Reports.GetReportsInGroupAsync(Guid.Parse(_config.WorkspaceId));
                var reports = new List<PowerBIReport>();

                if (reportsResponse.Value != null)
                {
                    foreach (var report in reportsResponse.Value)
                    {
                        reports.Add(new PowerBIReport
                        {
                            Id = report.Id.ToString(),
                            Name = report.Name,
                            WebUrl = report.WebUrl,
                            EmbedUrl = report.EmbedUrl,
                            DatasetId = report.DatasetId,
                            CreatedBy = "System", // Power BI API doesn't always provide this
                            CreatedDate = DateTime.UtcNow, // Placeholder
                            LastViewed = await GetReportLastViewedAsync(report.Id.ToString()),
                            ViewCount = await GetReportViewCountAsync(report.Id.ToString())
                        });
                    }
                }

                _logger.LogInformation("Retrieved {Count} reports", reports.Count);
                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports");
                throw;
            }
        }

        public async Task<PowerBIRefreshResult> RefreshDatasetAsync(string datasetId)
        {
            try
            {
                _logger.LogInformation("Refreshing dataset {DatasetId}", datasetId);

                await EnsureValidTokenAsync();
                var client = CreatePowerBIClient();
                
                // Trigger refresh
                await client.Datasets.RefreshDatasetInGroupAsync(Guid.Parse(_config.WorkspaceId), datasetId);
                
                // Wait a moment and check status
                await Task.Delay(2000);
                var refreshHistory = await GetDatasetRefreshHistoryAsync(client, datasetId);
                var latestRefresh = refreshHistory.FirstOrDefault();

                var result = new PowerBIRefreshResult
                {
                    DatasetId = datasetId,
                    RefreshId = latestRefresh?.RequestId ?? "Unknown",
                    Status = latestRefresh?.Status ?? "Initiated",
                    StartTime = latestRefresh?.StartTime ?? DateTime.UtcNow,
                    EndTime = latestRefresh?.EndTime,
                    Duration = latestRefresh?.EndTime.HasValue == true && latestRefresh.StartTime.HasValue 
                        ? latestRefresh.EndTime.Value - latestRefresh.StartTime.Value 
                        : null
                };

                _logger.LogInformation("Dataset refresh initiated: {Status}", result.Status);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dataset {DatasetId}", datasetId);
                throw;
            }
        }

        public async Task<PowerBIEmbedToken> GenerateEmbedTokenAsync(string reportId, List<string> datasetIds)
        {
            try
            {
                _logger.LogInformation("Generating embed token for report {ReportId}", reportId);

                await EnsureValidTokenAsync();
                var client = CreatePowerBIClient();

                var generateTokenRequest = new GenerateTokenRequest(
                    accessLevel: TokenAccessLevel.View
                );

                var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(
                    Guid.Parse(_config.WorkspaceId), 
                    Guid.Parse(reportId), 
                    generateTokenRequest);

                var embedToken = new PowerBIEmbedToken
                {
                    Token = tokenResponse.Token,
                    TokenId = tokenResponse.TokenId.ToString(),
                    Expiration = tokenResponse.Expiration,
                    ReportId = reportId,
                    DatasetIds = datasetIds,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Embed token generated, expires at {Expiration}", embedToken.Expiration);
                return embedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embed token for report {ReportId}", reportId);
                throw;
            }
        }

        public async Task<bool> CreateOrUpdateDatasetAsync(PowerBIDatasetDefinition definition)
        {
            try
            {
                _logger.LogInformation("Creating/updating dataset {DatasetName}", definition.Name);

                await EnsureValidTokenAsync();
                var client = CreatePowerBIClient();

                // Check if dataset exists
                var existingDatasets = await client.Datasets.GetDatasetsInGroupAsync(Guid.Parse(_config.WorkspaceId));
                var existingDataset = existingDatasets.Value?.FirstOrDefault(d => d.Name == definition.Name);

                if (existingDataset != null)
                {
                    // Update existing dataset
                    _logger.LogInformation("Updating existing dataset {DatasetId}", existingDataset.Id);
                    // Note: Power BI API has limited update capabilities for datasets
                    // Typically requires recreating the dataset
                    return true;
                }
                else
                {
                    // Create new dataset
                    var createDatasetRequest = new CreateDatasetRequest
                    {
                        Name = definition.Name,
                        Tables = definition.Tables.Select(t => new Table
                        {
                            Name = t.Name,
                            Columns = t.Columns.Select(c => new Column
                            {
                                Name = c.Name,
                                DataType = c.DataType
                            }).ToList()
                        }).ToList()
                    };

                    var newDataset = await client.Datasets.PostDatasetInGroupAsync(
                        Guid.Parse(_config.WorkspaceId), 
                        createDatasetRequest);

                    _logger.LogInformation("Dataset created with ID {DatasetId}", newDataset.Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating dataset {DatasetName}", definition.Name);
                return false;
            }
        }

        public async Task<List<PowerBIAlert>> GetAlertsAsync()
        {
            try
            {
                _logger.LogInformation("Getting Power BI alerts");

                // Simulate async operation
                await Task.Delay(100);

                // For demo purposes, return mock alerts based on configuration
                // In real implementation, use Power BI API to get actual alerts
                var alerts = new List<PowerBIAlert>
                {
                    new PowerBIAlert
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Vessel Delay Alert",
                        Description = "Alert when vessel delays exceed 30 minutes",
                        IsActive = true,
                        Condition = "DelayMinutes > 30",
                        Frequency = "Immediate",
                        Recipients = new List<string> { "operations@havila.no" },
                        LastTriggered = DateTime.UtcNow.AddHours(-2),
                        TriggerCount = 5
                    },
                    new PowerBIAlert
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Low Satisfaction Alert",
                        Description = "Alert when customer satisfaction drops below 3.5",
                        IsActive = true,
                        Condition = "avg(Rating) < 3.5",
                        Frequency = "Daily",
                        Recipients = new List<string> { "customer-service@havila.no" },
                        LastTriggered = DateTime.UtcNow.AddDays(-1),
                        TriggerCount = 2
                    }
                };

                _logger.LogInformation("Retrieved {Count} alerts", alerts.Count);
                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts");
                throw;
            }
        }

        public async Task<PowerBIUsageMetrics> GetUsageMetricsAsync(string reportId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Getting usage metrics for report {ReportId}", reportId);

                await Task.CompletedTask; // Placeholder for async requirement

                // Mock usage metrics - in real implementation, use Power BI API
                var metrics = new PowerBIUsageMetrics
                {
                    ReportId = reportId,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalViews = Random.Shared.Next(100, 1000),
                    UniqueViewers = Random.Shared.Next(20, 100),
                    AverageViewDuration = TimeSpan.FromMinutes(Random.Shared.Next(5, 30)),
                    TopViewers = new List<string> 
                    { 
                        "fleet.manager@havila.no", 
                        "operations@havila.no", 
                        "captain.smith@havila.no" 
                    },
                    PeakUsageHour = Random.Shared.Next(8, 18),
                    MobileViewPercentage = Random.Shared.Next(20, 60),
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Usage metrics retrieved: {TotalViews} views, {UniqueViewers} unique viewers", 
                    metrics.TotalViews, metrics.UniqueViewers);

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage metrics for report {ReportId}", reportId);
                throw;
            }
        }

        // Private helper methods
        private async Task EnsureValidTokenAsync()
        {
            if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _tokenExpiry.AddMinutes(-5))
            {
                await RefreshAccessTokenAsync();
            }
        }

        private async Task RefreshAccessTokenAsync()
        {
            try
            {
                // For demo purposes, simulate token refresh
                // In real implementation, use MSAL or similar for authentication
                _accessToken = $"fake_token_{Guid.NewGuid()}";
                _tokenExpiry = DateTime.UtcNow.AddHours(1);
                
                _logger.LogInformation("Access token refreshed, expires at {Expiry}", _tokenExpiry);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing access token");
                throw;
            }
        }

        private PowerBIClient CreatePowerBIClient()
        {
            var tokenCredentials = new TokenCredentials(_accessToken, "Bearer");
            return new PowerBIClient(new Uri(_config.ApiUrl), tokenCredentials);
        }

        private async Task<List<Refresh>> GetDatasetRefreshHistoryAsync(PowerBIClient client, string datasetId)
        {
            try
            {
                var refreshHistory = await client.Datasets.GetRefreshHistoryInGroupAsync(
                    Guid.Parse(_config.WorkspaceId), 
                    datasetId);
                return refreshHistory.Value?.ToList() ?? new List<Refresh>();
            }
            catch
            {
                return new List<Refresh>();
            }
        }

        private async Task<int> GetDatasetTableCountAsync(PowerBIClient client, string datasetId)
        {
            try
            {
                var tablesResponse = await client.Datasets.GetTablesInGroupAsync(
                    Guid.Parse(_config.WorkspaceId), 
                    datasetId);
                return tablesResponse.Value?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<long> GetDatasetSizeAsync(PowerBIClient client, string datasetId)
        {
            // Mock implementation - actual API might not provide size directly
            await Task.CompletedTask;
            return Random.Shared.NextInt64(1000000, 100000000); // 1MB to 100MB
        }

        private async Task<DateTime?> GetReportLastViewedAsync(string reportId)
        {
            // Mock implementation - would require usage metrics API
            await Task.CompletedTask;
            return DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30));
        }

        private async Task<int> GetReportViewCountAsync(string reportId)
        {
            // Mock implementation - would require usage metrics API
            await Task.CompletedTask;
            return Random.Shared.Next(10, 500);
        }

        private string GetWorkspaceCapacity(Group? workspace)
        {
            // Mock implementation - extract from workspace properties
            return "A1";
        }

        private string GetWorkspaceRegion(Group? workspace)
        {
            // Mock implementation - extract from workspace properties
            return "West Europe";
        }
    }

    // Model classes
    public class PowerBIWorkspaceStatus
    {
        public string WorkspaceId { get; set; } = string.Empty;
        public string WorkspaceName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DatasetCount { get; set; }
        public int ReportCount { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Capacity { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    public class PowerBIDataset
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ConfiguredBy { get; set; } = string.Empty;
        public bool IsRefreshable { get; set; }
        public DateTime? LastRefresh { get; set; }
        public string RefreshStatus { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public long SizeInBytes { get; set; }
    }

    public class PowerBIReport
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WebUrl { get; set; } = string.Empty;
        public string EmbedUrl { get; set; } = string.Empty;
        public string DatasetId { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastViewed { get; set; }
        public int ViewCount { get; set; }
    }

    public class PowerBIRefreshResult
    {
        public string DatasetId { get; set; } = string.Empty;
        public string RefreshId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
    }

    public class PowerBIEmbedToken
    {
        public string Token { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string ReportId { get; set; } = string.Empty;
        public List<string> DatasetIds { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class PowerBIDatasetDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PowerBITableDefinition> Tables { get; set; } = new();
    }

    public class PowerBITableDefinition
    {
        public string Name { get; set; } = string.Empty;
        public List<PowerBIColumnDefinition> Columns { get; set; } = new();
    }

    public class PowerBIColumnDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
    }

    public class PowerBIAlert
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new();
        public DateTime? LastTriggered { get; set; }
        public int TriggerCount { get; set; }
    }

    public class PowerBIUsageMetrics
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public TimeSpan AverageViewDuration { get; set; }
        public List<string> TopViewers { get; set; } = new();
        public int PeakUsageHour { get; set; }
        public double MobileViewPercentage { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    // Extension methods for dependency injection
    public static class PowerBIExtensions
    {
        public static IServiceCollection AddHavilaPowerBI(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("PowerBI").Get<PowerBIConfiguration>();
            services.AddSingleton(config!);
            
            services.AddHttpClient<PowerBIWorkspaceService>();
            services.AddScoped<IPowerBIWorkspaceService, PowerBIWorkspaceService>();

            return services;
        }
    }
}