using System.ComponentModel.DataAnnotations;

namespace MaritimeIQ.Platform.Models
{
    /// <summary>
    /// Data models for maritime data ingestion
    /// Consolidated models to avoid duplication across controllers and services
    /// </summary>

    public class AISDataBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public AISMessage[] Messages { get; set; } = Array.Empty<AISMessage>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = "API";
    }

    public class AISMessage
    {
        [Required]
        public string MMSI { get; set; } = string.Empty;
        
        public string VesselName { get; set; } = string.Empty;
        
        [Range(-90.0, 90.0)]
        public double Latitude { get; set; }
        
        [Range(-180.0, 180.0)]
        public double Longitude { get; set; }
        
        [Range(0.0, 40.0)]
        public double SpeedOverGround { get; set; }
        
        [Range(0.0, 359.9)]
        public double CourseOverGround { get; set; }
        
        [Range(0, 359)]
        public int TrueHeading { get; set; }
        
        public string NavigationalStatus { get; set; } = string.Empty;
        
        public double RateOfTurn { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
    }

    public class EnvironmentalDataBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public List<EnvironmentalSensorReading> Readings { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = "API";
    }

    public class EnvironmentalSensorReading
    {
        [Required]
        public int VesselId { get; set; }
        
        [Required]
        public DateTime MeasurementTime { get; set; }
        
        [Range(0.0, 1000.0)]
        public double CO2EmissionKg { get; set; }
        
        [Range(0.0, 100.0)]
        public double NOxEmissionKg { get; set; }
        
        [Range(0.0, 10.0)]
        public double SOxEmissionKg { get; set; }
        
        [Range(0.0, 10000.0)]
        public double FuelConsumptionLiters { get; set; }
        
        [Range(0.0, 100.0)]
        public double BatteryStateOfCharge { get; set; }
        
        public double WaterTemperature { get; set; }
        
        public double AirTemperature { get; set; }
        
        public string Parameter { get; set; } = string.Empty;
        
        public double Value { get; set; }
    }

    public class VoyageData
    {
        [Required]
        public string VoyageId { get; set; } = string.Empty;
        
        [Required]
        public int VesselId { get; set; }
        
        public string DeparturePort { get; set; } = string.Empty;
        
        public string ArrivalPort { get; set; } = string.Empty;
        
        public DateTime? DepartureTime { get; set; }
        
        public DateTime? ArrivalTime { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        [Range(0, 5000)]
        public int PassengerCount { get; set; }
        
        [Range(0.0, 1000000.0)]
        public double CargoWeight { get; set; }
    }

    // Processing result models
    public class AISProcessingResult
    {
        public string BatchId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsRejected { get; set; }
        public int VesselsTracked { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class EnvironmentalIngestionResult
    {
        public string BatchId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsRejected { get; set; }
        public int ComplianceViolations { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class DataIngestionResult
    {
        public string BatchId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsRejected { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
