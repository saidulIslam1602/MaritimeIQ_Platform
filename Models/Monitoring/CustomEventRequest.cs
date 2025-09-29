namespace MaritimeIQ.Platform.Models.Monitoring
{
    /// <summary>
    /// Request model for tracking custom events in monitoring system
    /// </summary>
    public class CustomEventRequest
    {
        public string EventName { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
        public Dictionary<string, object> Metrics { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
        public DateTime? EventTime { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for creating health alerts
    /// </summary>
    public class CreateAlertRequest
    {
        public string AlertName { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public string Description { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}