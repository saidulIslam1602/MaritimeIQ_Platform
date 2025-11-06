using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using MaritimeIQ.Platform.Models.Incident;
using Newtonsoft.Json;
using System.Text;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// PagerDuty integration service for real incident management
    /// Implements industry-standard alerting and escalation
    /// </summary>
    public interface IPagerDutyService
    {
        Task<string> TriggerIncidentAsync(string summary, IncidentSeverity severity, Dictionary<string, object> customDetails, string? dedupKey = null);
        Task<bool> AcknowledgeIncidentAsync(string dedupKey, string acknowledgedBy);
        Task<bool> ResolveIncidentAsync(string dedupKey, string resolvedBy, string? resolutionNote = null);
        Task<bool> SendIncidentUpdateAsync(string dedupKey, string message, Dictionary<string, object>? additionalDetails = null);
        Task<List<PagerDutyIncident>> GetActiveIncidentsAsync();
        Task<bool> TestIntegrationAsync();
    }

    public class PagerDutyService : BaseMaritimeService, IPagerDutyService
    {
        private readonly HttpClient _httpClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly string _integrationKey;
        private readonly string _apiToken;
        private readonly string _serviceId;
        private const string EVENTS_API_URL = "https://events.pagerduty.com/v2/enqueue";
        private const string REST_API_URL = "https://api.pagerduty.com";

        public override string ServiceName => "PagerDuty Integration Service";

        public PagerDutyService(
            HttpClient httpClient,
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            ILogger<PagerDutyService> logger) : base(logger, configuration)
        {
            _httpClient = httpClient;
            _telemetryClient = telemetryClient;
            
            // Get PagerDuty configuration from app settings
            _integrationKey = configuration["PagerDuty:IntegrationKey"] ?? throw new InvalidOperationException("PagerDuty IntegrationKey not configured");
            _apiToken = configuration["PagerDuty:ApiToken"] ?? throw new InvalidOperationException("PagerDuty ApiToken not configured");
            _serviceId = configuration["PagerDuty:ServiceId"] ?? throw new InvalidOperationException("PagerDuty ServiceId not configured");
            
            // Configure HTTP client for PagerDuty API
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token token={_apiToken}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.pagerduty+json;version=2");
        }

        /// <summary>
        /// Trigger a new incident in PagerDuty
        /// </summary>
        public async Task<string> TriggerIncidentAsync(string summary, IncidentSeverity severity, Dictionary<string, object> customDetails, string? dedupKey = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    dedupKey ??= Guid.NewGuid().ToString();
                    
                    var payload = new
                    {
                        routing_key = _integrationKey,
                        event_action = "trigger",
                        dedup_key = dedupKey,
                        payload = new
                        {
                            summary = summary,
                            severity = MapSeverityToPagerDuty(severity),
                            source = "MaritimeIQ Platform",
                            component = "Maritime Operations",
                            group = "Production",
                            @class = "Maritime Infrastructure",
                            custom_details = customDetails
                        }
                    };

                    var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    _logger.LogInformation("Triggering PagerDuty incident: {Summary} (Severity: {Severity}, DedupKey: {DedupKey})", 
                        summary, severity, dedupKey);

                    var response = await _httpClient.PostAsync(EVENTS_API_URL, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<PagerDutyEventResponse>(responseContent);
                        
                        // Track successful incident creation
                        _telemetryClient.TrackEvent("PagerDutyIncidentTriggered", new Dictionary<string, string>
                        {
                            ["IncidentKey"] = dedupKey,
                            ["Severity"] = severity.ToString(),
                            ["Summary"] = summary,
                            ["Status"] = result?.Status ?? "unknown"
                        });

                        _logger.LogInformation("PagerDuty incident triggered successfully. Status: {Status}, Message: {Message}", 
                            result?.Status, result?.Message);
                        
                        return dedupKey;
                    }
                    else
                    {
                        _logger.LogError("Failed to trigger PagerDuty incident. Status: {StatusCode}, Response: {Response}", 
                            response.StatusCode, responseContent);
                        
                        _telemetryClient.TrackException(new Exception($"PagerDuty API error: {response.StatusCode} - {responseContent}"));
                        throw new Exception($"PagerDuty API error: {response.StatusCode} - {responseContent}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error triggering PagerDuty incident: {Summary}", summary);
                    _telemetryClient.TrackException(ex);
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                    _telemetryClient.TrackDependency("PagerDuty", "TriggerIncident", DateTime.UtcNow.Subtract(stopwatch.Elapsed), stopwatch.Elapsed, true);
                }
            });
        }

        /// <summary>
        /// Acknowledge an existing incident
        /// </summary>
        public async Task<bool> AcknowledgeIncidentAsync(string dedupKey, string acknowledgedBy)
        {
            return await ExecuteOperationAsync(async () =>
            {
                try
                {
                    var payload = new
                    {
                        routing_key = _integrationKey,
                        event_action = "acknowledge",
                        dedup_key = dedupKey
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    _logger.LogInformation("Acknowledging PagerDuty incident: {DedupKey} by {AcknowledgedBy}", dedupKey, acknowledgedBy);

                    var response = await _httpClient.PostAsync(EVENTS_API_URL, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        _telemetryClient.TrackEvent("PagerDutyIncidentAcknowledged", new Dictionary<string, string>
                        {
                            ["IncidentKey"] = dedupKey,
                            ["AcknowledgedBy"] = acknowledgedBy
                        });

                        _logger.LogInformation("PagerDuty incident acknowledged successfully: {DedupKey}", dedupKey);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Failed to acknowledge PagerDuty incident: {DedupKey}. Response: {Response}", dedupKey, responseContent);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error acknowledging PagerDuty incident: {DedupKey}", dedupKey);
                    _telemetryClient.TrackException(ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// Resolve an existing incident
        /// </summary>
        public async Task<bool> ResolveIncidentAsync(string dedupKey, string resolvedBy, string? resolutionNote = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                try
                {
                    var payload = new
                    {
                        routing_key = _integrationKey,
                        event_action = "resolve",
                        dedup_key = dedupKey,
                        payload = new
                        {
                            summary = resolutionNote ?? "Incident resolved",
                            custom_details = new
                            {
                                resolved_by = resolvedBy,
                                resolution_time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                resolution_note = resolutionNote
                            }
                        }
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    _logger.LogInformation("Resolving PagerDuty incident: {DedupKey} by {ResolvedBy}", dedupKey, resolvedBy);

                    var response = await _httpClient.PostAsync(EVENTS_API_URL, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        _telemetryClient.TrackEvent("PagerDutyIncidentResolved", new Dictionary<string, string>
                        {
                            ["IncidentKey"] = dedupKey,
                            ["ResolvedBy"] = resolvedBy,
                            ["ResolutionNote"] = resolutionNote ?? "No note provided"
                        });

                        _logger.LogInformation("PagerDuty incident resolved successfully: {DedupKey}", dedupKey);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Failed to resolve PagerDuty incident: {DedupKey}. Response: {Response}", dedupKey, responseContent);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resolving PagerDuty incident: {DedupKey}", dedupKey);
                    _telemetryClient.TrackException(ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// Send an update to an existing incident
        /// </summary>
        public async Task<bool> SendIncidentUpdateAsync(string dedupKey, string message, Dictionary<string, object>? additionalDetails = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                try
                {
                    var customDetails = new Dictionary<string, object>
                    {
                        ["update_message"] = message,
                        ["update_time"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    };

                    if (additionalDetails != null)
                    {
                        foreach (var detail in additionalDetails)
                        {
                            customDetails[detail.Key] = detail.Value;
                        }
                    }

                    var payload = new
                    {
                        routing_key = _integrationKey,
                        event_action = "trigger", // Use trigger with same dedup_key to update
                        dedup_key = dedupKey,
                        payload = new
                        {
                            summary = $"Update: {message}",
                            severity = "info",
                            source = "MaritimeIQ Platform",
                            custom_details = customDetails
                        }
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(EVENTS_API_URL, content);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending PagerDuty incident update: {DedupKey}", dedupKey);
                    _telemetryClient.TrackException(ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// Get active incidents from PagerDuty
        /// </summary>
        public async Task<List<PagerDutyIncident>> GetActiveIncidentsAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                try
                {
                    var url = $"{REST_API_URL}/incidents?service_ids[]={_serviceId}&statuses[]=triggered&statuses[]=acknowledged";
                    var response = await _httpClient.GetAsync(url);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<PagerDutyIncidentsResponse>(responseContent);
                        return result?.Incidents ?? new List<PagerDutyIncident>();
                    }
                    else
                    {
                        _logger.LogError("Failed to get active PagerDuty incidents. Response: {Response}", responseContent);
                        return new List<PagerDutyIncident>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting active PagerDuty incidents");
                    _telemetryClient.TrackException(ex);
                    return new List<PagerDutyIncident>();
                }
            });
        }

        /// <summary>
        /// Test PagerDuty integration connectivity
        /// </summary>
        public async Task<bool> TestIntegrationAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                try
                {
                    _logger.LogInformation("Testing PagerDuty integration...");
                    
                    var testIncidentKey = await TriggerIncidentAsync(
                        "MaritimeIQ Platform - Integration Test", 
                        IncidentSeverity.Info, 
                        new Dictionary<string, object>
                        {
                            ["test"] = true,
                            ["timestamp"] = DateTime.UtcNow,
                            ["environment"] = "test"
                        },
                        $"test-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
                    );

                    // Wait a moment then resolve the test incident
                    await Task.Delay(2000);
                    
                    var resolved = await ResolveIncidentAsync(testIncidentKey, "System", "Integration test completed successfully");
                    
                    _logger.LogInformation("PagerDuty integration test completed. Resolved: {Resolved}", resolved);
                    return !string.IsNullOrEmpty(testIncidentKey) && resolved;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PagerDuty integration test failed");
                    return false;
                }
            });
        }

        /// <summary>
        /// Map internal severity to PagerDuty severity levels
        /// </summary>
        private static string MapSeverityToPagerDuty(IncidentSeverity severity)
        {
            return severity switch
            {
                IncidentSeverity.Critical => "critical",
                IncidentSeverity.High => "error",
                IncidentSeverity.Medium => "warning",
                IncidentSeverity.Low => "info",
                IncidentSeverity.Info => "info",
                _ => "info"
            };
        }
    }

    /// <summary>
    /// PagerDuty API response models
    /// </summary>
    public class PagerDutyEventResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? DedupKey { get; set; }
    }

    public class PagerDutyIncidentsResponse
    {
        public List<PagerDutyIncident>? Incidents { get; set; }
    }

    public class PagerDutyIncident
    {
        public string? Id { get; set; }
        public string? Summary { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Urgency { get; set; }
        public PagerDutyService? Service { get; set; }
        public List<PagerDutyAssignment>? Assignments { get; set; }
    }

    public class PagerDutyAssignment
    {
        public PagerDutyUser? Assignee { get; set; }
    }

    public class PagerDutyUser
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
