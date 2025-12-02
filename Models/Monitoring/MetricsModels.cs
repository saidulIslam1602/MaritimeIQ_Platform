namespace MaritimeIQ.Platform.Models.Monitoring
{
    /// <summary>
    /// Real-time throughput metrics calculated from actual event processing
    /// </summary>
    public class ThroughputMetrics
    {
        public long EventsProcessedTotal { get; set; }
        public long EventsLastHour { get; set; }
        public double EventsPerSecond { get; set; }
        public double EventsPerMinute { get; set; }
        public string FormattedThroughput { get; set; } = string.Empty;
        public DateTime LastCalculated { get; set; }
    }

    /// <summary>
    /// Application uptime metrics based on actual start time
    /// </summary>
    public class UptimeMetrics
    {
        public DateTime StartTime { get; set; }
        public TimeSpan Uptime { get; set; }
        public double UptimePercentage { get; set; }
        public int TotalRestarts { get; set; }
        public DateTime? LastRestartTime { get; set; }
    }

    /// <summary>
    /// Data quality metrics from actual validation results
    /// </summary>
    public class DataQualityMetrics
    {
        public double CompletionRate { get; set; }
        public double AccuracyScore { get; set; }
        public double TimelinessScore { get; set; }
        public double ConsistencyScore { get; set; }
        public int ValidationErrors { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Event tracking entry for sliding window calculations
    /// </summary>
    public class EventEntry
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// System resource metrics from actual process data
    /// </summary>
    public class SystemResourceMetrics
    {
        public double CpuUsagePercent { get; set; }
        public long MemoryUsageBytes { get; set; }
        public double MemoryUsagePercent { get; set; }
        public int ThreadCount { get; set; }
        public long WorkingSetBytes { get; set; }
        public DateTime Collected { get; set; }
    }
}

