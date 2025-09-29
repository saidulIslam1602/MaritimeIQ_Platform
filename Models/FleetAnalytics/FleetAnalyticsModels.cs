using System;
using System.Collections.Generic;
using MaritimeIQ.Platform.Models.Safety;

namespace MaritimeIQ.Platform.Models.FleetAnalytics
{
    #region Fuel and Efficiency Models

    /// <summary>
    /// Vessel fuel efficiency metrics
    /// </summary>
    public class VesselFuelEfficiency
    {
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public double FuelConsumptionPerHour { get; set; }
        public double FuelConsumptionPerNauticalMile { get; set; }
        public double EfficiencyScore { get; set; }
        public double OptimalConsumption { get; set; }
        public double ActualConsumption { get; set; }
        public Dictionary<string, double> ConsumptionByCondition { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Fuel consumption trend analysis
    /// </summary>
    public class FuelConsumptionTrend
    {
        public string VesselId { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public List<DataPoint> ConsumptionHistory { get; set; } = new();
        public double TrendPercentage { get; set; }
        public string TrendDirection { get; set; } = "Stable";
        public List<string> ContributingFactors { get; set; } = new();
        public Dictionary<string, double> SeasonalVariations { get; set; } = new();
    }

    /// <summary>
    /// Fuel optimization recommendations
    /// </summary>
    public class FuelOptimizationRecommendation
    {
        public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double PotentialSavings { get; set; }
        public string Priority { get; set; } = "Medium";
        public string ImplementationComplexity { get; set; } = "Medium";
        public TimeSpan EstimatedImplementationTime { get; set; }
        public List<string> RequiredActions { get; set; } = new();
    }

    #endregion

    #region Core Metrics Models

    /// <summary>
    /// Fuel efficiency metrics for individual vessels
    /// </summary>
    public class FuelEfficiencyMetrics
    {
        public double ConsumptionPerHour { get; set; }
        public double ConsumptionPerNauticalMile { get; set; }
        public double EfficiencyScore { get; set; }
        public double OptimizationPotential { get; set; }
        public Dictionary<string, double> DetailedMetrics { get; set; } = new();
    }

    /// <summary>
    /// Operational performance metrics
    /// </summary>
    public class OperationalMetrics
    {
        public double OnTimePerformance { get; set; }
        public double UtilizationRate { get; set; }
        public int OperationalHours { get; set; }
        public double RevenuePerHour { get; set; }
        public double OperationalEfficiency { get; set; }
        public Dictionary<string, double> KPIs { get; set; } = new();
    }

    /// <summary>
    /// Environmental performance metrics
    /// </summary>
    public class EnvironmentalMetrics
    {
        public double CO2Emissions { get; set; }
        public double NOxEmissions { get; set; }
        public double SOxEmissions { get; set; }
        public double WasteGenerated { get; set; }
        public double RecyclingRate { get; set; }
        public double ComplianceScore { get; set; }
    }

    #endregion

    #region Compliance Models

    /// <summary>
    /// Overall compliance summary for fleet
    /// </summary>
    public class ComplianceSummary
    {
        public double OverallComplianceScore { get; set; }
        public int TotalViolations { get; set; }
        public int CriticalViolations { get; set; }
        public List<VesselComplianceStatus> VesselStatuses { get; set; } = new();
        public Dictionary<string, int> ViolationsByCategory { get; set; } = new();
        public List<string> RecentActions { get; set; } = new();
        public DateTime LastAssessment { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Individual vessel compliance status
    /// </summary>
    public class VesselComplianceStatus
    {
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public double ComplianceScore { get; set; }
        public string Status { get; set; } = "Compliant";
        public List<string> ActiveViolations { get; set; } = new();
        public Dictionary<string, string> CertificationStatuses { get; set; } = new();
        public DateTime LastInspection { get; set; }
        public DateTime NextInspectionDue { get; set; }
    }

    /// <summary>
    /// Compliance trend over time
    /// </summary>
    public class ComplianceTrend
    {
        public string Category { get; set; } = string.Empty;
        public List<DataPoint> TrendData { get; set; } = new();
        public double ImprovementRate { get; set; }
        public string TrendDirection { get; set; } = "Improving";
        public List<string> KeyMilestones { get; set; } = new();
    }

    #endregion

    #region Passenger and Service Models

    /// <summary>
    /// Passenger experience summary
    /// </summary>
    public class PassengerSummary
    {
        public int TotalPassengers { get; set; }
        public double AverageOccupancy { get; set; }
        public double SatisfactionScore { get; set; }
        public Dictionary<string, double> ServiceRatings { get; set; } = new();
        public List<string> CommonComplaints { get; set; } = new();
        public List<string> PositiveFeedback { get; set; } = new();
        public double RepeatCustomerRate { get; set; }
    }

    /// <summary>
    /// Route-specific passenger experience metrics
    /// </summary>
    public class RouteExperienceMetrics
    {
        public string RouteId { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public double PassengerSatisfaction { get; set; }
        public double OnTimePerformance { get; set; }
        public Dictionary<string, double> ServiceMetrics { get; set; } = new();
        public List<string> PopularAttractions { get; set; } = new();
        public double WeatherImpactScore { get; set; }
    }

    /// <summary>
    /// Service quality metrics
    /// </summary>
    public class ServiceMetrics
    {
        public double FoodServiceRating { get; set; }
        public double CabinServiceRating { get; set; }
        public double StaffServiceRating { get; set; }
        public double CleanlinessRating { get; set; }
        public double EntertainmentRating { get; set; }
        public double WifiQuality { get; set; }
        public Dictionary<string, double> AdditionalServices { get; set; } = new();
    }

    /// <summary>
    /// Experience insights and recommendations
    /// </summary>
    public class ExperienceInsight
    {
        public string InsightType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = "Medium";
        public List<string> RecommendedActions { get; set; } = new();
        public double ConfidenceLevel { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    #endregion

    #region Route and Performance Models

    /// <summary>
    /// Route performance summary
    /// </summary>
    public class RouteSummary
    {
        public string RouteId { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public double Efficiency { get; set; }
        public double Profitability { get; set; }
        public double PopularityScore { get; set; }
        public List<RoutePerformanceMetrics> PerformanceHistory { get; set; } = new();
        public Dictionary<string, double> KeyMetrics { get; set; } = new();
    }

    /// <summary>
    /// Detailed route performance metrics
    /// </summary>
    public class RoutePerformanceMetrics
    {
        public string RouteId { get; set; } = string.Empty;
        public DateTime Period { get; set; }
        public double FuelEfficiency { get; set; }
        public double OnTimePerformance { get; set; }
        public double PassengerLoad { get; set; }
        public double Revenue { get; set; }
        public double OperatingCosts { get; set; }
        public double ProfitMargin { get; set; }
        public Dictionary<string, double> WeatherImpacts { get; set; } = new();
    }

    /// <summary>
    /// Route optimization opportunities
    /// </summary>
    public class RouteOptimizationOpportunity
    {
        public string OpportunityId { get; set; } = Guid.NewGuid().ToString();
        public string RouteId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double PotentialImprovement { get; set; }
        public string ImprovementMetric { get; set; } = string.Empty;
        public double ImplementationCost { get; set; }
        public TimeSpan PaybackPeriod { get; set; }
        public string Priority { get; set; } = "Medium";
    }

    #endregion

    #region Weather and Environmental Models

    /// <summary>
    /// Weather impact analysis summary
    /// </summary>
    public class WeatherImpactSummary
    {
        public DateTime AnalysisPeriod { get; set; } = DateTime.UtcNow;
        public double OverallImpactScore { get; set; }
        public List<RouteWeatherImpact> RouteImpacts { get; set; } = new();
        public Dictionary<string, double> WeatherFactors { get; set; } = new();
        public List<WeatherDelayAnalysis> DelayAnalysis { get; set; } = new();
        public SeasonalWeatherPatterns SeasonalPatterns { get; set; } = new();
    }

    /// <summary>
    /// Route-specific weather impact
    /// </summary>
    public class RouteWeatherImpact
    {
        public string RouteId { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public double ImpactScore { get; set; }
        public Dictionary<string, double> WeatherMetrics { get; set; } = new();
        public int DelayIncidents { get; set; }
        public TimeSpan AverageDelay { get; set; }
        public double FuelImpact { get; set; }
    }

    /// <summary>
    /// Weather delay analysis
    /// </summary>
    public class WeatherDelayAnalysis
    {
        public string DelayType { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public double SeasonalityFactor { get; set; }
        public List<string> MitigationStrategies { get; set; } = new();
        public double CostImpact { get; set; }
    }

    /// <summary>
    /// Weather optimization opportunities
    /// </summary>
    public class WeatherOptimizationOpportunity
    {
        public string OpportunityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double PotentialSavings { get; set; }
        public string Metric { get; set; } = string.Empty;
        public List<string> RequiredActions { get; set; } = new();
        public string Priority { get; set; } = "Medium";
    }

    /// <summary>
    /// Seasonal weather patterns
    /// </summary>
    public class SeasonalWeatherPatterns
    {
        public Dictionary<string, WeatherSeasonData> SeasonalData { get; set; } = new();
        public List<string> CriticalPeriods { get; set; } = new();
        public Dictionary<string, double> MonthlyAverages { get; set; } = new();
        public List<string> WeatherTrends { get; set; } = new();
    }

    /// <summary>
    /// Weather season data
    /// </summary>
    public class WeatherSeasonData
    {
        public string Season { get; set; } = string.Empty;
        public double AverageWindSpeed { get; set; }
        public double AverageWaveHeight { get; set; }
        public double VisibilityScore { get; set; }
        public int StormDays { get; set; }
        public double DelayProbability { get; set; }
    }

    #endregion

    #region Safety and Risk Models

    /// <summary>
    /// Safety performance summary
    /// </summary>
    public class SafetySummary
    {
        public double OverallSafetyScore { get; set; }
        public int TotalIncidents { get; set; }
        public int SafetyViolations { get; set; }
        public List<IncidentAnalysis> RecentIncidents { get; set; } = new();
        public List<SafetyTrend> SafetyTrends { get; set; } = new();
        public Dictionary<string, int> IncidentsByCategory { get; set; } = new();
        public DateTime LastSafetyAudit { get; set; }
    }

    /// <summary>
    /// Individual incident analysis
    /// </summary>
    public class IncidentAnalysis
    {
        public string IncidentId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RootCause { get; set; } = string.Empty;
        public List<string> ContributingFactors { get; set; } = new();
        public List<string> PreventiveMeasures { get; set; } = new();
        public DateTime OccurredAt { get; set; }
        public string Status { get; set; } = "Resolved";
    }

    /// <summary>
    /// Safety trend analysis
    /// </summary>
    public class SafetyTrend
    {
        public string Category { get; set; } = string.Empty;
        public List<DataPoint> TrendData { get; set; } = new();
        public double ImprovementRate { get; set; }
        public string Direction { get; set; } = "Improving";
        public List<string> KeyActions { get; set; } = new();
    }

    /// <summary>
    /// Risk assessment model
    /// </summary>
    public class RiskAssessment
    {
        public string RiskId { get; set; } = Guid.NewGuid().ToString();
        public string RiskType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Probability { get; set; }
        public double Impact { get; set; }
        public double RiskScore { get; set; }
        public string Priority { get; set; } = "Medium";
        public List<string> MitigationStrategies { get; set; } = new();
        public DateTime LastAssessed { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Safety recommendations
    /// </summary>
    public class SafetyRecommendation
    {
        public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public double RiskReduction { get; set; }
        public TimeSpan ImplementationTime { get; set; }
        public double Cost { get; set; }
        public List<string> RequiredResources { get; set; } = new();
    }

    #endregion

    #region Maintenance Models

    /// <summary>
    /// Fleet maintenance summary
    /// </summary>
    public class MaintenanceSummary
    {
        public int PendingMaintenanceItems { get; set; }
        public int OverdueItems { get; set; }
        public double MaintenanceEfficiency { get; set; }
        public double EquipmentAvailability { get; set; }
        public List<EquipmentHealthStatus> EquipmentHealth { get; set; } = new();
        public List<MaintenanceRecommendation> Recommendations { get; set; } = new();
        public double TotalMaintenanceCost { get; set; }
    }

    /// <summary>
    /// Equipment health status
    /// </summary>
    public class EquipmentHealthStatus
    {
        public string EquipmentId { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string VesselId { get; set; } = string.Empty;
        public double HealthScore { get; set; }
        public string Status { get; set; } = "Good";
        public DateTime LastMaintenance { get; set; }
        public DateTime NextMaintenanceDue { get; set; }
        public List<string> CurrentIssues { get; set; } = new();
        public double EstimatedRemainingLife { get; set; }
    }

    /// <summary>
    /// Maintenance recommendations
    /// </summary>
    public class MaintenanceRecommendation
    {
        public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
        public string EquipmentId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public DateTime RecommendedDate { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public double EstimatedCost { get; set; }
        public List<string> RequiredParts { get; set; } = new();
    }

    #endregion

    #region Seasonal and Aurora Models (for Northern Routes)

    /// <summary>
    /// Aurora viewing summary
    /// </summary>
    public class AuroraSummary
    {
        public double ViewingProbability { get; set; }
        public string ActivityLevel { get; set; } = "Moderate";
        public List<AuroraViewingOpportunity> BestViewingTimes { get; set; } = new();
        public Dictionary<string, double> LocationProbabilities { get; set; } = new();
        public List<PassengerSatisfactionCorrelation> SatisfactionData { get; set; } = new();
        public AuroraForecastAccuracy ForecastAccuracy { get; set; } = new();
    }

    /// <summary>
    /// Aurora viewing opportunities
    /// </summary>
    public class AuroraViewingOpportunity
    {
        public DateTime ViewingTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public double Probability { get; set; }
        public string ActivityLevel { get; set; } = "Moderate";
        public double CloudCoverProbability { get; set; }
        public string OptimalViewingConditions { get; set; } = string.Empty;
    }

    /// <summary>
    /// Passenger satisfaction correlation with aurora viewing
    /// </summary>
    public class PassengerSatisfactionCorrelation
    {
        public string RouteId { get; set; } = string.Empty;
        public bool AuroraVisible { get; set; }
        public double SatisfactionScore { get; set; }
        public double SatisfactionBoost { get; set; }
        public int PassengerCount { get; set; }
        public DateTime TripDate { get; set; }
    }

    /// <summary>
    /// Aurora forecast accuracy tracking
    /// </summary>
    public class AuroraForecastAccuracy
    {
        public double OverallAccuracy { get; set; }
        public Dictionary<string, double> AccuracyByTimeframe { get; set; } = new();
        public int TotalForecasts { get; set; }
        public int AccurateForecasts { get; set; }
        public List<string> ImprovementAreas { get; set; } = new();
    }

    #endregion

    #region Utilization Models

    /// <summary>
    /// Fleet utilization summary
    /// </summary>
    public class UtilizationSummary
    {
        public double OverallUtilization { get; set; }
        public List<VesselUtilizationMetrics> VesselUtilization { get; set; } = new();
        public List<RouteUtilizationMetrics> RouteUtilization { get; set; } = new();
        public List<CapacityOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
        public SeasonalUtilizationPattern SeasonalPatterns { get; set; } = new();
    }

    /// <summary>
    /// Individual vessel utilization metrics
    /// </summary>
    public class VesselUtilizationMetrics
    {
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public double OperationalUtilization { get; set; }
        public double PassengerUtilization { get; set; }
        public double RevenueUtilization { get; set; }
        public int OperatingDays { get; set; }
        public int MaintenanceDays { get; set; }
        public double Efficiency { get; set; }
    }

    /// <summary>
    /// Route utilization metrics
    /// </summary>
    public class RouteUtilizationMetrics
    {
        public string RouteId { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public double AverageOccupancy { get; set; }
        public double SeasonalVariation { get; set; }
        public int TotalTrips { get; set; }
        public double Revenue { get; set; }
        public double Profitability { get; set; }
    }

    /// <summary>
    /// Capacity optimization opportunities
    /// </summary>
    public class CapacityOptimizationOpportunity
    {
        public string OpportunityId { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double PotentialImprovement { get; set; }
        public string AffectedRoute { get; set; } = string.Empty;
        public string AffectedVessel { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public double ImplementationCost { get; set; }
    }

    /// <summary>
    /// Seasonal utilization patterns
    /// </summary>
    public class SeasonalUtilizationPattern
    {
        public Dictionary<string, double> MonthlyUtilization { get; set; } = new();
        public string PeakSeason { get; set; } = string.Empty;
        public string OffSeason { get; set; } = string.Empty;
        public double SeasonalVariance { get; set; }
        public List<string> SeasonalFactors { get; set; } = new();
    }

    #endregion

    #region Benchmarking Models

    /// <summary>
    /// Industry benchmarking summary
    /// </summary>
    public class BenchmarkingSummary
    {
        public string BenchmarkPeriod { get; set; } = string.Empty;
        public List<PerformanceBenchmark> PerformanceBenchmarks { get; set; } = new();
        public List<IndustryComparison> IndustryComparisons { get; set; } = new();
        public List<BestPracticeRecommendation> BestPractices { get; set; } = new();
        public double OverallRanking { get; set; }
        public string CompetitivePosition { get; set; } = string.Empty;
    }

    /// <summary>
    /// Performance benchmark data
    /// </summary>
    public class PerformanceBenchmark
    {
        public string MetricName { get; set; } = string.Empty;
        public double OurPerformance { get; set; }
        public double IndustryAverage { get; set; }
        public double IndustryBest { get; set; }
        public string PerformanceCategory { get; set; } = "Average";
        public double ImprovementPotential { get; set; }
        public string BenchmarkSource { get; set; } = string.Empty;
    }

    /// <summary>
    /// Industry comparison data
    /// </summary>
    public class IndustryComparison
    {
        public string Category { get; set; } = string.Empty;
        public string Metric { get; set; } = string.Empty;
        public double OurValue { get; set; }
        public double IndustryMedian { get; set; }
        public double TopQuartile { get; set; }
        public string RelativePosition { get; set; } = string.Empty;
        public List<string> ImprovementAreas { get; set; } = new();
    }

    /// <summary>
    /// Best practice recommendations
    /// </summary>
    public class BestPracticeRecommendation
    {
        public string PracticeId { get; set; } = Guid.NewGuid().ToString();
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BenchmarkCompany { get; set; } = string.Empty;
        public double ExpectedImprovement { get; set; }
        public string ImplementationDifficulty { get; set; } = "Medium";
        public TimeSpan ImplementationTime { get; set; }
        public double Cost { get; set; }
    }

    #endregion

    #region Analytics and Dashboard Models

    /// <summary>
    /// Analytics chart configuration
    /// </summary>
    public class AnalyticsChart
    {
        public string ChartId { get; set; } = Guid.NewGuid().ToString();
        public string ChartType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<ChartDataSeries> DataSeries { get; set; } = new();
        public Dictionary<string, object> ChartOptions { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Chart data series
    /// </summary>
    public class ChartDataSeries
    {
        public string SeriesName { get; set; } = string.Empty;
        public List<DataPoint> Data { get; set; } = new();
        public string Color { get; set; } = string.Empty;
        public string Type { get; set; } = "line";
    }

    /// <summary>
    /// Analytics insights
    /// </summary>
    public class AnalyticsInsight
    {
        public string InsightId { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = "Medium";
        public double Confidence { get; set; }
        public List<string> RecommendedActions { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Fleet status overview
    /// </summary>
    public class FleetStatus
    {
        public int TotalVessels { get; set; }
        public int OperationalVessels { get; set; }
        public int VesselsInMaintenance { get; set; }
        public double OverallEfficiency { get; set; }
        public List<string> ActiveAlerts { get; set; } = new();
        public Dictionary<string, int> VesselsByStatus { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Real-time metrics
    /// </summary>
    public class RealtimeMetric
    {
        public string MetricId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double Target { get; set; }
        public string Status { get; set; } = "Normal";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// General alert model
    /// </summary>
    public class Alert
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Active";
        public Dictionary<string, object> Context { get; set; } = new();
    }

    /// <summary>
    /// KPI dashboard widget
    /// </summary>
    public class KPIWidget
    {
        public string WidgetId { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "metric";
        public double Value { get; set; }
        public double Target { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Trend { get; set; } = "stable";
        public double TrendPercentage { get; set; }
        public string Status { get; set; } = "normal";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    #endregion

    #region Summary Models

    /// <summary>
    /// Fuel consumption and efficiency summary
    /// </summary>
    public class FuelSummary
    {
        public double TotalConsumption { get; set; }
        public double AverageEfficiency { get; set; }
        public double CostSavings { get; set; }
        public List<VesselFuelEfficiency> VesselEfficiencies { get; set; } = new();
        public List<FuelConsumptionTrend> ConsumptionTrends { get; set; } = new();
        public List<FuelOptimizationRecommendation> OptimizationRecommendations { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Analytics time range enumeration
    /// </summary>
    public enum AnalyticsTimeRange
    {
        Last24Hours,
        LastWeek,
        LastMonth,
        LastQuarter,
        LastYear,
        Custom
    }

    /// <summary>
    /// Custom analytics request
    /// </summary>
    public class CustomAnalyticsRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string AnalyticsType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public List<string> Metrics { get; set; } = new();
        public string OutputFormat { get; set; } = "JSON";
    }

    /// <summary>
    /// Analytics export request
    /// </summary>
    public class AnalyticsExportRequest
    {
        public string ExportId { get; set; } = Guid.NewGuid().ToString();
        public string AnalyticsType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } = "CSV";
        public List<string> IncludedMetrics { get; set; } = new();
        public Dictionary<string, object> FilterOptions { get; set; } = new();
    }

    /// <summary>
    /// Analytics export result
    /// </summary>
    public class AnalyticsExportResult
    {
        public string ExportId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string DownloadUrl { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    #endregion

    #region Core Analytics Models

    /// <summary>
    /// Fleet performance analytics summary
    /// </summary>
    public class FleetPerformanceAnalytics
    {
        public double OverallPerformanceScore { get; set; }
        public List<VesselPerformanceMetrics> VesselPerformances { get; set; } = new();
        public Dictionary<string, double> KPIs { get; set; } = new();
        public List<PerformanceTrend> Trends { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Individual vessel performance metrics
    /// </summary>
    public class VesselPerformanceMetrics
    {
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public double OverallScore { get; set; }
        public FuelEfficiencyMetrics FuelEfficiency { get; set; } = new();
        public OperationalMetrics Operations { get; set; } = new();
        public SafetyMetrics Safety { get; set; } = new();
        public EnvironmentalMetrics Environmental { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Fuel efficiency analytics
    /// </summary>
    public class FuelEfficiencyAnalytics
    {
        public FuelSummary Summary { get; set; } = new();
        public List<VesselFuelEfficiency> VesselEfficiencies { get; set; } = new();
        public List<FuelConsumptionTrend> Trends { get; set; } = new();
        public List<FuelOptimizationRecommendation> Recommendations { get; set; } = new();
        public Dictionary<string, double> Benchmarks { get; set; } = new();
    }

    /// <summary>
    /// Environmental compliance analytics
    /// </summary>
    public class EnvironmentalComplianceAnalytics
    {
        public ComplianceSummary Summary { get; set; } = new();
        public List<VesselComplianceStatus> VesselStatuses { get; set; } = new();
        public List<ComplianceTrend> Trends { get; set; } = new();
        public Dictionary<string, double> EmissionMetrics { get; set; } = new();
        public List<string> Violations { get; set; } = new();
        public DateTime LastAssessment { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Performance trend analysis
    /// </summary>
    public class PerformanceTrend
    {
        public string MetricName { get; set; } = string.Empty;
        public List<DataPoint> TrendData { get; set; } = new();
        public double TrendPercentage { get; set; }
        public string Direction { get; set; } = "Stable";
        public string Period { get; set; } = string.Empty;
    }

    #endregion

    #region Advanced Analytics Models

    /// <summary>
    /// Passenger experience analytics
    /// </summary>
    public class PassengerExperienceAnalytics
    {
        public PassengerSummary Summary { get; set; } = new();
        public List<RouteExperienceMetrics> RouteExperiences { get; set; } = new();
        public List<ServiceMetrics> ServiceMetrics { get; set; } = new();
        public List<ExperienceInsight> Insights { get; set; } = new();
        public Dictionary<string, double> SatisfactionTrends { get; set; } = new();
    }

    /// <summary>
    /// Route efficiency analytics
    /// </summary>
    public class RouteEfficiencyAnalytics
    {
        public List<RouteSummary> RouteSummaries { get; set; } = new();
        public List<RoutePerformanceMetrics> PerformanceMetrics { get; set; } = new();
        public List<RouteOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
        public Dictionary<string, double> EfficiencyTrends { get; set; } = new();
    }

    /// <summary>
    /// Safety analytics
    /// </summary>
    public class SafetyAnalytics
    {
        public SafetySummary Summary { get; set; } = new();
        public List<IncidentAnalysis> IncidentAnalyses { get; set; } = new();
        public List<SafetyTrend> Trends { get; set; } = new();
        public List<RiskAssessment> RiskAssessments { get; set; } = new();
        public List<SafetyRecommendation> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Predictive maintenance analytics
    /// </summary>
    public class PredictiveMaintenanceAnalytics
    {
        public MaintenanceSummary Summary { get; set; } = new();
        public List<EquipmentHealthStatus> EquipmentHealth { get; set; } = new();
        public List<MaintenanceRecommendation> Recommendations { get; set; } = new();
        public Dictionary<string, double> PredictiveIndicators { get; set; } = new();
        public List<string> UpcomingMaintenance { get; set; } = new();
    }

    /// <summary>
    /// Weather impact analytics
    /// </summary>
    public class WeatherImpactAnalytics
    {
        public WeatherImpactSummary Summary { get; set; } = new();
        public List<RouteWeatherImpact> RouteImpacts { get; set; } = new();
        public List<WeatherDelayAnalysis> DelayAnalyses { get; set; } = new();
        public List<WeatherOptimizationOpportunity> Opportunities { get; set; } = new();
        public SeasonalWeatherPatterns SeasonalPatterns { get; set; } = new();
    }

    /// <summary>
    /// Northern Lights analytics (for Northern routes)
    /// </summary>
    public class NorthernLightsAnalytics
    {
        public AuroraSummary Summary { get; set; } = new();
        public List<AuroraViewingOpportunity> ViewingOpportunities { get; set; } = new();
        public List<PassengerSatisfactionCorrelation> SatisfactionCorrelations { get; set; } = new();
        public AuroraForecastAccuracy ForecastAccuracy { get; set; } = new();
        public Dictionary<string, double> SeasonalTrends { get; set; } = new();
    }

    /// <summary>
    /// Fleet utilization analytics
    /// </summary>
    public class FleetUtilizationAnalytics
    {
        public UtilizationSummary Summary { get; set; } = new();
        public List<VesselUtilizationMetrics> VesselUtilizations { get; set; } = new();
        public List<RouteUtilizationMetrics> RouteUtilizations { get; set; } = new();
        public List<CapacityOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
        public SeasonalUtilizationPattern SeasonalPatterns { get; set; } = new();
    }


    /// <summary>
    /// Benchmark metric
    /// </summary>
    public class BenchmarkMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public double IndustryAverage { get; set; }
        public double Percentile { get; set; }
        public string Performance { get; set; } = "Average";
    }

    /// <summary>
    /// Competitor comparison
    /// </summary>
    public class CompetitorComparison
    {
        public string CompetitorName { get; set; } = string.Empty;
        public Dictionary<string, double> MetricComparisons { get; set; } = new();
        public string OverallComparison { get; set; } = "Comparable";
    }

    /// <summary>
    /// Performance gap analysis
    /// </summary>
    public class PerformanceGap
    {
        public string Area { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double TargetValue { get; set; }
        public double Gap { get; set; }
        public string Priority { get; set; } = "Medium";
    }

    /// <summary>
    /// ETL job metrics for monitoring
    /// </summary>
    public class ETLJobMetrics
    {
        public string JobId { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public double ProcessingTimeSeconds { get; set; }
        public int RecordsProcessed { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Fleet benchmarking analytics
    /// </summary>
    public class FleetBenchmarkingAnalytics
    {
        public BenchmarkingSummary Summary { get; set; } = new();
        public List<PerformanceBenchmark> Benchmarks { get; set; } = new();
        public List<IndustryComparison> IndustryComparisons { get; set; } = new();
        public List<BestPracticeRecommendation> BestPractices { get; set; } = new();
        public string CompetitivePosition { get; set; } = string.Empty;
    }

    /// <summary>
    /// Custom analytics report
    /// </summary>
    public class CustomAnalyticsReport
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Data { get; set; } = new();
        public List<AnalyticsChart> Charts { get; set; } = new();
        public List<AnalyticsInsight> Insights { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Fleet dashboard data
    /// </summary>
    public class FleetDashboardData
    {
        public FleetStatus FleetStatus { get; set; } = new();
        public List<RealtimeMetric> RealtimeMetrics { get; set; } = new();
        public List<Alert> ActiveAlerts { get; set; } = new();
        public List<KPIWidget> KPIWidgets { get; set; } = new();
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    #endregion

    #region Common Data Models

    /// <summary>
    /// Generic data point for charts and trends
    /// </summary>
    public class DataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }


    #endregion
}