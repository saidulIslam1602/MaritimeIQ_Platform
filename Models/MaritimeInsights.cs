namespace HavilaKystruten.Maritime.Models;

/// <summary>
/// High-level operational snapshot for the Havila maritime platform.
/// </summary>
public record MaritimeInsightsOverview(
    FleetAnalyticsSnapshot Fleet,
    EnvironmentalSummary Environmental,
    VoyageScheduleSnapshot Voyages,
    IReadOnlyList<MaritimeAiModelSummary> AiModels,
    DateTime GeneratedAt
);

/// <summary>
/// Aggregated analytics for the active fleet.
/// </summary>
public record FleetAnalyticsSnapshot(
    int TotalVessels,
    int ActiveVessels,
    int InPort,
    double AverageSpeedKnots,
    double OnTimePerformance,
    decimal DailyCo2Emissions,
    decimal AverageBatteryState,
    IReadOnlyList<OperationalAlert> ActiveAlerts
);

/// <summary>
/// Summary of environmental KPIs tracked for the fleet.
/// </summary>
public record EnvironmentalSummary(
    decimal TotalFuelConsumptionLiters,
    decimal TotalPowerConsumptionKWh,
    decimal AverageWaterTemperature,
    decimal AverageAirTemperature,
    decimal AverageWindSpeedKnots,
    decimal AverageWaveHeightMeters
);

/// <summary>
/// Upcoming voyages and occupancy metrics.
/// </summary>
public record VoyageScheduleSnapshot(
    IReadOnlyList<VoyageScheduleItem> Voyages,
    int TotalPassengers,
    int TotalCrew
);

public record VoyageScheduleItem(
    int VoyageId,
    string VesselName,
    string RouteName,
    DateTime DepartureTime,
    string DeparturePort,
    string ArrivalPort,
    int PassengerCount,
    VoyageStatus Status
);

/// <summary>
/// Status grouping used by dashboards.
/// </summary>
public record FleetStatusSummary(
    int TotalVessels,
    int InService,
    int InPort,
    int Maintenance,
    int Emergency,
    DateTime LastUpdated
);

/// <summary>
/// Alert raised for operations team attention.
/// </summary>
public record OperationalAlert(
    string Title,
    string Severity,
    string Description,
    DateTime Timestamp
);

/// <summary>
/// Lightweight summary of AI models that power insights.
/// </summary>
public record MaritimeAiModelSummary(
    string ModelName,
    string ModelType,
    string Version,
    decimal Accuracy,
    bool IsActive
)
{
    public static MaritimeAiModelSummary From(HavilaKystruten.Maritime.Models.AIModel model) =>
        new(model.ModelName, model.ModelType, model.Version, model.Accuracy, model.IsActive);
}
