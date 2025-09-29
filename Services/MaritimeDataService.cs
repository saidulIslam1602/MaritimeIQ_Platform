using MaritimeIQ.Platform.Data;
using MaritimeIQ.Platform.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using EnvironmentalDataModel = MaritimeIQ.Platform.Models.EnvironmentalData;

namespace MaritimeIQ.Platform.Services;

public interface IMaritimeDataService
{
    IReadOnlyList<Vessel> GetFleet();
    Vessel? GetVessel(int vesselId);
    VesselPositionData? GetVesselPosition(int vesselId);
    IReadOnlyList<VesselPositionData> GetFleetPositions();
    FleetStatusSummary GetFleetStatus();
    FleetAnalyticsSnapshot GetFleetAnalytics();
    EnvironmentalSummary GetEnvironmentalSummary();
    VoyageScheduleSnapshot GetVoyageSchedule();
    MaritimeInsightsOverview GetPlatformInsights();
    bool TryUpdateVesselStatus(int vesselId, VesselStatus status, string? notes, out Vessel updatedVessel);
    VesselEmergencyResponse DeclareEmergency(int vesselId, VesselEmergencyReport report);
}

public class MaritimeDataService : IMaritimeDataService
{
    private readonly IReadOnlyList<Vessel> _fleet;
    private readonly IReadOnlyList<VesselPositionData> _positions;
    private readonly IReadOnlyList<Voyage> _voyages;
    private readonly IReadOnlyList<EnvironmentalDataModel> _environmental;
    private readonly IReadOnlyList<AIModel> _aiModels;
    private readonly List<OperationalAlert> _operationalAlerts = new();
    private readonly ILogger<MaritimeDataService> _logger;

    public MaritimeDataService(ILogger<MaritimeDataService> logger)
    {
        _logger = logger;

        _fleet = MaritimeFleetData.GetMaritimeFleet();
        _positions = MaritimeFleetData.GetCurrentVesselPositions();
        _voyages = MaritimeFleetData.GetSampleVoyages();
        _environmental = MaritimeFleetData.GetSampleEnvironmentalData();
        _aiModels = MaritimeFleetData.GetMaritimeAIModels();

        _logger.LogInformation("Maritime data service initialised with {FleetCount} vessels and {VoyageCount} voyages", _fleet.Count, _voyages.Count);
    }

    public IReadOnlyList<Vessel> GetFleet() => _fleet.Select(CloneVessel).ToList();

    public Vessel? GetVessel(int vesselId) => _fleet.FirstOrDefault(v => v.Id == vesselId)?.Let(CloneVessel);

    public VesselPositionData? GetVesselPosition(int vesselId) => _positions.FirstOrDefault(p => p.VesselId == vesselId)?.Let(ClonePosition);

    public IReadOnlyList<VesselPositionData> GetFleetPositions() => _positions.Select(ClonePosition).ToList();

    public FleetStatusSummary GetFleetStatus()
    {
        var inService = _fleet.Count(v => v.Status == VesselStatus.InService);
        var inPort = _fleet.Count(v => v.Status == VesselStatus.InPort);
        var maintenance = _fleet.Count(v => v.Status == VesselStatus.Maintenance);
        var emergency = _fleet.Count(v => v.Status == VesselStatus.Emergency);

        return new FleetStatusSummary(
            _fleet.Count,
            inService,
            inPort,
            maintenance,
            emergency,
            DateTime.UtcNow
        );
    }

    public FleetAnalyticsSnapshot GetFleetAnalytics()
    {
        var activePositions = _positions.Where(p => !p.IsEmergency).ToList();
    var avgSpeed = activePositions.Any() ? (double)activePositions.Average(p => p.SpeedKnots) : 0d;
        var co2 = _environmental.Sum(e => e.CO2EmissionKg);
        var battery = _environmental.Average(e => e.BatteryStateOfCharge);
        var alerts = BuildAlerts(activePositions)
            .Concat(_operationalAlerts)
            .OrderByDescending(a => a.Timestamp)
            .ToList();

        return new FleetAnalyticsSnapshot(
            _fleet.Count,
            _fleet.Count(v => v.Status == VesselStatus.InService),
            _fleet.Count(v => v.Status == VesselStatus.InPort),
            Math.Round(avgSpeed, 2),
            0.942, // mock yet realistic KPI
            Math.Round(co2, 2),
            Math.Round(battery, 2),
            alerts
        );
    }

    public EnvironmentalSummary GetEnvironmentalSummary()
    {
        return new EnvironmentalSummary(
            Math.Round(_environmental.Sum(e => e.FuelConsumptionLiters), 2),
            Math.Round(_environmental.Sum(e => e.PowerConsumptionKWh), 2),
            Math.Round(_environmental.Average(e => e.WaterTemperature), 2),
            Math.Round(_environmental.Average(e => e.AirTemperature), 2),
            Math.Round(_environmental.Average(e => e.WindSpeedKnots), 2),
            Math.Round(_environmental.Average(e => e.WaveHeightMeters), 2)
        );
    }

    public VoyageScheduleSnapshot GetVoyageSchedule()
    {
        var schedule = _voyages.Select(v => new VoyageScheduleItem(
            v.Id,
            _fleet.FirstOrDefault(f => f.Id == v.VesselId)?.Name ?? "Unknown Vessel",
            GetRouteName(v.RouteId),
            v.DepartureTime,
            v.DeparturePort,
            v.ArrivalPort,
            v.PassengerCount,
            v.Status
        )).OrderBy(v => v.DepartureTime).ToList();

        return new VoyageScheduleSnapshot(
            schedule,
            schedule.Sum(v => v.PassengerCount),
            _voyages.Sum(v => v.CrewCount)
        );
    }

