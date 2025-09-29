namespace MaritimeIQ.Platform.Models.ApiManagement
{
    // Extended API Management Models for comprehensive API management functionality

    public class SecurityPolicy
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<SecurityRule> Rules { get; set; } = new List<SecurityRule>();
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SecurityRule
    {
        public string Name { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    public class RateLimitPolicy
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int RequestsPerMinute { get; set; }
        public int RequestsPerHour { get; set; }
        public int RequestsPerDay { get; set; }
        public int BurstLimit { get; set; }
        public int WindowSizeMinutes { get; set; }
        public bool EnforcePerIP { get; set; }
        public bool EnforcePerUser { get; set; }
        public string OverageAction { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ApiUsageMetrics
    {
        public string ApiId { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double AverageLatency { get; set; }
        public int UniqueUsers { get; set; }
        public List<EndpointUsage> TopEndpoints { get; set; } = new List<EndpointUsage>();
    }

    public class EndpointUsage
    {
        public string Endpoint { get; set; } = string.Empty;
        public int RequestCount { get; set; }
    }

    public class SubscriptionUsageMetrics
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int RemainingQuota { get; set; }
        public double QuotaUsagePercentage { get; set; }
        public DateTime LastUsed { get; set; }
        public List<ApiUsageInfo> MostUsedApis { get; set; } = new List<ApiUsageInfo>();
    }

    public class ApiUsageInfo
    {
        public string ApiId { get; set; } = string.Empty;
        public int RequestCount { get; set; }
    }

    public class ApiAnalytics
    {
        public string ApiId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<RequestTrendData> RequestTrends { get; set; } = new List<RequestTrendData>();
        public ErrorAnalysisData ErrorAnalysis { get; set; } = new ErrorAnalysisData();
        public ApiPerformanceMetrics PerformanceMetrics { get; set; } = new ApiPerformanceMetrics();
        public GeographicUsageData GeographicDistribution { get; set; } = new GeographicUsageData();
        public UserBehaviorData UserBehaviorAnalytics { get; set; } = new UserBehaviorData();
    }

    public class RequestTrendData
    {
        public DateTime Date { get; set; }
        public int RequestCount { get; set; }
        public double SuccessRate { get; set; }
        public double AverageLatency { get; set; }
    }

    public class ErrorAnalysisData
    {
        public int TotalErrors { get; set; }
        public Dictionary<string, int> ErrorsByType { get; set; } = new Dictionary<string, int>();
        public List<EndpointError> TopErrorEndpoints { get; set; } = new List<EndpointError>();
    }

    public class EndpointError
    {
        public string Endpoint { get; set; } = string.Empty;
        public int ErrorCount { get; set; }
    }

    public class ApiPerformanceMetrics
    {
        public double AverageLatency { get; set; }
        public double P95Latency { get; set; }
        public double P99Latency { get; set; }
        public double ThroughputPerSecond { get; set; }
        public int ConcurrentUsers { get; set; }
    }

    public class GeographicUsageData
    {
        public Dictionary<string, int> RequestsByCountry { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RequestsByRegion { get; set; } = new Dictionary<string, int>();
    }

    public class UserBehaviorData
    {
        public TimeSpan AverageSessionDuration { get; set; }
        public int AverageRequestsPerSession { get; set; }
        public double ReturnUserRate { get; set; }
        public List<int> PeakUsageHours { get; set; } = new List<int>();
        public List<EndpointUsage> MostUsedEndpoints { get; set; } = new List<EndpointUsage>();
    }

    public class ApiValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
        public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();
        public DateTime ValidatedAt { get; set; }
        public bool OpenApiCompliant { get; set; }
        public int SecurityScore { get; set; }
    }

    public class ValidationError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }

    public class ValidationWarning
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public class ApiTestResult
    {
        public string ApiId { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public bool Success { get; set; }
        public long ResponseSize { get; set; }
        public DateTime TestExecutedAt { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }

    public class ApiTestRequest
    {
        public string Method { get; set; } = "GET";
        public string Endpoint { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string? Body { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    public class ApiDocumentation
    {
        public string ApiId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public List<EndpointDocumentation> Endpoints { get; set; } = new List<EndpointDocumentation>();
        public List<ApiExample> Examples { get; set; } = new List<ApiExample>();
        public DateTime LastUpdated { get; set; }
    }

    public class EndpointDocumentation
    {
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ApiParameter> Parameters { get; set; } = new List<ApiParameter>();
        public List<ApiResponse> Responses { get; set; } = new List<ApiResponse>();
    }

    public class ApiParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? DefaultValue { get; set; }
    }

    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Schema { get; set; }
    }

    public class ApiExample
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Request { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
    }

    public class OpenApiSchema
    {
        public string OpenApi { get; set; } = "3.0.1";
        public OpenApiInfo Info { get; set; } = new OpenApiInfo();
        public List<OpenApiServer> Servers { get; set; } = new List<OpenApiServer>();
        public Dictionary<string, object> Paths { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Components { get; set; } = new Dictionary<string, object>();
    }

    public class OpenApiInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class OpenApiServer
    {
        public string Url { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ApiManagementDashboard
    {
        public int TotalApis { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TotalDevelopers { get; set; }
        public int RequestsToday { get; set; }
        public int RequestsThisMonth { get; set; }
        public double AverageLatency { get; set; }
        public double ErrorRate { get; set; }
        public List<ApiUsageInfo> TopApis { get; set; } = new List<ApiUsageInfo>();
        public List<ActivityLog> RecentActivity { get; set; } = new List<ActivityLog>();
        public DateTime LastUpdated { get; set; }
    }

    public class ActivityLog
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class ApiOverview
    {
        public string ApiId { get; set; } = string.Empty;
        public int RequestsToday { get; set; }
        public int RequestsThisWeek { get; set; }
        public int RequestsThisMonth { get; set; }
        public double AverageLatency { get; set; }
        public double ErrorRate { get; set; }
        public int ActiveSubscriptions { get; set; }
        public List<EndpointUsage> TopEndpoints { get; set; } = new List<EndpointUsage>();
        public List<RecentError> RecentErrors { get; set; } = new List<RecentError>();
        public string PerformanceTrend { get; set; } = string.Empty;
    }

    public class RecentError
    {
        public string Endpoint { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public int Count { get; set; }
        public DateTime LastOccurrence { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}