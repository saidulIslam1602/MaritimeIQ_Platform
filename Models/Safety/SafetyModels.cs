using System;
using System.Collections.Generic;

namespace MaritimeIQ.Platform.Models.Safety
{
    /// <summary>
    /// Enumeration for safety alert severity levels
    /// </summary>
    public enum AlertSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Enumeration for safety status levels
    /// </summary>
    public enum SafetyStatus
    {
        Safe,
        Caution,
        Critical
    }

    /// <summary>
    /// Unified safety alert model for all maritime safety scenarios
    /// </summary>
    public class SafetyAlert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = AlertSeverity.Medium.ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string Priority { get; set; } = "Normal";
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Safety incident report model
    /// </summary>
    public class SafetyIncidentReport
    {
        public string IncidentId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public string IncidentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string ReportedBy { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<string> AffectedPersonnel { get; set; } = new();
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Safety incident response model
    /// </summary>
    public class SafetyIncidentResponse
    {
        public string IncidentId { get; set; } = Guid.NewGuid().ToString();
        public string Status { get; set; } = "Reported";
        public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public DateTime? EstimatedResolutionTime { get; set; }
    }

    /// <summary>
    /// Comprehensive safety metrics for fleet operations
    /// </summary>
    public class SafetyMetrics
    {
        public int TotalIncidents { get; set; }
        public int CriticalIncidents { get; set; }
        public double IncidentRate { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public double ComplianceScore { get; set; }
        public int SafetyIncidents { get; set; }
        public int NearMisses { get; set; }
        public double SafetyScore { get; set; }
        public int TrainingCompliance { get; set; }
        public int EquipmentFailures { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, double> AdditionalMetrics { get; set; } = new();
    }

    /// <summary>
    /// Safety performance summary for analytics
    /// </summary>
    public class SafetySummary
    {
        public double OverallSafetyScore { get; set; }
        public int TotalIncidents { get; set; }
        public int IncidentsThisMonth { get; set; }
        public int SafetyViolations { get; set; }
        public double SafetyScore { get; set; }
        public string Status { get; set; } = "Good";
        public string TrendDirection { get; set; } = "Stable";
        public DateTime LastSafetyAudit { get; set; }
        public List<string> RecentIncidents { get; set; } = new();
        public Dictionary<string, int> IncidentsByCategory { get; set; } = new();
    }

    /// <summary>
    /// Safety dashboard summary
    /// </summary>
    public class SafetyDashboard
    {
        public int ActiveIncidents { get; set; }
        public int TotalAlertsToday { get; set; }
        public SafetyStatus OverallStatus { get; set; } = SafetyStatus.Safe;
        public List<SafetyAlert> RecentAlerts { get; set; } = new();
        public SafetyMetrics Metrics { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<string> RecentActions { get; set; } = new();
        public Dictionary<string, int> IncidentsByType { get; set; } = new();
    }
}