    public MaritimeInsightsOverview GetPlatformInsights()
    {
        return new MaritimeInsightsOverview(
            GetFleetAnalytics(),
            GetEnvironmentalSummary(),
            GetVoyageSchedule(),
            _aiModels.Select(MaritimeAiModelSummary.From).ToList(),
            DateTime.UtcNow
        );
    }

    public bool TryUpdateVesselStatus(int vesselId, VesselStatus status, string? notes, out Vessel updatedVessel)
    {
        var vessel = _fleet.FirstOrDefault(v => v.Id == vesselId);
        if (vessel is null)
        {
            updatedVessel = default!;
            return false;
        }

        vessel.Status = status;
        vessel.LastUpdated = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            _operationalAlerts.Add(new OperationalAlert(
                $"Status update for {vessel.Name}",
                "Info",
                notes!,
                DateTime.UtcNow
            ));
        }

        updatedVessel = CloneVessel(vessel);
        return true;
    }

    public VesselEmergencyResponse DeclareEmergency(int vesselId, VesselEmergencyReport report)
    {
        var vessel = _fleet.FirstOrDefault(v => v.Id == vesselId)
            ?? throw new ArgumentException($"Vessel {vesselId} not found", nameof(vesselId));

        vessel.Status = VesselStatus.Emergency;
        vessel.LastUpdated = DateTime.UtcNow;

        var position = _positions.FirstOrDefault(p => p.VesselId == vesselId);
        if (position is not null)
        {
            position.IsEmergency = true;
            position.Timestamp = DateTime.UtcNow;
        }

        var alert = new OperationalAlert(
            $"Emergency reported on {vessel.Name}",
            "Critical",
            string.IsNullOrWhiteSpace(report.Description)
                ? $"{report.Type} emergency reported by {report.ReportedBy}"
                : report.Description,
            DateTime.UtcNow
        );

        _operationalAlerts.Add(alert);

        return new VesselEmergencyResponse
        {
            VesselName = vessel.Name,
            Acknowledged = true,
            Response = "Emergency services have been notified. Coast Guard alerted.",
            Timestamp = DateTime.UtcNow
        };
    }

    private string GetRouteName(int routeId)
    {
        return routeId switch
        {
            1 => "Bergen-Kirkenes Northbound",
            2 => "Kirkenes-Bergen Southbound",
            _ => "Unknown Route"
        };
    }

    private static IReadOnlyList<OperationalAlert> BuildAlerts(IEnumerable<VesselPositionData> positions)
    {
        var alerts = new List<OperationalAlert>();

        foreach (var position in positions)
        {
            if (position.WaveHeightM.HasValue && position.WaveHeightM.Value > 3.0M)
            {
                alerts.Add(new OperationalAlert(
                    $"High seas for {position.MMSINumber}",
                    "Warning",
                    $"Wave height {position.WaveHeightM:0.0}m near {position.CurrentPort}",
                    position.Timestamp
                ));
            }

            if (position.IsEmergency)
            {
                alerts.Add(new OperationalAlert(
                    $"Emergency reported for {position.MMSINumber}",
                    "Critical",
                    "Automatic distress beacon triggered.",
                    position.Timestamp
                ));
            }
        }

        return alerts;
    }

    private static Vessel CloneVessel(Vessel vessel)
    {
        return new Vessel
        {
            Id = vessel.Id,
            Name = vessel.Name,
            IMONumber = vessel.IMONumber,
            CallSign = vessel.CallSign,
            Type = vessel.Type,
            PassengerCapacity = vessel.PassengerCapacity,
            CrewCapacity = vessel.CrewCapacity,
            LengthMeters = vessel.LengthMeters,
            BeamMeters = vessel.BeamMeters,
            DraftMeters = vessel.DraftMeters,
            GrossTonnage = vessel.GrossTonnage,
            Status = vessel.Status,
            LastUpdated = vessel.LastUpdated,
            CurrentRouteId = vessel.CurrentRouteId,
            CurrentRoute = vessel.CurrentRoute,
            CurrentPosition = vessel.CurrentPosition == null ? null : ClonePosition(vessel.CurrentPosition)
        };
    }

    private static Position ClonePosition(Position position)
    {
        return new Position
        {
            Id = position.Id,
            Latitude = position.Latitude,
            Longitude = position.Longitude,
            SpeedKnots = position.SpeedKnots,
            HeadingDegrees = position.HeadingDegrees,
            Timestamp = position.Timestamp,
            WindSpeedKnots = position.WindSpeedKnots,
            WaveHeightMeters = position.WaveHeightMeters,
            VisibilityKm = position.VisibilityKm
        };
    }

    private static VesselPositionData ClonePosition(VesselPositionData position)
    {
        return new VesselPositionData
        {
            Id = position.Id,
            VesselId = position.VesselId,
            Latitude = position.Latitude,
            Longitude = position.Longitude,
            SpeedKnots = position.SpeedKnots,
            HeadingDegrees = position.HeadingDegrees,
            Timestamp = position.Timestamp,
            CurrentPort = position.CurrentPort,
            NextPort = position.NextPort,
            EstimatedArrival = position.EstimatedArrival,
            WindSpeedKnots = position.WindSpeedKnots,
            WaveHeightM = position.WaveHeightM,
            VisibilityKm = position.VisibilityKm,
            WeatherCondition = position.WeatherCondition,
            MMSINumber = position.MMSINumber,
            NavigationStatus = position.NavigationStatus,
            IsEmergency = position.IsEmergency
        };
    }
}

internal static class MaritimeServiceExtensions
{
    public static TOut? Let<TIn, TOut>(this TIn? source, Func<TIn, TOut> selector) where TIn : class where TOut : class
    {
        return source == null ? null : selector(source);
    }
}
