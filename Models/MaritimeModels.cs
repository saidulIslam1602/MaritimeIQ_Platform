using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HavilaKystruten.Maritime.Models
{
    // Chat models for maritime communication
    public class ChatRequest
    {
        public string Query { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
        public List<string> RelevantSources { get; set; } = new();
        public double Confidence { get; set; }
    }

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

    // ========================================
    // BERGEN-KIRKENES CRUISE SPECIFIC MODELS
    // ========================================

    public class BergenKirkenesRoute
    {
        public string RouteName { get; set; } = string.Empty;
        public double TotalDistance { get; set; } // nautical miles
        public int Duration { get; set; } // hours
        public string Operator { get; set; } = string.Empty;
        public List<string> Vessels { get; set; } = new List<string>();
        public List<CoastalPort> Ports { get; set; } = new List<CoastalPort>();
        public SeasonalSchedule SeasonalSchedule { get; set; } = new SeasonalSchedule();
    }

    public class CoastalPort
    {
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int StopDuration { get; set; } // minutes
        public List<string> Attractions { get; set; } = new List<string>();
        public bool IsNorthernLightsViewingPort { get; set; }
        public bool IsMidnightSunPort { get; set; }
    }

    public class VesselPosition
    {
        public string VesselName { get; set; } = string.Empty;
        public string CurrentPort { get; set; } = string.Empty;
        public string NextPort { get; set; } = string.Empty;
        public DateTime EstimatedArrival { get; set; }
        public int PassengerCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public string WeatherCondition { get; set; } = string.Empty;
        public string NorthernLightsVisibility { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; } // knots
        public double Heading { get; set; } // degrees
    }

    public class NorthernLightsForecast
    {
        public DateTime ForecastDate { get; set; }
        public double KpIndex { get; set; } // Aurora activity level (0-9)
        public string OverallVisibility { get; set; } = string.Empty;
        public List<AuroraViewingSpot> BestViewingPorts { get; set; } = new List<AuroraViewingSpot>();
        public string SolarActivity { get; set; } = string.Empty;
        public string WeatherPattern { get; set; } = string.Empty;
    }

    public class AuroraViewingSpot
    {
        public string PortName { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }
        public string VisibilityRating { get; set; } = string.Empty; // Excellent, Very Good, Good, Fair, Poor
        public int CloudCover { get; set; } // percentage
        public string MoonPhase { get; set; } = string.Empty;
        public double OptimalViewingTime { get; set; } // hours after sunset
        public List<string> RecommendedViewingLocations { get; set; } = new List<string>();
    }

    public class PortExperience
    {
        public string PortName { get; set; } = string.Empty;
        public List<string> Highlights { get; set; } = new List<string>();
        public string LocalWeather { get; set; } = string.Empty;
        public string BestPhotography { get; set; } = string.Empty;
        public string LocalCuisine { get; set; } = string.Empty;
        public int ShoreExcursionDuration { get; set; } // minutes
        public List<string> Activities { get; set; } = new List<string>();
        public string CulturalSignificance { get; set; } = string.Empty;
        public List<string> LanguageNotes { get; set; } = new List<string>();
        public string Currency { get; set; } = "Norwegian Krone (NOK)";
    }

    public class SeasonalSchedule
    {
        public SeasonSchedule WinterSchedule { get; set; } = new SeasonSchedule();
        public SeasonSchedule SummerSchedule { get; set; } = new SeasonSchedule();
        public List<SpecialCruise> SpecialCruises { get; set; } = new List<SpecialCruise>();
    }

    public class SeasonSchedule
    {
        public string Season { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public List<string> SpecialFeatures { get; set; } = new List<string>();
        public string WeatherConsiderations { get; set; } = string.Empty;
        public double AverageOccupancy { get; set; } // percentage
        public List<string> RecommendedActivities { get; set; } = new List<string>();
    }

    public class SpecialCruise
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Theme { get; set; } = string.Empty; // e.g., "Northern Lights", "Midnight Sun", "Christmas"
        public double PremiumRate { get; set; } // percentage above normal rate
        public List<string> IncludedExtras { get; set; } = new List<string>();
    }

    public class EmergencyAlert
    {
        public string VesselName { get; set; } = string.Empty;
        public string EmergencyType { get; set; } = string.Empty; // Medical, Technical, Weather, Security
        public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Description { get; set; } = string.Empty;
        public VesselPosition VesselPosition { get; set; } = new VesselPosition();
        public DateTime AlertTime { get; set; }
        public int PassengersOnBoard { get; set; }
        public int CrewOnBoard { get; set; }
        public string WeatherConditions { get; set; } = string.Empty;
    }

    public class EmergencyResponse
    {
        public Guid AlertId { get; set; }
        public DateTime ResponseTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int EstimatedResponseTime { get; set; } // minutes
        public string NearestRescueVessel { get; set; } = string.Empty;
        public List<string> EmergencyContacts { get; set; } = new List<string>();
        public string CoordinatingAuthority { get; set; } = string.Empty;
        public List<string> ResponseActions { get; set; } = new List<string>();
    }

    // Passenger experience and booking models
    public class CruiseBooking
    {
        public Guid BookingId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public string DeparturePort { get; set; } = string.Empty; // Bergen or Kirkenes
        public string CabinType { get; set; } = string.Empty;
        public int NumberOfPassengers { get; set; }
        public bool NorthernLightsPackage { get; set; }
        public bool MidnightSunPackage { get; set; }
        public List<string> SelectedExcursions { get; set; } = new List<string>();
        public double TotalPrice { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
    }

    public class WeatherForecast
    {
        public string Location { get; set; } = string.Empty;
        public DateTime ForecastDate { get; set; }
        public double Temperature { get; set; } // Celsius
        public double WindSpeed { get; set; } // knots
        public string WindDirection { get; set; } = string.Empty;
        public double WaveHeight { get; set; } // meters
        public int Visibility { get; set; } // nautical miles
        public string SeaConditions { get; set; } = string.Empty;
        public string SuitabilityForShoreExcursions { get; set; } = string.Empty;
    }

    // ========================================
    // DATABASE SCHEMA MODELS - From Step-by-Step Guide Part 2
    // ========================================

    // Voyages table - Bergen-Kirkenes routes tracking
    [Table("voyages")]
    public class Voyage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int VesselId { get; set; }
        
        [ForeignKey("VesselId")]
        public virtual Vessel Vessel { get; set; } = null!;
        
        [Required]
        public int RouteId { get; set; }
        
        [ForeignKey("RouteId")]
        public virtual Route Route { get; set; } = null!;
        
        public DateTime DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string DeparturePort { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string ArrivalPort { get; set; } = string.Empty;
        
        public int PassengerCount { get; set; }
        public int CrewCount { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelConsumptionLiters { get; set; }
        
        [Column(TypeName = "decimal(8,2)")]
        public decimal DistanceCoveredNM { get; set; }
        
        public VoyageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<EnvironmentalData> EnvironmentalData { get; set; } = new List<EnvironmentalData>();
    }
    
    public enum VoyageStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Delayed,
        Cancelled
    }

    // Environmental data table - CO2, fuel consumption tracking
    [Table("environmental_data")]
    public class EnvironmentalData
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int VesselId { get; set; }
        
        [ForeignKey("VesselId")]
        public virtual Vessel Vessel { get; set; } = null!;
        
        public int? VoyageId { get; set; }
        
        [ForeignKey("VoyageId")]
        public virtual Voyage? Voyage { get; set; }
        
        public DateTime MeasurementTime { get; set; }
        
        [Column(TypeName = "decimal(8,2)")]
        public decimal CO2EmissionKg { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelConsumptionLiters { get; set; }
        
        [Column(TypeName = "decimal(6,2)")]
        public decimal PowerConsumptionKWh { get; set; }
        
        [Column(TypeName = "decimal(8,2)")]
        public decimal BatteryStateOfCharge { get; set; } // For Havila's hybrid vessels
        
        [Column(TypeName = "decimal(6,2)")]
        public decimal WaterTemperature { get; set; }
        
        [Column(TypeName = "decimal(6,2)")]
        public decimal AirTemperature { get; set; }
        
        [Column(TypeName = "decimal(6,2)")]
        public decimal WindSpeedKnots { get; set; }
        
        [Column(TypeName = "decimal(6,2)")]
        public decimal WaveHeightMeters { get; set; }
        
        public bool ComplianceStatus { get; set; } = true;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    // AI Models table - ML model management
    [Table("ai_models")]
    public class AIModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string ModelType { get; set; } = string.Empty; // RouteOptimization, WeatherPrediction, FuelOptimization
        
        [MaxLength(20)]
        public string Version { get; set; } = "1.0";
        
        public DateTime TrainingDate { get; set; }
        public DateTime? LastUsed { get; set; }
        
        [Column(TypeName = "decimal(5,4)")]
        public decimal Accuracy { get; set; }
        
        [MaxLength(500)]
        public string ModelPath { get; set; } = string.Empty; // Azure Blob Storage path
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Performance metrics
        [Column(TypeName = "decimal(10,2)")]
        public decimal ProcessingTimeMs { get; set; }
        
        public int UsageCount { get; set; } = 0;
        
        [MaxLength(2000)]
        public string ConfigurationJson { get; set; } = string.Empty;
    }

    // Enhanced Ports table with Norwegian coastal ports
    [Table("ports")]
    public class EnhancedPort
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string UNLocode { get; set; } = string.Empty; // e.g., "NOBGO" for Bergen
        
        [Column(TypeName = "decimal(10,7)")]
        public decimal Latitude { get; set; }
        
        [Column(TypeName = "decimal(10,7)")]
        public decimal Longitude { get; set; }
        
        [MaxLength(50)]
        public string Country { get; set; } = "Norway";
        
        [MaxLength(50)]
        public string Region { get; set; } = string.Empty; // e.g., "Northern Norway", "Western Norway"
        
        public bool HasPassengerFacilities { get; set; }
        public bool HasCargoFacilities { get; set; }
        public bool HasFuelStation { get; set; }
        public bool HasMaintenance { get; set; }
        
        [Column(TypeName = "decimal(6,2)")]
        public decimal MaxVesselLengthM { get; set; }
        
        [Column(TypeName = "decimal(4,2)")]
        public decimal MaxDraftM { get; set; }
        
        public int TideRange { get; set; } // in centimeters
        
        public bool IsNorthernLightsPort { get; set; }
        public bool IsMidnightSunPort { get; set; }
        
        [MaxLength(1000)]
        public string Attractions { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string WeatherConsiderations { get; set; } = string.Empty;
        
        public int AverageStopDurationMinutes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // Real-time vessel tracking and AIS data
    [Table("vessel_positions")]
    public class VesselPositionData
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int VesselId { get; set; }
        
        [ForeignKey("VesselId")]
        public virtual Vessel Vessel { get; set; } = null!;
        
        [Column(TypeName = "decimal(10,7)")]
        public decimal Latitude { get; set; }
        
        [Column(TypeName = "decimal(10,7)")]
        public decimal Longitude { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal SpeedKnots { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal HeadingDegrees { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        [MaxLength(50)]
        public string CurrentPort { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string NextPort { get; set; } = string.Empty;
        
        public DateTime? EstimatedArrival { get; set; }
        
        [Column(TypeName = "decimal(6,2)")]
        public decimal? WindSpeedKnots { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? WaveHeightM { get; set; }
        
        [Column(TypeName = "decimal(4,1)")]
        public decimal? VisibilityKm { get; set; }
        
        [MaxLength(100)]
        public string WeatherCondition { get; set; } = string.Empty;
        
        // AIS-specific data
        [MaxLength(20)]
        public string MMSINumber { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string NavigationStatus { get; set; } = string.Empty;
        
        public bool IsEmergency { get; set; } = false;
    }

    // Passenger and booking management
    [Table("bookings")]
    public class BookingData
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string PassengerName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public int VoyageId { get; set; }
        
        [ForeignKey("VoyageId")]
        public virtual Voyage Voyage { get; set; } = null!;
        
        [MaxLength(50)]
        public string CabinType { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string CabinNumber { get; set; } = string.Empty;
        
        public int PassengerCount { get; set; } = 1;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
        
        [MaxLength(20)]
        public string Currency { get; set; } = "NOK";
        
        public BookingStatus Status { get; set; }
        
        public bool NorthernLightsPackage { get; set; }
        public bool MidnightSunPackage { get; set; }
        
        [MaxLength(1000)]
        public string SelectedExcursions { get; set; } = string.Empty; // JSON array
        
        [MaxLength(2000)]
        public string SpecialRequests { get; set; } = string.Empty;
        
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string PaymentReference { get; set; } = string.Empty;
    }
    
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        CheckedIn,
        Completed,
        Cancelled,
        Refunded
    }

    // Weather and environmental conditions
    [Table("weather_data")]
    public class WeatherData
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(10,7)")]
        public decimal Latitude { get; set; }
        
        [Column(TypeName = "decimal(10,7)")]
        public decimal Longitude { get; set; }
        
        public DateTime ForecastTime { get; set; }
        public DateTime ValidTime { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal TemperatureC { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal WindSpeedKnots { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal WindDirectionDegrees { get; set; }
        
        [Column(TypeName = "decimal(4,2)")]
        public decimal WaveHeightM { get; set; }
        
        [Column(TypeName = "decimal(4,1)")]
        public decimal VisibilityKm { get; set; }
        
        [Column(TypeName = "decimal(6,2)")]
        public decimal BarometricPressure { get; set; }
        
        [Column(TypeName = "decimal(3,0)")]
        public decimal Humidity { get; set; }
        
        [Column(TypeName = "decimal(3,0)")]
        public decimal CloudCover { get; set; }
        
        [MaxLength(100)]
        public string Conditions { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(2,1)")]
        public decimal UVIndex { get; set; }
        
        // Northern Lights specific
        [Column(TypeName = "decimal(2,1)")]
        public decimal AuroraKpIndex { get; set; }
        
        [MaxLength(50)]
        public string AuroraVisibility { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string SeaConditions { get; set; } = string.Empty;
        
        public bool SuitableForShoreExcursions { get; set; } = true;
    }

    // IoT sensor data for vessels
    [Table("sensor_data")]
    public class SensorData
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int VesselId { get; set; }
        
        [ForeignKey("VesselId")]
        public virtual Vessel Vessel { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string SensorType { get; set; } = string.Empty; // Engine, Battery, Navigation, Safety
        
        [Required]
        [MaxLength(100)]
        public string SensorLocation { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string MetricName { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(12,4)")]
        public decimal Value { get; set; }
        
        [MaxLength(20)]
        public string Unit { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        [Column(TypeName = "decimal(12,4)")]
        public decimal? MinThreshold { get; set; }
        
        [Column(TypeName = "decimal(12,4)")]
        public decimal? MaxThreshold { get; set; }
        
        public bool IsAlert { get; set; } = false;
        
        [MaxLength(500)]
        public string AlertMessage { get; set; } = string.Empty;
        
        public SensorStatus Status { get; set; } = SensorStatus.Normal;
    }
    
    public enum SensorStatus
    {
        Normal,
        Warning,
        Critical,
        Offline,
        Maintenance
    }

    // Real-time sensor reading model for data ingestion
    public class SensorReading
    {
        public string SensorType { get; set; } = string.Empty;
        public string SensorLocation { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? MaxThreshold { get; set; }
        public bool IsAlert { get; set; }
        public string AlertMessage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    // Batch model for sensor data ingestion
    public class SensorDataBatch
    {
        public int VesselId { get; set; }
        public SensorReading[] Readings { get; set; } = Array.Empty<SensorReading>();
        public DateTime BatchTime { get; set; }
    }

    // Voyage information shared across services, controllers, and Azure Functions
    public class VoyageInfo
    {
        public string VoyageId { get; set; } = string.Empty;
        public int? VesselId { get; set; } = null;
        public string VesselName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public VoyagePosition? CurrentPosition { get; set; } = null;
        public string Destination { get; set; } = string.Empty;
        public DateTime? EstimatedArrival { get; set; } = null;
        public int? PassengerCount { get; set; } = null;
        public DateTime? DepartureDate { get; set; } = null;
    }

    public class VoyagePosition
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string NearestPort { get; set; } = string.Empty;
        public decimal? SpeedKnots { get; set; } = null;
        public decimal? HeadingDegrees { get; set; } = null;
    }

    // Search results model used by Maritime Intelligence and Search controllers
    public class MaritimeSearchResult
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}