using System.ComponentModel.DataAnnotations;

namespace MaritimeIQ.Platform.Models.Incident
{
    /// <summary>
    /// Incident severity levels following industry standards
    /// </summary>
    public enum IncidentSeverity
    {
        /// <summary>
        /// Complete service outage, data loss, security breach
        /// </summary>
        Critical = 1,
        
        /// <summary>
        /// Partial service degradation, high error rates
        /// </summary>
        High = 2,
        
        /// <summary>
        /// Minor service impact, isolated issues
        /// </summary>
        Medium = 3,
        
        /// <summary>
        /// Minimal impact, cosmetic issues
        /// </summary>
        Low = 4,
        
        /// <summary>
        /// Informational alerts
        /// </summary>
        Info = 5
    }

    /// <summary>
    /// Incident status throughout its lifecycle
    /// </summary>
    public enum IncidentStatus
    {
        /// <summary>
        /// Incident has been triggered but not acknowledged
        /// </summary>
        Triggered,
        
        /// <summary>
        /// Incident acknowledged by on-call engineer
        /// </summary>
        Acknowledged,
        
        /// <summary>
        /// Investigation in progress
        /// </summary>
        Investigating,
        
        /// <summary>
        /// Mitigation being applied
        /// </summary>
        Mitigating,
        
        /// <summary>
        /// Incident resolved
        /// </summary>
        Resolved,
        
        /// <summary>
        /// Post-mortem completed
        /// </summary>
        Closed
    }

    /// <summary>
    /// Maritime-specific incident categories
    /// </summary>
    public enum IncidentCategory
    {
        /// <summary>
        /// System infrastructure issues
        /// </summary>
        Infrastructure,
        
        /// <summary>
        /// Application performance issues
        /// </summary>
        Performance,
        
        /// <summary>
        /// Security-related incidents
        /// </summary>
        Security,
        
        /// <summary>
        /// Data quality or pipeline issues
        /// </summary>
        DataQuality,
        
        /// <summary>
        /// Vessel tracking failures
        /// </summary>
        VesselTracking,
        
        /// <summary>
        /// Environmental compliance violations
        /// </summary>
        Environmental,
        
        /// <summary>
        /// Emergency response system failures
        /// </summary>
        Emergency,
        
        /// <summary>
        /// Third-party service failures
        /// </summary>
        External
    }

    /// <summary>
    /// Core incident model
    /// </summary>
    public class Incident
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public IncidentSeverity Severity { get; set; }
        
        public IncidentStatus Status { get; set; } = IncidentStatus.Triggered;
        
