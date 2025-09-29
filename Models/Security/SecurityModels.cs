namespace MaritimeIQ.Platform.Models.Security
{
    /// <summary>
    /// Request model for reporting security incidents
    /// </summary>
    public class SecurityIncidentRequest
    {
        public string IncidentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public string ReportedBy { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string AdditionalDetails { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Security event model for logging and tracking
    /// </summary>
    public class SecurityEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Details { get; set; } = new();
        public string Status { get; set; } = "Active";
        public string Source { get; set; } = "System";
    }

    /// <summary>
    /// Login attempt tracking model
    /// </summary>
    public class LoginAttempt
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    /// <summary>
    /// Blocked IP address model
    /// </summary>
    public class BlockedIpAddress
    {
        public string IpAddress { get; set; } = string.Empty;
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
        public string Reason { get; set; } = string.Empty;
        public string BlockedBy { get; set; } = "System";
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Security dashboard summary model
    /// </summary>
    public class SecurityDashboard
    {
        public SecurityMetrics Metrics { get; set; } = new();
        public List<SecurityEvent> RecentEvents { get; set; } = new();
        public List<SecurityAlert> ActiveAlerts { get; set; } = new();
        public SecurityHealth Health { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Security metrics model
    /// </summary>
    public class SecurityMetrics
    {
        public int TotalSecurityEvents { get; set; }
        public int CriticalAlerts { get; set; }
        public int FailedLoginAttempts { get; set; }
        public int BlockedIpAddresses { get; set; }
        public int SuspiciousActivities { get; set; }
        public double SecurityScore { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Security alert model
    /// </summary>
    public class SecurityAlert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public string ResolvedBy { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
    }

    /// <summary>
    /// Security health model
    /// </summary>
    public class SecurityHealth
    {
        public string OverallStatus { get; set; } = "Healthy";
        public List<SecurityHealthCheck> Checks { get; set; } = new();
        public DateTime LastCheck { get; set; } = DateTime.UtcNow;
        public int Score { get; set; } = 100;
    }

    /// <summary>
    /// Security health check model
    /// </summary>
    public class SecurityHealthCheck
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Suspicious activity model
    /// </summary>
    public class SuspiciousActivity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public string RiskLevel { get; set; } = "Medium";
        public Dictionary<string, object> Details { get; set; } = new();
        public string Status { get; set; } = "Under Review";
    }
}