using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Models.PowerBI;
using MaritimeIQ.Platform.Services.Interfaces;

namespace MaritimeIQ.Platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PowerBIController : ControllerBase
    {
        private readonly IPowerBIService _powerBIService;
        private readonly ILogger<PowerBIController> _logger;

        public PowerBIController(
            IPowerBIService powerBIService,
            ILogger<PowerBIController> logger)
        {
            _powerBIService = powerBIService;
            _logger = logger;
        }

        private async Task<IActionResult> ExecuteOperationAsync(Func<Task<IActionResult>> operation)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing PowerBI operation");
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Get all maritime dashboards available in Power BI
        /// </summary>
        [HttpGet("dashboards")]
        public async Task<IActionResult> GetMaritimeDashboards()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var dashboards = await _powerBIService.GetMaritimeDashboardsAsync();
                return Ok(dashboards);
            });
        }

        /// <summary>
        /// Get a specific dashboard by ID
        /// </summary>
        [HttpGet("dashboard/{dashboardId}")]
        public async Task<IActionResult> GetDashboard(string dashboardId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var dashboard = await _powerBIService.GetDashboardByIdAsync(dashboardId);
                
                if (dashboard == null)
                {
                    return NotFound($"Dashboard with ID '{dashboardId}' not found");
                }

                return Ok(dashboard);
            });
        }

        /// <summary>
        /// Get embed token for Power BI dashboard
        /// </summary>
        [HttpGet("dashboard/{dashboardId}/embed-token")]
        public async Task<IActionResult> GetEmbedToken(string dashboardId, [FromQuery] string userId = "")
        {
            return await ExecuteOperationAsync(async () =>
            {
                var embedConfig = await _powerBIService.GetEmbedTokenAsync(dashboardId, userId);
                return Ok(embedConfig);
            });
        }

        /// <summary>
        /// Refresh a specific Power BI dashboard
        /// </summary>
        [HttpPost("dashboard/{dashboardId}/refresh")]
        public async Task<IActionResult> RefreshDashboard(string dashboardId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var success = await _powerBIService.RefreshDashboardAsync(dashboardId);
                
                if (!success)
                {
                    return BadRequest($"Failed to refresh dashboard '{dashboardId}'");
                }

                return Ok(new { 
                    Message = $"Dashboard '{dashboardId}' refresh initiated successfully", 
                    RefreshTime = DateTime.UtcNow 
                });
            });
        }

        /// <summary>
        /// Get comprehensive KPI summary for maritime operations
        /// </summary>
        [HttpGet("kpis/summary")]
        public async Task<IActionResult> GetKPISummary()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var kpiSummary = await _powerBIService.GetKPISummaryAsync();
                return Ok(kpiSummary);
            });
        }

        /// <summary>
        /// Get Northern Lights forecast for optimal viewing
        /// </summary>
        [HttpGet("northern-lights/forecast")]
        public async Task<IActionResult> GetNorthernLightsForecast()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var forecast = await _powerBIService.GetNorthernLightsForecastAsync();
                return Ok(forecast);
            });
        }

        /// <summary>
        /// Get data for a specific Power BI report
        /// </summary>
        [HttpGet("reports/{reportId}/data")]
        public async Task<IActionResult> GetReportData(string reportId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var reportData = await _powerBIService.GetReportDataAsync(reportId);
                return Ok(reportData);
            });
        }

        /// <summary>
        /// Get all available Power BI reports
        /// </summary>
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var reports = await _powerBIService.GetReportsAsync();
                return Ok(reports);
            });
        }

        /// <summary>
        /// Get a specific Power BI report by ID
        /// </summary>
        [HttpGet("reports/{reportId}")]
        public async Task<IActionResult> GetReport(string reportId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var report = await _powerBIService.GetReportByIdAsync(reportId);
                
                if (report == null)
                {
                    return NotFound($"Report with ID '{reportId}' not found");
                }

                return Ok(report);
            });
        }

        /// <summary>
        /// Get usage metrics for a specific report
        /// </summary>
        [HttpGet("reports/{reportId}/usage")]
        public async Task<IActionResult> GetReportUsageMetrics(
            string reportId, 
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start >= end)
                {
                    return BadRequest("Start date must be before end date");
                }

                var usageMetrics = await _powerBIService.GetUsageMetricsAsync(reportId, start, end);
                return Ok(usageMetrics);
            });
        }

        /// <summary>
        /// Get all Power BI datasets
        /// </summary>
        [HttpGet("datasets")]
        public async Task<IActionResult> GetDatasets()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var datasets = await _powerBIService.GetDatasetsAsync();
                return Ok(datasets);
            });
        }

        /// <summary>
        /// Get a specific dataset by ID
        /// </summary>
        [HttpGet("datasets/{datasetId}")]
        public async Task<IActionResult> GetDataset(string datasetId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var dataset = await _powerBIService.GetDatasetByIdAsync(datasetId);
                
                if (dataset == null)
                {
                    return NotFound($"Dataset with ID '{datasetId}' not found");
                }

                return Ok(dataset);
            });
        }

        /// <summary>
        /// Refresh a specific Power BI dataset
        /// </summary>
        [HttpPost("datasets/{datasetId}/refresh")]
        public async Task<IActionResult> RefreshDataset(string datasetId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var refreshResult = await _powerBIService.RefreshDatasetAsync(datasetId);
                return Ok(refreshResult);
            });
        }

        /// <summary>
        /// Update a Power BI dataset with new data
        /// </summary>
        [HttpPost("datasets/{datasetId}/update")]
        public async Task<IActionResult> UpdateDataset(
            string datasetId, 
            [FromBody] Dictionary<string, object> data)
        {
            return await ExecuteOperationAsync(async () =>
            {
                if (data == null || !data.Any())
                {
                    return BadRequest("Data payload cannot be empty");
                }

                var success = await _powerBIService.UpdateDatasetAsync(datasetId, data);
                
                if (!success)
                {
                    return BadRequest($"Failed to update dataset '{datasetId}'");
                }

                return Ok(new { 
                    Message = $"Dataset '{datasetId}' updated successfully", 
                    UpdateTime = DateTime.UtcNow,
                    RecordsUpdated = data.Count
                });
            });
        }

        /// <summary>
        /// Get tiles for a specific dashboard
        /// </summary>
        [HttpGet("dashboard/{dashboardId}/tiles")]
        public async Task<IActionResult> GetDashboardTiles(string dashboardId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var tiles = await _powerBIService.GetDashboardTilesAsync(dashboardId);
                return Ok(tiles);
            });
        }

        /// <summary>
        /// Get a specific tile by ID
        /// </summary>
        [HttpGet("tiles/{tileId}")]
        public async Task<IActionResult> GetTile(string tileId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var tile = await _powerBIService.GetTileByIdAsync(tileId);
                
                if (tile == null)
                {
                    return NotFound($"Tile with ID '{tileId}' not found");
                }

                return Ok(tile);
            });
        }

        /// <summary>
        /// Update a specific dashboard tile
        /// </summary>
        [HttpPut("tiles/{tileId}")]
        public async Task<IActionResult> UpdateTile(string tileId, [FromBody] DashboardTile tile)
        {
            return await ExecuteOperationAsync(async () =>
            {
                if (tile == null)
                {
                    return BadRequest("Tile data is required");
                }

                var success = await _powerBIService.UpdateTileAsync(tileId, tile);
                
                if (!success)
                {
                    return BadRequest($"Failed to update tile '{tileId}'");
                }

                return Ok(new { 
                    Message = $"Tile '{tileId}' updated successfully", 
                    UpdateTime = DateTime.UtcNow 
                });
            });
        }

        /// <summary>
        /// Validate Power BI connection and configuration
        /// </summary>
        [HttpGet("connection/validate")]
        public async Task<IActionResult> ValidateConnection()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var isValid = await _powerBIService.ValidateConnectionAsync();
                
                return Ok(new { 
                    IsValid = isValid,
                    Message = isValid ? "Power BI connection is valid" : "Power BI connection failed",
                    CheckedAt = DateTime.UtcNow
                });
            });
        }

        /// <summary>
        /// Get current Power BI workspace information
        /// </summary>
        [HttpGet("workspace/info")]
        public async Task<IActionResult> GetWorkspaceInfo()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var workspaceInfo = await _powerBIService.GetWorkspaceInfoAsync();
                return Ok(workspaceInfo);
            });
        }

        /// <summary>
        /// Get available data sources for Power BI integration
        /// </summary>
        [HttpGet("datasources")]
        public async Task<IActionResult> GetAvailableDataSources()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var dataSources = await _powerBIService.GetAvailableDataSourcesAsync();
                return Ok(dataSources);
            });
        }

        /// <summary>
        /// Test connection to a specific data source
        /// </summary>
        [HttpPost("datasources/{dataSourceId}/test")]
        public async Task<IActionResult> TestDataConnection(string dataSourceId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var isConnected = await _powerBIService.TestDataConnectionAsync(dataSourceId);
                
                return Ok(new { 
                    DataSourceId = dataSourceId,
                    IsConnected = isConnected,
                    Message = isConnected ? "Connection successful" : "Connection failed",
                    TestedAt = DateTime.UtcNow
                });
            });
        }

        /// <summary>
        /// Get Power BI system health status
        /// </summary>
        [HttpGet("system/health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var healthInfo = await _powerBIService.GetSystemHealthAsync();
                return Ok(healthInfo);
            });
        }
    }
}