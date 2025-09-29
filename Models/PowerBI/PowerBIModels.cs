using System.ComponentModel.DataAnnotations;

namespace MaritimeIQ.Platform.Models.PowerBI
{
    public class MaritimeDashboard
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<MaritimePowerBIReport> Reports { get; set; } = new();
        public List<DashboardTile> Tiles { get; set; } = new();
        public DateTime LastRefresh { get; set; }
        public string AccessLevel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class MaritimePowerBIReport
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RefreshFrequency { get; set; } = string.Empty;
        public string[] DataSources { get; set; } = Array.Empty<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string EmbedUrl { get; set; } = string.Empty;
        public string ReportWebUrl { get; set; } = string.Empty;
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class DashboardTile
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; }
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    public class PowerBIEmbedConfig
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string EmbedUrl { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }
        public EmbedSettings Settings { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsValid => Expiration.HasValue && Expiration.Value > DateTime.UtcNow;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class EmbedSettings
    {
        public bool FilterPaneEnabled { get; set; }
        public bool NavContentPaneEnabled { get; set; }
        public string Background { get; set; } = string.Empty;
        public bool ShowTabs { get; set; } = true;
        public string Theme { get; set; } = "light";
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }

    public class MaritimeKPISummary
    {
        public FleetOperationsKPIs FleetOperations { get; set; } = new();
        public EnvironmentalKPIs Environmental { get; set; } = new();
        public PassengerExperienceKPIs PassengerExperience { get; set; } = new();
        public SafetyKPIs Safety { get; set; } = new();
        public FinancialKPIs Financial { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public string SummaryPeriod { get; set; } = "current-month";
        public double OverallScore { get; set; }
        public List<string> KeyInsights { get; set; } = new();
    }

    public class FleetOperationsKPIs
    {
        public int ActiveVessels { get; set; }
        public double OnTimePerformance { get; set; }
        public double AverageSpeed { get; set; }
        public double FuelEfficiency { get; set; }
        public int CurrentlyAtSea { get; set; }
        public int InPort { get; set; }
        public int TotalVoyagesThisMonth { get; set; }
        public int PassengersThisMonth { get; set; }
        public double CapacityUtilization { get; set; }
        public double MaintenanceCompliance { get; set; }
    }

    public class EnvironmentalKPIs
    {
        public double CO2EmissionsToday { get; set; }
        public double CO2ReductionPercentage { get; set; }
        public double HybridModeUsage { get; set; }
        public double WasteRecyclingRate { get; set; }
        public double EnergyEfficiencyScore { get; set; }
        public double EnvironmentalComplianceScore { get; set; }
        public double RenewableEnergyUsage { get; set; }
        public double SustainabilityIndex { get; set; }
    }

    public class PassengerExperienceKPIs
    {
        public double AverageRating { get; set; }
        public double NorthernLightsVisibilityTonight { get; set; }
        public double ServiceComplaintRate { get; set; }
        public double RecommendationScore { get; set; }
        public double OnboardExperienceRating { get; set; }
        public double DiningServiceRating { get; set; }
        public double AccommodationRating { get; set; }
        public double ExcursionSatisfaction { get; set; }
    }

    public class SafetyKPIs
    {
        public int IncidentsThisMonth { get; set; }
        public double SafetyDrillCompliance { get; set; }
        public double EmergencyResponseTime { get; set; }
        public double SafetyTrainingCompletion { get; set; }
        public double EquipmentReadinessScore { get; set; }
        public double CrewSafetyCertificationRate { get; set; }
        public double RiskAssessmentScore { get; set; }
        public double ComplianceRating { get; set; }
    }

    public class FinancialKPIs
    {
        public decimal RevenueThisMonth { get; set; }
        public double BookingOccupancyRate { get; set; }
        public decimal RevenuePerPassenger { get; set; }
        public decimal OperationalCostPerVoyage { get; set; }
        public double ProfitMargin { get; set; }
        public double SeasonalVariance { get; set; }
        public decimal TotalBookingValue { get; set; }
        public double YearOverYearGrowth { get; set; }
    }

    public class NorthernLightsForecast
    {
        public string CurrentActivity { get; set; } = string.Empty;
        public int ActivityLevel { get; set; }
        public double VisibilityPercentage { get; set; }
        public DateTime BestViewingTime { get; set; }
        public string WeatherConditions { get; set; } = string.Empty;
        public List<ViewingLocation> OptimalViewingLocations { get; set; } = new();
        public List<DailyForecast> Forecast7Day { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public string DataSource { get; set; } = string.Empty;
        public double ConfidenceLevel { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class ViewingLocation
    {
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double VisibilityScore { get; set; }
        public string LightPollution { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public double Elevation { get; set; }
        public string AccessibilityLevel { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();
    }

    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public int ActivityLevel { get; set; }
        public double VisibilityPercentage { get; set; }
        public string WeatherDescription { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double CloudCover { get; set; }
        public double MoonPhase { get; set; }
        public List<string> ViewingTips { get; set; } = new();
    }

    public class MaritimePowerBIDataset
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime LastRefresh { get; set; }
        public string RefreshStatus { get; set; } = string.Empty;
        public List<PowerBITable> Tables { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class PowerBITable
    {
        public string Name { get; set; } = string.Empty;
        public List<PowerBIColumn> Columns { get; set; } = new();
        public int RowCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PowerBIColumn
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsKey { get; set; }
        public bool IsNullable { get; set; } = true;
    }

    public class MaritimePowerBIRefreshResult
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> RefreshedTables { get; set; } = new();
        public TimeSpan? Duration => EndTime?.Subtract(StartTime);
    }

    public class MaritimePowerBIUsageMetrics
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public TimeSpan AverageViewDuration { get; set; }
        public List<DailyUsage> DailyBreakdown { get; set; } = new();
        public List<UserUsage> TopUsers { get; set; } = new();
    }

    public class DailyUsage
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int UniqueUsers { get; set; }
        public double AverageDuration { get; set; }
    }

    public class UserUsage
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public double TotalDuration { get; set; }
        public DateTime LastAccess { get; set; }
    }
}