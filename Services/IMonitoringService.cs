using MaritimeIQ.Platform.Models.Monitoring;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Interface for monitoring and system health services
    /// </summary>
    public interface IMonitoringService
    {
        Task<SystemHealthStatus> GetSystemHealthAsync();
        Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync(string timeFrame);
        Task<ApplicationInsightsMetrics> GetApplicationInsightsMetricsAsync(string timeFrame);
        Task<AlertSummary> GetAlertsAsync();
        Task<LogSummary> GetLogsAsync(string timeRange, string logLevel);
        Task<AvailabilityTestSummary> GetAvailabilityTestsAsync();
        Task TrackCustomEventAsync(CustomEventRequest customEvent);
        Task<bool> CreateHealthAlertAsync(string alertName, string condition, string severity);
        Task<object> GetHealthDashboardDataAsync();
    }
}