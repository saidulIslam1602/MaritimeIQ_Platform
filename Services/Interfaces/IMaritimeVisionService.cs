using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for maritime computer vision and AI-powered image analysis
    /// </summary>
    public interface IMaritimeVisionService
    {
        /// <summary>
        /// Analyze maritime imagery for vessel detection and identification
        /// </summary>
        Task<VisionAnalysisResult> AnalyzeMaritimeImageAsync(string imageUrl, string analysisType);

        /// <summary>
        /// Process real-time camera feeds from vessels for safety monitoring
        /// </summary>
        Task<List<SafetyViolation>> ProcessCameraFeedAsync(string vesselId, Stream imageStream);

        /// <summary>
        /// Detect weather conditions from maritime imagery
        /// </summary>
        Task<WeatherConditionResult> DetectWeatherConditionsAsync(string imageUrl);

        /// <summary>
        /// Identify marine wildlife for environmental compliance
        /// </summary>
        Task<WildlifeDetectionResult> DetectMarineWildlifeAsync(string imageUrl);

        /// <summary>
        /// Analyze Northern Lights visibility from vessel cameras
        /// </summary>
        Task<NorthernLightsVisibilityResult> AnalyzeNorthernLightsVisibilityAsync(string imageUrl);

        /// <summary>
        /// Process port approach imagery for docking assistance
        /// </summary>
        Task<DockingAssistanceResult> ProcessPortApproachImageryAsync(string vesselId, string portId, Stream imageStream);

        /// <summary>
        /// Generate maritime safety reports from visual analysis
        /// </summary>
        Task<SafetyReport> GenerateVisualSafetyReportAsync(string vesselId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Detect obstacles and hazards in maritime environment
        /// </summary>
        Task<HazardDetectionResult> DetectMaritimeHazardsAsync(string imageUrl);
    }

    #region Supporting Models

    public class VisionAnalysisResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AnalysisType { get; set; } = string.Empty;
        public List<DetectedObject> DetectedObjects { get; set; } = new();
        public double Confidence { get; set; }
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class DetectedObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public BoundingBox BoundingBox { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class BoundingBox
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class SafetyViolation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class WeatherConditionResult
    {
        public string Condition { get; set; } = string.Empty;
        public double Visibility { get; set; }
        public string SeaState { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class WildlifeDetectionResult
    {
        public List<DetectedWildlife> DetectedAnimals { get; set; } = new();
        public string RecommendedAction { get; set; } = string.Empty;
        public bool RequiresSlowdown { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }

    public class DetectedWildlife
    {
        public string Species { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public BoundingBox Location { get; set; } = new();
        public string ProtectionLevel { get; set; } = string.Empty;
    }

    public class NorthernLightsVisibilityResult
    {
        public bool IsVisible { get; set; }
        public double Intensity { get; set; }
        public string Quality { get; set; } = string.Empty;
        public string RecommendedViewingLocation { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class DockingAssistanceResult
    {
        public string PortId { get; set; } = string.Empty;
        public string VesselId { get; set; } = string.Empty;
        public double DistanceToPort { get; set; }
        public string RecommendedApproachAngle { get; set; } = string.Empty;
        public List<string> Hazards { get; set; } = new();
        public string DockingRecommendation { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class SafetyReport
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<SafetyViolation> Violations { get; set; } = new();
        public int TotalIncidents { get; set; }
        public string OverallSafetyScore { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class HazardDetectionResult
    {
        public List<MaritimeHazard> DetectedHazards { get; set; } = new();
        public string OverallRiskLevel { get; set; } = "Low";
        public List<string> RecommendedActions { get; set; } = new();
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }

    public class MaritimeHazard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = "Low";
        public BoundingBox Location { get; set; } = new();
        public double DistanceFromVessel { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    #endregion
}