        public IncidentCategory Category { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? AcknowledgedAt { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public DateTime? ClosedAt { get; set; }
        
        public string? AssignedTo { get; set; }
        
        public string? PagerDutyIncidentId { get; set; }
        
        public string Source { get; set; } = "MaritimeIQ Platform";
        
        public Dictionary<string, object> CustomDetails { get; set; } = new();
        
        public List<IncidentUpdate> Updates { get; set; } = new();
        
        public List<string> AffectedServices { get; set; } = new();
        
        public int? AffectedUsers { get; set; }
        
        public decimal? BusinessImpact { get; set; }
        
        /// <summary>
        /// Time to acknowledge (MTTA)
        /// </summary>
        public TimeSpan? TimeToAcknowledge => AcknowledgedAt?.Subtract(CreatedAt);
        
        /// <summary>
        /// Time to resolve (MTTR)
        /// </summary>
        public TimeSpan? TimeToResolve => ResolvedAt?.Subtract(CreatedAt);
        
        /// <summary>
        /// Total incident duration
        /// </summary>
        public TimeSpan? Duration => (ResolvedAt ?? DateTime.UtcNow).Subtract(CreatedAt);
    }

    /// <summary>
    /// Incident update/timeline entry
    /// </summary>
    public class IncidentUpdate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string IncidentId { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string UpdatedBy { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public IncidentStatus? StatusChange { get; set; }
        
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// On-call engineer information
    /// </summary>
    public class OnCallEngineer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? Phone { get; set; }
        
        public string? PagerDutyUserId { get; set; }
        
        public string? SlackUserId { get; set; }
        
        public List<string> Skills { get; set; } = new();
        
        public string TimeZone { get; set; } = "UTC";
        
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// On-call schedule entry
    /// </summary>
    public class OnCallSchedule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string EngineerId { get; set; } = string.Empty;
        
        public OnCallEngineer? Engineer { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }
        
        public string Role { get; set; } = "Primary"; // Primary, Secondary, Manager
        
        public bool IsActive { get; set; } = true;
        
        public List<string> EscalationContacts { get; set; } = new();
    }

    /// <summary>
    /// Escalation policy configuration
    /// </summary>
    public class EscalationPolicy
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Name { get; set; } = string.Empty;
        
        public List<EscalationLevel> Levels { get; set; } = new();
        
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Individual escalation level
    /// </summary>
    public class EscalationLevel
    {
        public int Level { get; set; }
        
        public TimeSpan EscalationDelay { get; set; }
        
        public List<string> ContactIds { get; set; } = new();
        
        public List<string> NotificationMethods { get; set; } = new(); // email, sms, phone, push
    }

    /// <summary>
    /// Post-mortem analysis
    /// </summary>
    public class PostMortem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string IncidentId { get; set; } = string.Empty;
        
        public Incident? Incident { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public string Summary { get; set; } = string.Empty;
        
        public string RootCause { get; set; } = string.Empty;
        
        public List<string> ContributingFactors { get; set; } = new();
        
        public string Resolution { get; set; } = string.Empty;
        
        public List<string> WhatWentWell { get; set; } = new();
        
        public List<string> WhatWentWrong { get; set; } = new();
        
        public List<string> WhereWeGotLucky { get; set; } = new();
        
        public List<ActionItem> ActionItems { get; set; } = new();
        
        public List<string> RelatedIncidents { get; set; } = new();
        
        public bool IsPublished { get; set; } = false;
    }

    /// <summary>
    /// Action item from post-mortem
    /// </summary>
    public class ActionItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Description { get; set; } = string.Empty;
        
        public string AssignedTo { get; set; } = string.Empty;
        
        public DateTime DueDate { get; set; }
        
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        public string Status { get; set; } = "Open"; // Open, In Progress, Completed, Cancelled
        
        public DateTime? CompletedAt { get; set; }
        
        public string? CompletionNotes { get; set; }
    }

    /// <summary>
    /// Real-time incident metrics for dashboards
    /// </summary>
    public class IncidentMetrics
    {
        public int TotalIncidents { get; set; }
        
        public int ActiveIncidents { get; set; }
        
        public int CriticalIncidents { get; set; }
        
        public int HighSeverityIncidents { get; set; }
        
        public TimeSpan AverageMTTA { get; set; } // Mean Time To Acknowledge
        
        public TimeSpan AverageMTTR { get; set; } // Mean Time To Resolve
        
        public double IncidentRate { get; set; } // Incidents per day
        
        public Dictionary<IncidentCategory, int> IncidentsByCategory { get; set; } = new();
        
        public Dictionary<string, int> IncidentsByService { get; set; } = new();
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Maritime-specific incident context
    /// </summary>
    public class MaritimeIncidentContext
    {
        public List<string>? AffectedVessels { get; set; }
        
        public List<string>? AffectedRoutes { get; set; }
        
        public int? PassengersAffected { get; set; }
        
        public bool IsEnvironmentalImpact { get; set; }
        
        public bool IsEmergencyResponse { get; set; }
        
        public string? WeatherConditions { get; set; }
        
        public string? Location { get; set; }
        
        public Dictionary<string, double>? EnvironmentalMetrics { get; set; }
    }
}
