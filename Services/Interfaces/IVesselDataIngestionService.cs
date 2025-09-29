using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for high-volume vessel data ingestion and processing
    /// </summary>
    public interface IVesselDataIngestionService
    {
        /// <summary>
        /// Ingest real-time vessel telemetry data
        /// </summary>
        Task<DataIngestionResult> IngestVesselTelemetryAsync(string vesselId, VesselTelemetryBatch telemetryBatch);

        /// <summary>
        /// Process AIS data stream for vessel tracking
        /// </summary>
        Task<AISProcessingResult> ProcessAISDataStreamAsync(AISDataBatch aisDataBatch);

        /// <summary>
        /// Ingest environmental sensor data from vessels
        /// </summary>
        Task<EnvironmentalIngestionResult> IngestEnvironmentalDataAsync(string vesselId, EnvironmentalDataBatch environmentalData);

        /// <summary>
        /// Process engine performance data
        /// </summary>
        Task<EngineDataResult> IngestEnginePerformanceDataAsync(string vesselId, EnginePerformanceBatch engineData);

        /// <summary>
        /// Ingest navigation and GPS data
        /// </summary>
        Task<NavigationDataResult> IngestNavigationDataAsync(string vesselId, NavigationDataBatch navigationData);

        /// <summary>
        /// Process passenger and crew data updates
        /// </summary>
        Task<PassengerDataResult> IngestPassengerDataAsync(string vesselId, PassengerDataBatch passengerData);

        /// <summary>
        /// Ingest safety system data and alerts
        /// </summary>
        Task<SafetyDataResult> IngestSafetySystemDataAsync(string vesselId, SafetySystemBatch safetyData);

        /// <summary>
        /// Process cargo and manifest information
        /// </summary>
        Task<CargoDataResult> IngestCargoDataAsync(string vesselId, CargoManifestBatch cargoData);

        /// <summary>
        /// Get ingestion statistics and health metrics
        /// </summary>
        Task<IngestionStatistics> GetIngestionStatisticsAsync(string? vesselId = null, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// Validate data quality and integrity
        /// </summary>
        Task<DataQualityReport> ValidateDataQualityAsync(string vesselId, string dataType, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Process batch data import from external systems
        /// </summary>
        Task<BatchImportResult> ProcessBatchImportAsync(string dataSource, string dataType, Stream dataStream);

        /// <summary>
        /// Configure data ingestion settings for a vessel
        /// </summary>
        Task<bool> ConfigureIngestionSettingsAsync(string vesselId, IngestionConfiguration configuration);
    }

    #region Supporting Models

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

    public class AISDataBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public List<AISMessage> Messages { get; set; } = new();
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = string.Empty;
    }

    public class AISMessage
    {
        public string MMSI { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        public string NavigationStatus { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class EnvironmentalDataBatch
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public string VesselId { get; set; } = string.Empty;
        public List<EnvironmentalReading> Readings { get; set; } = new();
        public DateTime BatchTime { get; set; } = DateTime.UtcNow;
    }

    public class EnvironmentalReading
    {
        public string Parameter { get; set; } = string.Empty; // CO2, NOx, SOx, etc.
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Location { get; set; } = string.Empty;
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
        public string UpdateType { get; set; } = string.Empty; // Boarding, Disembarking, etc.
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

    public class DataIngestionResult
    {
        public string BatchId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsRejected { get; set; }
        public List<string> Errors { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    public class AISProcessingResult : DataIngestionResult
    {
        public int VesselsTracked { get; set; }
        public List<string> NewVessels { get; set; } = new();
        public List<string> AlertsGenerated { get; set; } = new();
    }

    public class EnvironmentalIngestionResult : DataIngestionResult
    {
        public int ComplianceViolations { get; set; }
        public List<string> ViolationTypes { get; set; } = new();
    }

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

    #endregion
}