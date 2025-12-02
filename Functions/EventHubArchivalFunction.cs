using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MaritimeIQ.Platform.Services.Interfaces;
using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Functions
{
    /// <summary>
    /// Event Hub Archival Function - Archives streaming data to files for batch processing
    /// Bridges the gap between real-time streaming and batch processing requirements
    /// </summary>
    public class EventHubArchivalFunction
    {
        private readonly ILogger<EventHubArchivalFunction> _logger;
        private readonly IDataLakeService _dataLakeService;

        public EventHubArchivalFunction(
            ILogger<EventHubArchivalFunction> logger,
            IDataLakeService dataLakeService)
        {
            _logger = logger;
            _dataLakeService = dataLakeService;
        }

        /// <summary>
        /// Archive AIS data from Event Hub to Data Lake files
        /// Triggered by Event Hub messages for continuous archival
        /// </summary>
        [Function("ArchiveAISDataToFiles")]
        public async Task ArchiveAISDataToFiles(
            [EventHubTrigger("ais-data-stream", Connection = "EventHubConnectionString")] string[] events,
            FunctionContext context)
        {
            _logger.LogInformation($"Archiving {events.Length} AIS messages to Data Lake files");

            var archivedCount = 0;
            var errorCount = 0;

            foreach (string eventData in events)
            {
                try
                {
                    var aisMessage = JsonSerializer.Deserialize<AISMessage>(eventData);
                    if (aisMessage != null)
                    {
                        // Convert single message to batch format for consistency
                        var batch = new AISDataBatch
                        {
                            BatchId = Guid.NewGuid().ToString(),
                            Messages = new[] { aisMessage },
                            Timestamp = DateTime.UtcNow,
                            Source = "EventHub-Archive"
                        };

                        // Save to Data Lake with timestamp-based path
                        var filePath = await _dataLakeService.SaveAISBatchAsync(
                            batch, 
                            "/mnt/maritime/raw/ais_archive/"
                        );

                        archivedCount++;
                        _logger.LogDebug($"Archived AIS message from vessel {aisMessage.VesselName} to {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error archiving AIS message: {eventData}");
                    errorCount++;
                }
            }

            _logger.LogInformation($"Event Hub archival completed: {archivedCount} archived, {errorCount} errors");
        }

        /// <summary>
        /// Archive environmental sensor data from Event Hub
        /// </summary>
        [Function("ArchiveEnvironmentalDataToFiles")]
        public async Task ArchiveEnvironmentalDataToFiles(
            [EventHubTrigger("environmental-sensors", Connection = "EventHubConnectionString")] string[] events,
            FunctionContext context)
        {
            _logger.LogInformation($"Archiving {events.Length} environmental sensor messages to Data Lake files");

            var archivedCount = 0;
            var errorCount = 0;

            foreach (string eventData in events)
            {
                try
                {
                    var sensorReading = JsonSerializer.Deserialize<EnvironmentalSensorReading>(eventData);
                    if (sensorReading != null)
                    {
                        // Convert to batch format
                        var batch = new EnvironmentalDataBatch
                        {
                            BatchId = Guid.NewGuid().ToString(),
                            Readings = new List<EnvironmentalSensorReading> { sensorReading },
                            Timestamp = DateTime.UtcNow,
                            Source = "EventHub-Archive"
                        };

                        var filePath = await _dataLakeService.SaveEnvironmentalBatchAsync(
                            batch,
                            "/mnt/maritime/raw/environmental_archive/"
                        );

                        archivedCount++;
                        _logger.LogDebug($"Archived environmental reading from vessel {sensorReading.VesselId} to {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error archiving environmental message: {eventData}");
                    errorCount++;
                }
            }

            _logger.LogInformation($"Environmental data archival completed: {archivedCount} archived, {errorCount} errors");
        }

        /// <summary>
        /// Timer-triggered function to consolidate small archive files into larger batches
        /// Runs every hour to optimize file sizes for batch processing
        /// </summary>
        [Function("ConsolidateArchiveFiles")]
        public async Task ConsolidateArchiveFiles(
            [TimerTrigger("0 0 * * * *")] TimerInfo timer, // Every hour
            FunctionContext context)
        {
            _logger.LogInformation("Starting archive file consolidation");

            try
            {
                var currentHour = DateTime.UtcNow.AddHours(-1); // Process previous hour
                var archivePath = $"/mnt/maritime/raw/ais_archive/{currentHour:yyyy/MM/dd/HH}";

                // List all files from the previous hour
                var files = await _dataLakeService.ListBatchFilesAsync(archivePath, "*.csv");
                
                if (files.Count > 1)
                {
                    _logger.LogInformation($"Consolidating {files.Count} archive files from {archivePath}");
                    
                    // In a real implementation, you would:
                    // 1. Read all small files
                    // 2. Combine them into larger files
                    // 3. Delete the small files
                    // 4. Save consolidated files
                    
                    // For now, just log the consolidation opportunity
                    _logger.LogInformation($"Archive consolidation opportunity: {files.Count} files in {archivePath}");
                }

                _logger.LogInformation("Archive file consolidation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during archive file consolidation");
            }
        }

        /// <summary>
        /// Health check function for archival system
        /// </summary>
        [Function("ArchivalHealthCheck")]
        public async Task ArchivalHealthCheck(
            [TimerTrigger("0 */15 * * * *")] TimerInfo timer, // Every 15 minutes
            FunctionContext context)
        {
            _logger.LogInformation("Performing archival system health check");

            try
            {
                var isHealthy = await _dataLakeService.HealthCheckAsync();
                
                if (isHealthy)
                {
                    _logger.LogInformation("✅ Archival system health check passed");
                }
                else
                {
                    _logger.LogWarning("⚠️ Archival system health check failed - Data Lake connectivity issues");
                }

                // Check recent file creation
                var recentFiles = await _dataLakeService.ListBatchFilesAsync(
                    $"/mnt/maritime/raw/ais_archive/{DateTime.UtcNow:yyyy/MM/dd}",
                    "*.csv"
                );

                _logger.LogInformation($"Recent archive activity: {recentFiles.Count} files created today");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during archival health check");
            }
        }
    }

    // Models are now in Models/DataIngestionModels.cs to avoid duplication
}
