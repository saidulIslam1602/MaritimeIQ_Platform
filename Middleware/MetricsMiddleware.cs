using System.Diagnostics;
using MaritimeIQ.Platform.Services;

namespace MaritimeIQ.Platform.Middleware
{
    /// <summary>
    /// Middleware to automatically track all HTTP requests and measure response times
    /// </summary>
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MetricsMiddleware> _logger;

        public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IMetricsCollectorService metricsCollector)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = context.Request.Path.ToString();

            try
            {
                // Increment request counter
                metricsCollector.IncrementRequestCounter(endpoint);

                // Continue with the request pipeline
                await _next(context);

                stopwatch.Stop();

                // Track successful request
                if (context.Response.StatusCode < 400)
                {
                    metricsCollector.IncrementEventCounter("api_request_success");
                }
                else
                {
                    metricsCollector.IncrementEventCounter("api_request_error");
                }

                _logger.LogDebug(
                    "Request {Method} {Path} completed in {Duration}ms with status {StatusCode}",
                    context.Request.Method,
                    endpoint,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                metricsCollector.IncrementEventCounter("api_request_exception");
                
                _logger.LogError(ex, 
                    "Request {Method} {Path} failed after {Duration}ms",
                    context.Request.Method,
                    endpoint,
                    stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}

