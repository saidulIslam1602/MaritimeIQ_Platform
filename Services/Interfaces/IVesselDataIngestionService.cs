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
}