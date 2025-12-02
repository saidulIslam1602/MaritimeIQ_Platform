using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MaritimeIQ.Platform.Models;
using MaritimeIQ.Platform.Services.Interfaces;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Azure Data Lake Storage service for maritime data
    /// Bridges API data to file storage for batch processing
    /// </summary>
    public class DataLakeService : BaseMaritimeService, IDataLakeService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly JsonSerializerOptions _jsonOptions;

        public override string ServiceName => "Data Lake Service";

        public DataLakeService(
            ILogger<DataLakeService> logger,
            IConfiguration configuration,
            BlobServiceClient blobServiceClient) : base(logger, configuration)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = configuration.GetValue<string>("DataLake:ContainerName") ?? "maritime-data";
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<string> SaveAISBatchAsync(AISDataBatch batch, string basePath = "/mnt/maritime/raw/ais_history/")
        {
            return await ExecuteOperationAsync(async () =>
            {
                var timestamp = DateTime.UtcNow;
                var fileName = $"ais_batch_{timestamp:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.json";
                var fullPath = $"{basePath.TrimEnd('/')}/{timestamp:yyyy/MM/dd}/{fileName}";
                
                LogInformation($"Saving AIS batch with {batch.Messages?.Length ?? 0} messages to {fullPath}");

                // Convert AIS batch to CSV-compatible format for Databricks ingestion
                var csvData = ConvertAISBatchToCsvFormat(batch);
                var csvFileName = fileName.Replace(".json", ".csv");
                var csvPath = fullPath.Replace(".json", ".csv");

                // Save both JSON (for audit) and CSV (for Databricks)
                await SaveToDataLakeAsync(fullPath, JsonSerializer.Serialize(batch, _jsonOptions));
                await SaveToDataLakeAsync(csvPath, csvData);

                LogInformation($"AIS batch saved successfully: {batch.Messages?.Length ?? 0} records");
                return csvPath; // Return CSV path for batch processing
            }, nameof(SaveAISBatchAsync));
        }

        public async Task<string> SaveEnvironmentalBatchAsync(EnvironmentalDataBatch batch, string basePath = "/mnt/maritime/raw/environmental/")
        {
            return await ExecuteOperationAsync(async () =>
            {
                var timestamp = DateTime.UtcNow;
                var fileName = $"environmental_batch_{timestamp:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.json";
                var fullPath = $"{basePath.TrimEnd('/')}/{timestamp:yyyy/MM/dd}/{fileName}";

                LogInformation($"Saving environmental batch with {batch.Readings?.Count ?? 0} readings to {fullPath}");

                await SaveToDataLakeAsync(fullPath, JsonSerializer.Serialize(batch, _jsonOptions));

                LogInformation($"Environmental batch saved successfully: {batch.Readings?.Count ?? 0} records");
                return fullPath;
            }, nameof(SaveEnvironmentalBatchAsync));
        }

        public async Task<string> SaveVoyageDataAsync(VoyageData voyage, string basePath = "/mnt/maritime/raw/voyages/")
        {
            return await ExecuteOperationAsync(async () =>
            {
                var timestamp = DateTime.UtcNow;
                var fileName = $"voyage_{voyage.VoyageId}_{timestamp:yyyyMMdd_HHmmss}.json";
                var fullPath = $"{basePath.TrimEnd('/')}/{timestamp:yyyy/MM/dd}/{fileName}";

                LogInformation($"Saving voyage data for voyage {voyage.VoyageId} to {fullPath}");

                await SaveToDataLakeAsync(fullPath, JsonSerializer.Serialize(voyage, _jsonOptions));

                LogInformation($"Voyage data saved successfully: {voyage.VoyageId}");
                return fullPath;
            }, nameof(SaveVoyageDataAsync));
        }

        public async Task<List<string>> ListBatchFilesAsync(string path, string pattern = "*.json")
        {
            return await ExecuteOperationAsync(async () =>
            {
                LogInformation($"Listing batch files in {path} with pattern {pattern}");

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobs = new List<string>();

                // Remove /mnt/ prefix for blob storage path
                var blobPath = path.Replace("/mnt/maritime/", "").TrimStart('/');

                await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: blobPath))
                {
                    if (MatchesPattern(blobItem.Name, pattern))
                    {
                        blobs.Add($"/mnt/maritime/{blobItem.Name}");
                    }
                }

                LogInformation($"Found {blobs.Count} batch files matching pattern {pattern}");
                return blobs;
            }, nameof(ListBatchFilesAsync));
        }

        public async Task<bool> HealthCheckAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                try
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                    await containerClient.GetPropertiesAsync();
                    
                    LogInformation("Data Lake health check passed");
                    return true;
                }
                catch (Exception ex)
                {
                    LogError($"Data Lake health check failed: {ex.Message}");
                    return false;
                }
            }, nameof(HealthCheckAsync));
        }

        private async Task SaveToDataLakeAsync(string path, string content)
        {
            // Convert /mnt/maritime/ path to blob storage path
            var blobPath = path.Replace("/mnt/maritime/", "").TrimStart('/');
            
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            
            var blobClient = containerClient.GetBlobClient(blobPath);
            
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            await blobClient.UploadAsync(stream, overwrite: true);
            
            // Set metadata for easier identification
            var metadata = new Dictionary<string, string>
            {
                ["source"] = "maritime-api",
                ["timestamp"] = DateTime.UtcNow.ToString("O"),
                ["content-type"] = path.EndsWith(".csv") ? "text/csv" : "application/json"
            };
            await blobClient.SetMetadataAsync(metadata);
        }

        private string ConvertAISBatchToCsvFormat(AISDataBatch batch)
        {
            var csv = new System.Text.StringBuilder();
            
            // CSV header matching Databricks schema
            csv.AppendLine("VesselName,MMSI,Latitude,Longitude,Speed,Heading,Timestamp");
            
            if (batch.Messages != null)
            {
                foreach (var message in batch.Messages)
                {
                    csv.AppendLine($"{EscapeCsv(message.VesselName ?? "")},{message.MMSI},{message.Latitude},{message.Longitude},{message.SpeedOverGround},{message.TrueHeading},{message.Timestamp:yyyy-MM-dd HH:mm:ss}");
                }
            }
            
            return csv.ToString();
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
                
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            
            return value;
        }

        private bool MatchesPattern(string fileName, string pattern)
        {
            if (pattern == "*" || pattern == "*.*")
                return true;
                
            if (pattern.StartsWith("*."))
            {
                var extension = pattern.Substring(1);
                return fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
            }
            
            return fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }
    }
}
