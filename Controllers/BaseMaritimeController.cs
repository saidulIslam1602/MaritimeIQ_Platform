using Microsoft.AspNetCore.Mvc;

namespace HavilaKystruten.Maritime.Controllers
{
    /// <summary>
    /// Base controller for all Maritime API controllers
    /// Provides common functionality, error handling, and logging patterns
    /// </summary>
    [ApiController]
    public abstract class BaseMaritimeController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseMaritimeController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Standard error handling for all controllers
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        /// <param name="operation">The operation that failed</param>
        /// <returns>Standardized error response</returns>
        protected IActionResult HandleException(Exception ex, string operation)
        {
            _logger.LogError(ex, "Error in {Operation}: {Message}", operation, ex.Message);
            
            return ex switch
            {
                ArgumentException or ArgumentNullException => BadRequest(new { 
                    error = "Invalid request", 
                    message = ex.Message 
                }),
                UnauthorizedAccessException => Unauthorized(new { 
                    error = "Access denied", 
                    message = "Insufficient permissions" 
                }),
                KeyNotFoundException => NotFound(new { 
                    error = "Resource not found", 
                    message = ex.Message 
                }),
                _ => StatusCode(500, new { 
                    error = "Internal server error", 
                    message = "An unexpected error occurred" 
                })
            };
        }

        /// <summary>
        /// Standard success response with logging
        /// </summary>
        /// <param name="data">Response data</param>
        /// <param name="operation">Operation name for logging</param>
        /// <returns>Success response</returns>
        protected IActionResult HandleSuccess<T>(T data, string operation)
        {
            _logger.LogInformation("Successfully completed {Operation}", operation);
            return Ok(data);
        }

        /// <summary>
        /// Standard async operation wrapper with error handling
        /// </summary>
        /// <param name="operation">The async operation to execute</param>
        /// <param name="operationName">Name for logging purposes</param>
        /// <returns>Standardized API response</returns>
        protected async Task<IActionResult> ExecuteOperationAsync<T>(
            Func<Task<T>> operation, 
            string operationName)
        {
            try
            {
                _logger.LogInformation("Starting {Operation}", operationName);
                var result = await operation();
                return HandleSuccess(result, operationName);
            }
            catch (Exception ex)
            {
                return HandleException(ex, operationName);
            }
        }
    }
}