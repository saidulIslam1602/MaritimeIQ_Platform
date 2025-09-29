using System;

namespace MaritimeIQ.Platform.Functions
{
    // Shared models for Azure Functions to avoid duplicates

    public class WeatherAlert
    {
        public string AlertId { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double RadiusKm { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
    }

    // AIS Processing Models
    public class AISMessage
    {
        public string MMSI { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal SpeedOverGround { get; set; }
        public decimal CourseOverGround { get; set; }
        public decimal TrueHeading { get; set; }
        public string NavigationalStatus { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class WeatherCondition
    {
        public double WindSpeed { get; set; }
        public double Visibility { get; set; }
        public double AuroraKpIndex { get; set; }
    }

    // Environmental Monitoring Models
    public class EnvironmentalReading
    {
        public int VesselId { get; set; }
        public DateTime MeasurementTime { get; set; }
        public decimal CO2EmissionKg { get; set; }
        public decimal FuelConsumptionLiters { get; set; }
        public decimal PowerConsumptionKWh { get; set; }
        public decimal BatteryStateOfCharge { get; set; }
        public decimal WaterTemperature { get; set; }
        public decimal AirTemperature { get; set; }
    }

    public class EnvironmentalAnalysisRequest
    {
        public string AnalysisType { get; set; } = string.Empty;
        public int VesselId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string[] Metrics { get; set; } = Array.Empty<string>();
    }

    public class EnvironmentalComplianceData
    {
        public double TotalOperatingHours { get; set; }
        public double TotalNauticalMiles { get; set; }
        public double TotalFuelConsumed { get; set; }
        public double BatteryUsageHours { get; set; }
        public double TotalCO2 { get; set; }
        public double CO2PerNM { get; set; }
        public double TotalNOx { get; set; }
        public double NOxPerKWh { get; set; }
        public double TotalSOx { get; set; }
        public double WasteGenerated { get; set; }
        public double WaterConsumed { get; set; }
        public double WastewaterTreated { get; set; }
        public double HybridUtilization { get; set; }
        public double BatteryModeHours { get; set; }
        public double RegenerativeEnergy { get; set; }
    }

    // Passenger Notification Models
    public class PassengerInfo
    {
        public int PassengerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = string.Empty;
        public string CabinNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
    }

    public class VesselInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CrewCount { get; set; }
    }

    public class PersonalizedCommunication
    {
        public int PassengerId { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public string[] DeliveryMethods { get; set; } = Array.Empty<string>();
        public string Content { get; set; } = string.Empty;
        public string CallToAction { get; set; } = string.Empty;
    }

    // Route Optimization Models
    public class RouteOptimizationRequest
    {
        public int VesselId { get; set; }
        public string DeparturePort { get; set; } = string.Empty;
        public string ArrivalPort { get; set; } = string.Empty;
        public DateTime PlannedDeparture { get; set; }
        public int PassengerCount { get; set; }
        public bool OptimizeForFuel { get; set; } = true;
        public bool OptimizeForTime { get; set; } = false;
        public bool OptimizeForComfort { get; set; } = true;
        public bool OptimizeForAurora { get; set; } = false;
        public string[] Priorities { get; set; } = Array.Empty<string>();
    }
}