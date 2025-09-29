using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Services;
using MaritimeIQ.Platform.Models.Monitoring;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// Monitoring controller for system health, performance analytics, and operational insights
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : BaseMaritimeController
    {
        private readonly IMonitoringService _monitoringService;

        public MonitoringController(
            IMonitoringService monitoringService,
            ILogger<MonitoringController> logger)
            : base(logger)
        {
            _monitoringService = monitoringService;
        }

        /// <summary>
        /// Get comprehensive system health status
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            return await ExecuteOperationAsync(
                () => _monitoringService.GetSystemHealthAsync(),
                "GetSystemHealth"
            );
        }

        /// <summary>
        /// Get performance analytics for specified timeframe
        /// </summary>
        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceAnalytics([FromQuery] string timeFrame = "24h")
        {
            return await ExecuteOperationAsync(
                () => _monitoringService.GetPerformanceAnalyticsAsync(timeFrame),
                "GetPerformanceAnalytics"
            );
        }

        /// <summary>
        /// Get Application Insights metrics and telemetry data
        /// </summary>
        [HttpGet("insights")]
        public async Task<IActionResult> GetApplicationInsights([FromQuery] string timeFrame = "24h")
        {
            return await ExecuteOperationAsync(
                () => _monitoringService.GetApplicationInsightsMetricsAsync(timeFrame),
                "GetApplicationInsights"
            );
        }

        /// <summary>
        /// Get current alerts and warnings
        /// </summary>
        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlerts()
        {
            return await ExecuteOperationAsync(
                () => _monitoringService.GetAlertsAsync(),
                "GetAlerts"
            );
        }

        /// <summary>
        /// Get application logs with filtering
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] string timeRange = "1h", [FromQuery] string logLevel = "Info")
        {
            return await ExecuteOperationAsync(
                () => _monitoringService.GetLogsAsync(timeRange, logLevel),
                "GetLogs"
            );
        }

        /// <summary>
        /// Get availability test results
        /// </summary>
        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailabilityTests()
        {
            return await ExecuteOperationAsync(
                () => _monitoringService.GetAvailabilityTestsAsync(),
                "GetAvailabilityTests"
            );
        }

        /// <summary>
        /// Track custom event for analytics
        /// </summary>
        [HttpPost("events")]
        public async Task<IActionResult> TrackCustomEvent([FromBody] CustomEventRequest customEvent)
        {
            if (customEvent == null || string.IsNullOrEmpty(customEvent.EventName))
            {
                return BadRequest(new { error = "Invalid event request. EventName is required." });
            }

            return await ExecuteOperationAsync(async () =>
            {
                await _monitoringService.TrackCustomEventAsync(customEvent);
                return new { message = "Event tracked successfully", eventName = customEvent.EventName };
            }, "TrackCustomEvent");
        }

        /// <summary>
        /// Create health alert configuration
        /// </summary>
        [HttpPost("alerts")]
        public async Task<IActionResult> CreateHealthAlert([FromBody] CreateAlertRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.AlertName))
            {
                return BadRequest(new { error = "Invalid alert request. AlertName is required." });
            }

            return await ExecuteOperationAsync(async () =>
            {
                var success = await _monitoringService.CreateHealthAlertAsync(request.AlertName, request.Condition, request.Severity);
                
                if (success)
                {
                    return new { message = "Health alert created successfully", alertName = request.AlertName };
                }
                else
                {
                    throw new InvalidOperationException("Failed to create health alert");
                }
            }, "CreateHealthAlert");
        }

        /// <summary>
        /// Get comprehensive dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetHealthDashboard()
        {
            return await ExecuteOperationAsync(
                () => _monitoringService.GetHealthDashboardDataAsync(),
                "GetHealthDashboard"
            );
        }
    }
}