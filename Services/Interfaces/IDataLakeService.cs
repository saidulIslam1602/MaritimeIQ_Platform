using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Interface for Azure Data Lake Storage operations
    /// Handles saving API data to files for batch processing
    /// </summary>
    public interface IDataLakeService
    {
        /// <summary>
        /// Save AIS data batch to Data Lake for batch processing
        /// </summary>
        /// <param name="batch">AIS data batch from API</param>
        /// <param name="basePath">Base path in Data Lake</param>
        /// <returns>Full path where data was saved</returns>
        Task<string> SaveAISBatchAsync(AISDataBatch batch, string basePath = "/mnt/maritime/raw/ais_history/");

        /// <summary>
        /// Save environmental sensor data to Data Lake
        /// </summary>
        /// <param name="batch">Environmental data batch</param>
        /// <param name="basePath">Base path in Data Lake</param>
        /// <returns>Full path where data was saved</returns>
        Task<string> SaveEnvironmentalBatchAsync(EnvironmentalDataBatch batch, string basePath = "/mnt/maritime/raw/environmental/");

        /// <summary>
        /// Save voyage data to Data Lake
        /// </summary>
        /// <param name="voyage">Voyage data</param>
        /// <param name="basePath">Base path in Data Lake</param>
        /// <returns>Full path where data was saved</returns>
        Task<string> SaveVoyageDataAsync(VoyageData voyage, string basePath = "/mnt/maritime/raw/voyages/");

        /// <summary>
        /// List available batch files for processing
        /// </summary>
        /// <param name="path">Path to search</param>
        /// <param name="pattern">File pattern (e.g., "*.json")</param>
        /// <returns>List of file paths</returns>
        Task<List<string>> ListBatchFilesAsync(string path, string pattern = "*.json");

        /// <summary>
        /// Check if Data Lake connection is healthy
        /// </summary>
        /// <returns>True if connection is working</returns>
        Task<bool> HealthCheckAsync();
    }
}
