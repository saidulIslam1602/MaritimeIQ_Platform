namespace HavilaKystruten.Maritime.Services
{
    /// <summary>
    /// Base service class for all Maritime services
    /// Provides common functionality, logging patterns, and health monitoring
    /// </summary>
    public abstract class BaseMaritimeService : IBaseMaritimeService
    {
        protected readonly ILogger _logger;
        protected readonly IConfiguration? _configuration;

        public abstract string ServiceName { get; }

        protected BaseMaritimeService(ILogger logger, IConfiguration? configuration = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;
        }

        /// <summary>
        /// Virtual health check method that can be overridden by derived services
        /// </summary>
        public virtual async Task<bool> HealthCheckAsync()
        {
            try
            {
                _logger.LogInformation("Performing health check for {ServiceName}", ServiceName);
                
                // Default health check - can be overridden by derived classes
                await Task.Delay(10); // Simulate async operation
                
                _logger.LogInformation("Health check successful for {ServiceName}", ServiceName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for {ServiceName}: {Message}", ServiceName, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Standard error handling for all services
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        /// <param name="operation">The operation that failed</param>
        protected void LogError(Exception ex, string operation)
        {
            _logger.LogError(ex, "Error in {ServiceName}.{Operation}: {Message}", 
                ServiceName, operation, ex.Message);
        }

        /// <summary>
        /// Standard information logging
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="args">Optional arguments for the log message</param>
        protected void LogInformation(string message, string? operation = null, params object[] args)
        {
            var logMessage = operation != null 
                ? $"[{ServiceName}.{operation}] {message}"
                : $"[{ServiceName}] {message}";
            
            _logger.LogInformation(logMessage, args);
        }

        /// <summary>
        /// Standard warning logging
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="args">Optional arguments for the log message</param>
        protected void LogWarning(string message, string? operation = null, params object[] args)
        {
            var logMessage = operation != null 
                ? $"[{ServiceName}.{operation}] {message}"
                : $"[{ServiceName}] {message}";
            
            _logger.LogWarning(logMessage, args);
        }

        /// <summary>
        /// Standard async operation wrapper with error handling
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">The async operation to execute</param>
        /// <param name="operationName">Name for logging purposes</param>
        /// <returns>Result of the operation</returns>
        protected async Task<T> ExecuteOperationAsync<T>(
            Func<Task<T>> operation, 
            string operationName)
        {
            try
            {
                LogInformation("Starting operation", operationName);
                var result = await operation();
                LogInformation("Operation completed successfully", operationName);
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
                throw; // Re-throw to allow caller to handle
            }
        }
    }
}