using Microsoft.ApplicationInsights;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Background service that periodically updates metrics and pushes to Application Insights
    /// </summary>
    public class MetricsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MetricsBackgroundService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(60);

        public MetricsBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<MetricsBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MetricsBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateMetricsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating metrics in background service");
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }

            _logger.LogInformation("MetricsBackgroundService stopped");
        }

        private async Task UpdateMetricsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            
            var metricsCollector = scope.ServiceProvider.GetRequiredService<IMetricsCollectorService>();
            var telemetryClient = scope.ServiceProvider.GetRequiredService<TelemetryClient>();

            try
            {
                // Clean up old metrics data
                metricsCollector.CleanupOldMetrics();

                // Get current metrics
                var throughput = metricsCollector.GetThroughputMetrics();
                var uptime = metricsCollector.GetUptimeMetrics();
                var dataQuality = metricsCollector.GetDataQualityMetrics();

                // Push to Application Insights
                telemetryClient.TrackMetric("maritime.events.per.hour", throughput.EventsLastHour);
                telemetryClient.TrackMetric("maritime.events.per.second", throughput.EventsPerSecond);
                telemetryClient.TrackMetric("maritime.uptime.percentage", uptime.UptimePercentage);
                telemetryClient.TrackMetric("maritime.uptime.hours", uptime.Uptime.TotalHours);
                telemetryClient.TrackMetric("maritime.data.quality.overall", 
                    (dataQuality.CompletionRate + dataQuality.AccuracyScore + 
                     dataQuality.TimelinessScore + dataQuality.ConsistencyScore) / 4.0);

                _logger.LogDebug(
                    "Metrics updated: {EventsPerHour} events/hour, {Uptime} uptime, {Requests} total requests",
                    throughput.EventsLastHour,
                    uptime.Uptime.ToString(@"dd\.hh\:mm\:ss"),
                    metricsCollector.GetTotalRequests());

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateMetricsAsync");
            }
        }
    }
}

