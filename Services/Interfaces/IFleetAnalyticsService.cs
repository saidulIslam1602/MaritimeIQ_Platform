using MaritimeIQ.Platform.Models;
using MaritimeIQ.Platform.Models.FleetAnalytics;
using MaritimeIQ.Platform.Models.Safety;

namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for comprehensive fleet analytics and performance monitoring
    /// </summary>
    public interface IFleetAnalyticsService
    {
        /// <summary>
        /// Get comprehensive fleet performance analytics
        /// </summary>
        Task<MaritimeIQ.Platform.Models.FleetAnalytics.FleetPerformanceAnalytics> GetFleetPerformanceAnalyticsAsync(MaritimeIQ.Platform.Models.FleetAnalytics.AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get individual vessel performance metrics
        /// </summary>
        Task<MaritimeIQ.Platform.Models.FleetAnalytics.VesselPerformanceMetrics> GetVesselPerformanceAsync(string vesselId, MaritimeIQ.Platform.Models.FleetAnalytics.AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get fleet fuel efficiency analytics
        /// </summary>
        Task<FuelEfficiencyAnalytics> GetFuelEfficiencyAnalyticsAsync(AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get environmental compliance analytics for the fleet
        /// </summary>
        Task<EnvironmentalComplianceAnalytics> GetEnvironmentalComplianceAnalyticsAsync(AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get passenger experience and satisfaction analytics
        /// </summary>
        Task<PassengerExperienceAnalytics> GetPassengerExperienceAnalyticsAsync(AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get route efficiency and optimization analytics
        /// </summary>
        Task<RouteEfficiencyAnalytics> GetRouteEfficiencyAnalyticsAsync(string? routeId = null, AnalyticsTimeRange? timeRange = null);

        /// <summary>
        /// Get fleet safety and incident analytics
        /// </summary>
        Task<SafetyAnalytics> GetSafetyAnalyticsAsync(AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get predictive maintenance analytics
        /// </summary>
        Task<PredictiveMaintenanceAnalytics> GetPredictiveMaintenanceAnalyticsAsync(string? vesselId = null);

        /// <summary>
        /// Get weather impact analytics on fleet operations
        /// </summary>
        Task<WeatherImpactAnalytics> GetWeatherImpactAnalyticsAsync(AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get Northern Lights viewing opportunity analytics
        /// </summary>
        Task<NorthernLightsAnalytics> GetNorthernLightsAnalyticsAsync(AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get fleet utilization and capacity analytics
        /// </summary>
        Task<FleetUtilizationAnalytics> GetFleetUtilizationAnalyticsAsync(AnalyticsTimeRange timeRange);

        /// <summary>
        /// Get comparative fleet benchmarking analytics
        /// </summary>
        Task<FleetBenchmarkingAnalytics> GetFleetBenchmarkingAnalyticsAsync(AnalyticsTimeRange timeRange);

        /// <summary>
        /// Generate custom analytics report based on specified parameters
        /// </summary>
        Task<CustomAnalyticsReport> GenerateCustomAnalyticsReportAsync(CustomAnalyticsRequest request);

        /// <summary>
        /// Get real-time fleet dashboard data
        /// </summary>
        Task<FleetDashboardData> GetFleetDashboardDataAsync();

        /// <summary>
        /// Export analytics data for external analysis
        /// </summary>
        Task<AnalyticsExportResult> ExportAnalyticsDataAsync(AnalyticsExportRequest request);
    }

    #region Analytics Models

    public class AnalyticsTimeRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Granularity { get; set; } = "Daily"; // Hourly, Daily, Weekly, Monthly
    }

    public class FleetPerformanceAnalytics
    {
        public FleetSummary Summary { get; set; } = new();
        public List<VesselPerformanceSummary> VesselPerformances { get; set; } = new();
        public List<PerformanceTrend> Trends { get; set; } = new();
        public List<PerformanceKPI> KPIs { get; set; } = new();
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class FleetSummary
    {
        public int TotalVessels { get; set; }
        public int ActiveVessels { get; set; }
        public double TotalDistanceCovered { get; set; }
        public double AverageSpeed { get; set; }
        public double FleetUtilization { get; set; }
        public string OverallStatus { get; set; } = "Operational";
    }

    public class VesselPerformanceMetrics
    {
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public OperationalMetrics Operational { get; set; } = new();
        public EfficiencyMetrics Efficiency { get; set; } = new();
        public SafetyMetrics Safety { get; set; } = new();
        public EnvironmentalMetrics Environmental { get; set; } = new();
        public PassengerMetrics Passenger { get; set; } = new();
        public List<PerformanceAlert> Alerts { get; set; } = new();
        public double OverallScore { get; set; }
    }

    public class OperationalMetrics
    {
        public double TotalDistanceCovered { get; set; }
        public double AverageSpeed { get; set; }
        public TimeSpan TotalOperatingTime { get; set; }
        public double OnTimePerformance { get; set; }
        public int PortCalls { get; set; }
        public double RouteCompliance { get; set; }
    }

    public class EfficiencyMetrics
    {
        public double FuelEfficiency { get; set; }
        public double PowerEfficiency { get; set; }
        public double MaintenanceEfficiency { get; set; }
        public double CostPerNauticalMile { get; set; }
        public double CapacityUtilization { get; set; }
    }

    public class EnvironmentalMetrics
    {
        public double CO2Emissions { get; set; }
        public double NOxEmissions { get; set; }
        public double SOxEmissions { get; set; }
        public double WasteGenerated { get; set; }
        public double RecyclingRate { get; set; }
        public double ComplianceScore { get; set; }
    }

    public class PassengerMetrics
    {
        public int TotalPassengers { get; set; }
        public double SatisfactionScore { get; set; }
        public int Complaints { get; set; }
        public int Compliments { get; set; }
        public double ServiceRating { get; set; }
        public int NorthernLightsViewings { get; set; }
    }

    public class FuelEfficiencyAnalytics
    {
        public FuelSummary Summary { get; set; } = new();
        public FleetFuelSummary FleetSummary { get; set; } = new();
        public List<VesselFuelEfficiency> VesselEfficiencies { get; set; } = new();
        public List<FuelConsumptionTrend> Trends { get; set; } = new();
        public List<FuelOptimizationRecommendation> Recommendations { get; set; } = new();
        public double PotentialSavings { get; set; }
    }

    public class FleetFuelSummary
    {
        public double TotalFuelConsumed { get; set; }
        public double AverageEfficiency { get; set; }
        public double CostSavingsAchieved { get; set; }
        public double EmissionReduction { get; set; }
        public string EfficiencyTrend { get; set; } = string.Empty;
    }

    public class EnvironmentalComplianceAnalytics
    {
        public ComplianceSummary Summary { get; set; } = new();
        public List<VesselComplianceStatus> VesselCompliance { get; set; } = new();
        public List<ComplianceViolation> Violations { get; set; } = new();
        public List<ComplianceTrend> Trends { get; set; } = new();
        public double OverallComplianceScore { get; set; }
        public List<string> ImprovementActions { get; set; } = new();
    }

    public class PassengerExperienceAnalytics
    {
        public PassengerSummary Summary { get; set; } = new();
        public List<RouteExperienceMetrics> RouteExperiences { get; set; } = new();
        public List<ServiceMetrics> ServiceMetrics { get; set; } = new();
        public List<ExperienceInsight> Insights { get; set; } = new();
        public List<string> ImprovementOpportunities { get; set; } = new();
    }

    public class RouteEfficiencyAnalytics
    {
        public string? RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public RouteSummary Summary { get; set; } = new();
        public List<RoutePerformanceMetrics> Performances { get; set; } = new();
        public List<RouteOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
        public WeatherImpactSummary WeatherImpact { get; set; } = new();
    }

    public class SafetyAnalytics
    {
        public MaritimeIQ.Platform.Models.Safety.SafetySummary Summary { get; set; } = new();
        public List<IncidentAnalysis> IncidentAnalyses { get; set; } = new();
        public List<SafetyTrend> Trends { get; set; } = new();
        public List<RiskAssessment> RiskAssessments { get; set; } = new();
        public List<SafetyRecommendation> Recommendations { get; set; } = new();
    }

    public class PredictiveMaintenanceAnalytics
    {
        public MaintenanceSummary Summary { get; set; } = new();
        public List<MaintenancePrediction> Predictions { get; set; } = new();
        public List<EquipmentHealthStatus> EquipmentHealth { get; set; } = new();
        public List<MaintenanceRecommendation> Recommendations { get; set; } = new();
        public double PotentialCostSavings { get; set; }
    }

    public class WeatherImpactAnalytics
    {
        public WeatherImpactSummary Summary { get; set; } = new();
        public List<RouteWeatherImpact> RouteImpacts { get; set; } = new();
        public List<WeatherDelayAnalysis> DelayAnalyses { get; set; } = new();
        public List<WeatherOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
        public SeasonalWeatherPatterns SeasonalPatterns { get; set; } = new();
    }

    public class NorthernLightsAnalytics
    {
        public AuroraSummary Summary { get; set; } = new();
        public List<AuroraViewingOpportunity> ViewingOpportunities { get; set; } = new();
        public List<PassengerSatisfactionCorrelation> SatisfactionCorrelations { get; set; } = new();
        public AuroraForecastAccuracy ForecastAccuracy { get; set; } = new();
        public List<string> OptimizationRecommendations { get; set; } = new();
    }

    public class FleetUtilizationAnalytics
    {
        public UtilizationSummary Summary { get; set; } = new();
        public List<VesselUtilizationMetrics> VesselUtilizations { get; set; } = new();
        public List<RouteUtilizationMetrics> RouteUtilizations { get; set; } = new();
        public List<CapacityOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
        public SeasonalUtilizationPattern SeasonalPatterns { get; set; } = new();
    }

    public class FleetBenchmarkingAnalytics
    {
        public BenchmarkingSummary Summary { get; set; } = new();
        public List<PerformanceBenchmark> Benchmarks { get; set; } = new();
        public List<IndustryComparison> IndustryComparisons { get; set; } = new();
        public List<BestPracticeRecommendation> BestPractices { get; set; } = new();
        public double CompetitivePosition { get; set; }
    }

    public class CustomAnalyticsReport
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public List<AnalyticsChart> Charts { get; set; } = new();
        public List<AnalyticsInsight> Insights { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; } = string.Empty;
    }

    public class FleetDashboardData
    {
        public FleetStatus FleetStatus { get; set; } = new();
        public List<VesselStatus> VesselStatuses { get; set; } = new();
        public List<RealtimeMetric> RealtimeMetrics { get; set; } = new();
        public List<Alert> ActiveAlerts { get; set; } = new();
        public List<KPIWidget> KPIWidgets { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class AnalyticsExportResult
    {
        public string ExportId { get; set; } = Guid.NewGuid().ToString();
        public string Status { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int RecordCount { get; set; }
        public double FileSizeMB { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }

    #endregion

    #region Supporting Classes

    public class VesselPerformanceSummary
    {
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public double PerformanceScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, double> KeyMetrics { get; set; } = new();
    }

    public class PerformanceTrend
    {
        public string MetricName { get; set; } = string.Empty;
        public List<DataPoint> DataPoints { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty;
        public double PercentageChange { get; set; }
    }

    public class DataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class PerformanceKPI
    {
        public string Name { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double TargetValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double PercentageToTarget { get; set; }
    }

    public class PerformanceAlert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Active";
    }

    public class CustomAnalyticsRequest
    {
        public string AnalyticsType { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public List<string> Metrics { get; set; } = new();
        public List<string> Dimensions { get; set; } = new();
        public Dictionary<string, object> Filters { get; set; } = new();
        public AnalyticsTimeRange TimeRange { get; set; } = new();
        public string Format { get; set; } = "JSON";
        public bool IncludeCharts { get; set; } = true;
    }

    public class AnalyticsExportRequest
    {
        public string ExportId { get; set; } = Guid.NewGuid().ToString();
        public string DataType { get; set; } = string.Empty;
        public AnalyticsTimeRange TimeRange { get; set; } = new();
        public List<string>? VesselIds { get; set; }
        public string Format { get; set; } = "CSV"; // CSV, JSON, Excel
        public Dictionary<string, object>? Filters { get; set; }
        public bool IncludeMetadata { get; set; } = true;
    }

    // Additional supporting models would be defined here...
    // (For brevity, showing representative structure)

    #endregion
}