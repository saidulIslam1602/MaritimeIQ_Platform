namespace MaritimeIQ.Platform.Models.Monitoring
{
    /// <summary>
    /// Performance analytics with time series data across different system components
    /// </summary>
    public class PerformanceAnalytics
    {
        public string TimeFrame { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public ApplicationPerformanceMetrics ApplicationMetrics { get; set; } = new();
        public InfrastructurePerformanceMetrics InfrastructureMetrics { get; set; } = new();
        public DatabasePerformanceMetrics DatabaseMetrics { get; set; } = new();
        public ExternalServicePerformanceMetrics ExternalServiceMetrics { get; set; } = new();
    }

    /// <summary>
    /// Application-level performance metrics over time
    /// </summary>
    public class ApplicationPerformanceMetrics
    {
        public List<TimeSeriesDataPoint> RequestsPerSecond { get; set; } = new();
        public List<TimeSeriesDataPoint> AverageResponseTime { get; set; } = new();
        public List<TimeSeriesDataPoint> ErrorRate { get; set; } = new();
        public List<TimeSeriesDataPoint> ThroughputMbps { get; set; } = new();
        public List<TimeSeriesDataPoint> ActiveUsers { get; set; } = new();
        public List<TimeSeriesDataPoint> ConcurrentConnections { get; set; } = new();
    }

    /// <summary>
    /// Infrastructure performance metrics over time
    /// </summary>
    public class InfrastructurePerformanceMetrics
    {
        public List<TimeSeriesDataPoint> CpuUtilization { get; set; } = new();
        public List<TimeSeriesDataPoint> MemoryUtilization { get; set; } = new();
        public List<TimeSeriesDataPoint> DiskIOPS { get; set; } = new();
        public List<TimeSeriesDataPoint> NetworkLatency { get; set; } = new();
        public List<TimeSeriesDataPoint> StorageUtilization { get; set; } = new();
        public List<TimeSeriesDataPoint> ContainerRestarts { get; set; } = new();
    }

    /// <summary>
    /// Database performance metrics over time
    /// </summary>
    public class DatabasePerformanceMetrics
    {
        public List<TimeSeriesDataPoint> ConnectionPoolUtilization { get; set; } = new();
        public List<TimeSeriesDataPoint> QueryExecutionTime { get; set; } = new();
        public List<TimeSeriesDataPoint> DeadlockCount { get; set; } = new();
        public List<TimeSeriesDataPoint> BlockedProcesses { get; set; } = new();
        public List<TimeSeriesDataPoint> DatabaseSize { get; set; } = new();
        public List<TimeSeriesDataPoint> IndexFragmentation { get; set; } = new();
    }

    /// <summary>
    /// External service performance metrics over time
    /// </summary>
    public class ExternalServicePerformanceMetrics
    {
        public List<TimeSeriesDataPoint> AzureOpenAILatency { get; set; } = new();
        public List<TimeSeriesDataPoint> CognitiveServicesLatency { get; set; } = new();
        public List<TimeSeriesDataPoint> StorageLatency { get; set; } = new();
        public List<TimeSeriesDataPoint> KeyVaultLatency { get; set; } = new();
        public List<TimeSeriesDataPoint> ExternalAPISuccessRate { get; set; } = new();
    }
}