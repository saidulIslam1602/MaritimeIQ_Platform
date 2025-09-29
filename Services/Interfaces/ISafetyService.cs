using MaritimeIQ.Platform.Models.Safety;

namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for maritime safety operations and incident management
    /// </summary>
    public interface ISafetyService
    {
        /// <summary>
        /// Get safety dashboard with current status and metrics
        /// </summary>
        Task<SafetyDashboard> GetSafetyDashboardAsync();

        /// <summary>
        /// Report a safety incident
        /// </summary>
        Task<SafetyIncidentResponse> ReportSafetyIncidentAsync(SafetyIncidentReport incident);

        /// <summary>
        /// Get safety metrics for a specified timeframe
        /// </summary>
        Task<SafetyMetrics> GetSafetyMetricsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get active safety alerts
        /// </summary>
        Task<List<SafetyAlert>> GetActiveSafetyAlertsAsync();
    }
}