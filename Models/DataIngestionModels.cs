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
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    // Additional batch models for comprehensive data ingestion
    public class VesselTelemetryBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public DateTime BatchTime { get; set; } = DateTime.UtcNow;
        public List<TelemetryReading> Readings { get; set; } = new();
        public string Source { get; set; } = string.Empty;
    }

    public class TelemetryReading
    {
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Quality { get; set; } = "Good";
    }

    public class EnginePerformanceBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public List<EngineReading> Readings { get; set; } = new();
        public DateTime BatchTime { get; set; } = DateTime.UtcNow;
    }

    public class EngineReading
    {
        public string EngineId { get; set; } = string.Empty;
        public double RPM { get; set; }
        public double Temperature { get; set; }
        public double FuelConsumption { get; set; }
        public double Power { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class NavigationDataBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public List<NavigationReading> Readings { get; set; } = new();
        public DateTime BatchTime { get; set; } = DateTime.UtcNow;
    }

    public class NavigationReading
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Course { get; set; }
        public double Speed { get; set; }
        public double Depth { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PassengerDataBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public List<PassengerUpdate> Updates { get; set; } = new();
        public DateTime BatchTime { get; set; } = DateTime.UtcNow;
    }

    public class PassengerUpdate
    {
        public string UpdateType { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class SafetySystemBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public List<SafetyReading> Readings { get; set; } = new();
        public DateTime BatchTime { get; set; } = DateTime.UtcNow;
    }

    public class SafetyReading
    {
        public string SystemName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsAlarm { get; set; }
        public string AlarmMessage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class CargoManifestBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public List<CargoItem> Items { get; set; } = new();
        public DateTime ManifestTime { get; set; } = DateTime.UtcNow;
    }

    public class CargoItem
    {
        public string ItemId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Weight { get; set; }
        public string Unit { get; set; } = "kg";
        public string Status { get; set; } = string.Empty;
    }

    // Result models for specific data types
    public class EngineDataResult : DataIngestionResult
    {
        public int MaintenanceAlerts { get; set; }
        public List<string> EngineIds { get; set; } = new();
    }

    public class NavigationDataResult : DataIngestionResult
    {
        public double DistanceCovered { get; set; }
        public int RouteDeviations { get; set; }
    }

    public class PassengerDataResult : DataIngestionResult
    {
        public int PassengerMovements { get; set; }
        public int CrewChanges { get; set; }
    }

    public class SafetyDataResult : DataIngestionResult
    {
        public int SafetyAlerts { get; set; }
        public List<string> CriticalAlerts { get; set; } = new();
    }

    public class CargoDataResult : DataIngestionResult
    {
        public double TotalWeight { get; set; }
        public int ItemsTracked { get; set; }
    }

    // Statistics and reporting models
    public class IngestionStatistics
    {
        public string VesselId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalBatches { get; set; }
        public int TotalRecords { get; set; }
        public double SuccessRate { get; set; }
        public Dictionary<string, int> DataTypeBreakdown { get; set; } = new();
        public TimeSpan AverageProcessingTime { get; set; }
    }

    public class DataQualityReport
    {
        public string VesselId { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double QualityScore { get; set; }
        public List<QualityIssue> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class QualityIssue
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    public class BatchImportResult
    {
        public string ImportId { get; set; } = Guid.NewGuid().ToString();
        public string DataSource { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int RejectedRecords { get; set; }
        public List<string> Errors { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    public class IngestionConfiguration
    {
        public string VesselId { get; set; } = string.Empty;
        public Dictionary<string, int> BatchSizes { get; set; } = new();
        public Dictionary<string, TimeSpan> IngestionIntervals { get; set; } = new();
        public List<string> EnabledDataTypes { get; set; } = new();
        public Dictionary<string, double> QualityThresholds { get; set; } = new();
        public bool AutoFailover { get; set; } = true;
    }
}
