namespace MaritimeIQ.Platform.Models.Monitoring
{
    /// <summary>
    /// Application Insights metrics and analytics
    /// </summary>
    public class ApplicationInsightsMetrics
    {
        public string TimeFrame { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public RequestMetrics RequestMetrics { get; set; } = new();
        public ExceptionMetrics ExceptionMetrics { get; set; } = new();
        public DependencyMetrics DependencyMetrics { get; set; } = new();
        public UserMetrics UserMetrics { get; set; } = new();
        public PerformanceCounters PerformanceCounters { get; set; } = new();
    }

    /// <summary>
    /// HTTP request metrics from Application Insights
    /// </summary>
    public class RequestMetrics
    {
        public long TotalRequests { get; set; }
        public long SuccessfulRequests { get; set; }
        public long FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public double P95ResponseTime { get; set; }
        public double P99ResponseTime { get; set; }
    }

    /// <summary>
    /// Exception tracking metrics
    /// </summary>
    public class ExceptionMetrics
    {
        public int TotalExceptions { get; set; }
        public int UniqueExceptions { get; set; }
        public int CriticalExceptions { get; set; }
        public List<ExceptionInfo> RecentExceptions { get; set; } = new();
    }

    /// <summary>
    /// Individual exception information
    /// </summary>
    public class ExceptionInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime FirstOccurrence { get; set; }
        public DateTime LastOccurrence { get; set; }
    }

    /// <summary>
    /// External dependency call metrics
    /// </summary>
    public class DependencyMetrics
    {
        public long TotalDependencyCalls { get; set; }
        public long SuccessfulCalls { get; set; }
        public long FailedCalls { get; set; }
        public double AverageDuration { get; set; }
        public List<DependencyInfo> MostFrequentDependencies { get; set; } = new();
    }

    /// <summary>
    /// Individual dependency information
    /// </summary>
    public class DependencyInfo
    {
        public string Name { get; set; } = string.Empty;
        public long CallCount { get; set; }
        public double AverageDuration { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// User activity and session metrics
    /// </summary>
    public class UserMetrics
    {
        public int ActiveUsers { get; set; }
        public int NewUsers { get; set; }
        public int ReturningUsers { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
        public long PageViews { get; set; }
        public long UniquePageViews { get; set; }
    }

    /// <summary>
    /// System performance counters
    /// </summary>
    public class PerformanceCounters
    {
        public double ProcessorTime { get; set; }
        public double AvailableMemory { get; set; }
        public double RequestsPerSecond { get; set; }
        public double RequestExecutionTime { get; set; }
        public int RequestsInApplicationQueue { get; set; }
    }

    /// <summary>
    /// Availability test summary and results
    /// </summary>
    public class AvailabilityTestSummary
    {
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int WarningTests { get; set; }
        public double OverallAvailability { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public List<AvailabilityTestResult> Tests { get; set; } = new();
    }

    /// <summary>
    /// Individual availability test result
    /// </summary>
    public class AvailabilityTestResult
    {
        public string TestName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime LastRun { get; set; }
        public double SuccessRate24h { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}