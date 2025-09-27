using System.ComponentModel.DataAnnotations;

namespace HavilaKystruten.Maritime.Models
{
    // Core vessel information
    public class Vessel
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string IMONumber { get; set; } = string.Empty; // International Maritime Organization number
        
        public string CallSign { get; set; } = string.Empty;
        
        public VesselType Type { get; set; }
        
        public int PassengerCapacity { get; set; }
        
        public int CrewCapacity { get; set; }
        
        public double LengthMeters { get; set; }
        
        public double BeamMeters { get; set; }
        
        public double DraftMeters { get; set; }
        
        public double GrossTonnage { get; set; }
        
        public VesselStatus Status { get; set; }
        
        public DateTime LastUpdated { get; set; }
        
        // Current position
        public Position? CurrentPosition { get; set; }
        
        // Current route assignment
        public int? CurrentRouteId { get; set; }
        public Route? CurrentRoute { get; set; }
    }

    public enum VesselType
    {
        PassengerFerry,
        CargoFerry,
        CruiseShip,
        ExpeditionVessel,
        ServiceVessel
    }

    public enum VesselStatus
    {
        InService,
        InPort,
        Maintenance,
        Emergency,
        OutOfService
    }

    // GPS position with maritime-specific data
    public class Position
    {
        public int Id { get; set; }
        
        [Range(-90, 90)]
        public double Latitude { get; set; }
        
        [Range(-180, 180)]
        public double Longitude { get; set; }
        
        public double? SpeedKnots { get; set; }
        
        public double? HeadingDegrees { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        // Weather conditions
        public double? WindSpeedKnots { get; set; }
        public double? WaveHeightMeters { get; set; }
        public double? VisibilityKm { get; set; }
    }

    // Route information for Norwegian coastal services
    public class Route
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty; // e.g., "Bergen-Kirkenes"
        
        public string Description { get; set; } = string.Empty;
        
        public List<Port> Ports { get; set; } = new List<Port>();
        
        public TimeSpan EstimatedDuration { get; set; }
        
        public double DistanceNauticalMiles { get; set; }
        
        public bool IsActive { get; set; }
        
        public RouteType Type { get; set; }
    }

    public enum RouteType
    {
        CoastalRoute,
        FjordRoute,
        ArcticRoute,
        InternationalRoute
    }

    // Norwegian ports and terminals
    public class Port
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty; // e.g., "Bergen", "Tromsø"
        
        public string UNLocode { get; set; } = string.Empty; // UN/LOCODE (e.g., "NOBGO" for Bergen)
        
        public Position Position { get; set; } = new Position();
        
        public List<Terminal> Terminals { get; set; } = new List<Terminal>();
        
        public bool HasPassengerFacilities { get; set; }
        
        public bool HasCargoFacilities { get; set; }
        
        public int MaxVesselLength { get; set; }
        
        public double MaxDraft { get; set; }
    }

    public class Terminal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TerminalType Type { get; set; }
        public bool IsAvailable { get; set; }
    }

    public enum TerminalType
    {
        Passenger,
        Vehicle,
        Cargo,
        Fuel,
        Maintenance
    }
}