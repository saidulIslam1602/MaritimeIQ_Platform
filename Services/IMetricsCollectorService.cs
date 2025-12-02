using MaritimeIQ.Platform.Models.Monitoring;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Interface for collecting and reporting real application metrics
    /// </summary>
    public interface IMetricsCollectorService
    {
        /// <summary>
        /// Increment counter for a specific event type
        /// </summary>
        void IncrementEventCounter(string eventType, int count = 1);

        /// <summary>
        /// Get the number of events processed in the last hour
        /// </summary>
        long GetEventsPerHour();

        /// <summary>
        /// Get the current events per second rate
        /// </summary>
        double GetEventsPerSecond();

        /// <summary>
        /// Get comprehensive throughput metrics
        /// </summary>
        ThroughputMetrics GetThroughputMetrics();

        /// <summary>
        /// Get formatted throughput string (e.g., "900K events/hour")
        /// </summary>
        string GetFormattedThroughput();

        /// <summary>
        /// Get application uptime percentage
        /// </summary>
        double GetUptimePercentage();

        /// <summary>
        /// Get comprehensive uptime metrics
        /// </summary>
        UptimeMetrics GetUptimeMetrics();

        /// <summary>
        /// Get current application uptime
        /// </summary>
        TimeSpan GetCurrentUptime();

        /// <summary>
        /// Record data quality score for a specific layer/category
        /// </summary>
        void RecordDataQuality(string category, double score);

        /// <summary>
        /// Get data quality score for a specific category
        /// </summary>
        double GetDataQualityScore(string category);

        /// <summary>
        /// Get comprehensive data quality metrics
        /// </summary>
        DataQualityMetrics GetDataQualityMetrics();

        /// <summary>
        /// Increment API request counter
        /// </summary>
        void IncrementRequestCounter(string endpoint);

        /// <summary>
        /// Get total requests for an endpoint
        /// </summary>
        long GetRequestCount(string endpoint);

        /// <summary>
        /// Get total requests across all endpoints
        /// </summary>
        long GetTotalRequests();

        /// <summary>
        /// Clean up old metrics data (for sliding window)
        /// </summary>
        void CleanupOldMetrics();
    }
}

