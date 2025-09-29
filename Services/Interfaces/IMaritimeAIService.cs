namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for AI-powered maritime operations and decision support
    /// </summary>
    public interface IMaritimeAIService
    {
        /// <summary>
        /// Get AI-powered recommendations for fleet optimization
        /// </summary>
        Task<AIRecommendationResult> GetFleetOptimizationRecommendationsAsync();

        /// <summary>
        /// Analyze vessel performance using machine learning
        /// </summary>
        Task<AIPerformanceAnalysis> AnalyzeVesselPerformanceAsync(string vesselId);

        /// <summary>
        /// Predict maintenance requirements using AI
        /// </summary>
        Task<MaintenancePrediction> PredictMaintenanceRequirementsAsync(string vesselId);

        /// <summary>
        /// Generate AI-powered route recommendations
        /// </summary>
        Task<RouteRecommendation> GenerateRouteRecommendationAsync(string startPort, string endPort, DateTime departureTime);

        /// <summary>
        /// Analyze passenger satisfaction using natural language processing
        /// </summary>
        Task<SentimentAnalysisResult> AnalyzePassengerFeedbackAsync(List<string> feedbackTexts);
    }

    public class AIRecommendationResult
    {
        public List<AIRecommendation> Recommendations { get; set; } = new();
        public double ConfidenceScore { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class AIRecommendation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double PotentialImpact { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class AIPerformanceAnalysis
    {
        public string VesselId { get; set; } = string.Empty;
        public double PerformanceScore { get; set; }
        public List<PerformanceInsight> Insights { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class PerformanceInsight
    {
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Impact { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    public class MaintenancePrediction
    {
        public string VesselId { get; set; } = string.Empty;
        public List<PredictedMaintenanceItem> PredictedItems { get; set; } = new();
        public double OverallRiskScore { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
    }

    public class PredictedMaintenanceItem
    {
        public string Component { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime PredictedDate { get; set; }
        public double Probability { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public double EstimatedCost { get; set; }
    }

    public class RouteRecommendation
    {
        public string StartPort { get; set; } = string.Empty;
        public string EndPort { get; set; } = string.Empty;
        public List<RouteOption> RecommendedRoutes { get; set; } = new();
        public WeatherConsiderations WeatherConsiderations { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class RouteOption
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public double FuelEfficiency { get; set; }
        public double SafetyScore { get; set; }
        public List<string> Waypoints { get; set; } = new();
        public string OptimizationCriteria { get; set; } = string.Empty;
    }

    public class WeatherConsiderations
    {
        public string OverallConditions { get; set; } = string.Empty;
        public List<WeatherAlert> Alerts { get; set; } = new();
        public double WeatherImpactScore { get; set; }
    }

    public class WeatherAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }

    public class SentimentAnalysisResult
    {
        public double OverallSentiment { get; set; }
        public List<SentimentItem> SentimentItems { get; set; } = new();
        public Dictionary<string, int> TopicFrequency { get; set; } = new();
        public List<string> KeyInsights { get; set; } = new();
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class SentimentItem
    {
        public string Text { get; set; } = string.Empty;
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; } = string.Empty;
        public List<string> Topics { get; set; } = new();
        public double Confidence { get; set; }
    }
}