namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for maritime intelligence and advanced analytics
    /// </summary>
    public interface IMaritimeIntelligenceService
    {
        /// <summary>
        /// Get comprehensive maritime intelligence dashboard
        /// </summary>
        Task<MaritimeIntelligenceDashboard> GetIntelligenceDashboardAsync();

        /// <summary>
        /// Analyze vessel patterns and behaviors
        /// </summary>
        Task<VesselPatternAnalysis> AnalyzeVesselPatternsAsync(string vesselId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get predictive insights for fleet operations
        /// </summary>
        Task<PredictiveInsights> GetPredictiveInsightsAsync(string category);

        /// <summary>
        /// Generate maritime intelligence report
        /// </summary>
        Task<IntelligenceReport> GenerateIntelligenceReportAsync(IntelligenceReportRequest request);

        /// <summary>
        /// Get anomaly detection results
        /// </summary>
        Task<AnomalyDetectionResult> DetectAnomaliesAsync(string dataType, DateTime timeRange);
    }

    public class MaritimeIntelligenceDashboard
    {
        public List<IntelligenceWidget> Widgets { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class IntelligenceWidget
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
    }

    public class VesselPatternAnalysis
    {
        public string VesselId { get; set; } = string.Empty;
        public List<Pattern> Patterns { get; set; } = new();
        public List<string> Insights { get; set; } = new();
    }

    public class Pattern
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class PredictiveInsights
    {
        public string Category { get; set; } = string.Empty;
        public List<Prediction> Predictions { get; set; } = new();
        public double Accuracy { get; set; }
    }

    public class Prediction
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PredictedDate { get; set; }
        public double Probability { get; set; }
    }

    public class IntelligenceReport
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> KeyFindings { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class IntelligenceReportRequest
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string>? VesselIds { get; set; }
    }

    public class AnomalyDetectionResult
    {
        public List<Anomaly> Anomalies { get; set; } = new();
        public double AnomalyScore { get; set; }
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class Anomaly
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
    }
}