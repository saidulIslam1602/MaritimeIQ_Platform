namespace MaritimeIQ.Platform.Models.Monitoring
{
    /// <summary>
    /// Alert summary with categorized alerts
    /// </summary>
    public class AlertSummary
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int HighAlerts { get; set; }
        public int MediumAlerts { get; set; }
        public int LowAlerts { get; set; }
        public int RecentAlerts { get; set; }
        public List<Alert> Alerts { get; set; } = new();
    }

    /// <summary>
    /// Individual alert details
    /// </summary>
    public class Alert
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string Source { get; set; } = string.Empty;
        public string AffectedResource { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
    }

    /// <summary>
    /// Log summary with categorized log entries
    /// </summary>
    public class LogSummary
    {
        public object TimeRange { get; set; } = new();
        public int TotalLogs { get; set; }
        public int ErrorLogs { get; set; }
        public int WarningLogs { get; set; }
        public int InfoLogs { get; set; }
        public int DebugLogs { get; set; }
        public List<LogEntry> Logs { get; set; } = new();
    }

    /// <summary>
    /// Individual log entry
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}