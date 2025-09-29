namespace MaritimeIQ.Platform.Models.Monitoring
{
    /// <summary>
    /// Overall system health status aggregating all monitoring components
    /// </summary>
    public class SystemHealthStatus
    {
        public DateTime Timestamp { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
        public List<ServiceHealthStatus> Services { get; set; } = new();
        public PerformanceMetrics Performance { get; set; } = new();
        public InfrastructureStatus Infrastructure { get; set; } = new();
        public List<DependencyStatus> Dependencies { get; set; } = new();
    }

    /// <summary>
    /// Health status for individual services in the maritime platform
    /// </summary>
    public class ServiceHealthStatus
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
        public string Version { get; set; } = string.Empty;
        public TimeSpan Uptime { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Core performance metrics for the system
    /// </summary>
    public class PerformanceMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public double NetworkThroughput { get; set; }
        public int RequestsPerSecond { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public double ErrorRate { get; set; }
        public int ActiveConnections { get; set; }
        public TimeSpan HealthCheckDuration { get; set; }
    }

    /// <summary>
    /// Status of external dependencies
    /// </summary>
    public class DependencyStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
        public string Endpoint { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic time series data point for metrics
    /// </summary>
    public class TimeSeriesDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }
}