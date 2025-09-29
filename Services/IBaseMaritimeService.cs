namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Base interface for all Maritime services
    /// Defines common service capabilities and health monitoring
    /// </summary>
    public interface IBaseMaritimeService
    {
        /// <summary>
        /// Performs a health check for the service
        /// </summary>
        /// <returns>True if service is healthy, false otherwise</returns>
        Task<bool> HealthCheckAsync();

        /// <summary>
        /// Gets the service name for logging and monitoring
        /// </summary>
        string ServiceName { get; }
    }
}