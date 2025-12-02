using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MaritimeIQ.Platform.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaritimeIQ.Platform.Tests.Services
{
    /// <summary>
    /// Unit tests for MetricsCollectorService to verify real metrics tracking
    /// </summary>
    public class MetricsCollectorServiceTests
    {
        private readonly ILogger<MetricsCollectorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TelemetryClient _telemetryClient;

        public MetricsCollectorServiceTests()
        {
            // Setup test dependencies
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<MetricsCollectorService>();

            var configValues = new Dictionary<string, string>
            {
                {"Metrics:UseRealMetrics", "true"},
                {"Metrics:SlidingWindowMinutes", "60"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues!)
                .Build();

            var telemetryConfiguration = new TelemetryConfiguration();
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [Fact]
        public void IncrementEventCounter_ShouldIncrementCorrectly()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.IncrementEventCounter("test_event", 10);
            service.IncrementEventCounter("test_event", 5);

            // Assert
            var throughput = service.GetThroughputMetrics();
            Assert.Equal(15, throughput.EventsProcessedTotal);
        }

        [Fact]
        public void GetEventsPerHour_ShouldReturnCorrectCount()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.IncrementEventCounter("ais_data", 100);
            service.IncrementEventCounter("vessel_telemetry", 50);

            // Assert
            var eventsPerHour = service.GetEventsPerHour();
            Assert.Equal(150, eventsPerHour);
        }

        [Fact]
        public void GetEventsPerSecond_ShouldCalculateCorrectly()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.IncrementEventCounter("streaming_events", 3600); // 3600 events

            // Assert
            var eventsPerSecond = service.GetEventsPerSecond();
            Assert.Equal(1.0, eventsPerSecond, precision: 2); // 3600/3600 = 1/sec
        }

        [Fact]
        public void GetFormattedThroughput_ShouldFormatCorrectly()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.IncrementEventCounter("test", 900000); // 900K events

            // Assert
            var formatted = service.GetFormattedThroughput();
            Assert.Contains("900K events/hour", formatted);
        }

        [Fact]
        public void GetUptimeMetrics_ShouldReturnValidUptime()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            System.Threading.Thread.Sleep(100); // Wait a bit
            var uptime = service.GetUptimeMetrics();

            // Assert
            Assert.True(uptime.Uptime.TotalMilliseconds > 0);
            Assert.True(uptime.UptimePercentage > 0);
            Assert.True(uptime.StartTime <= DateTime.UtcNow);
        }

        [Fact]
        public void GetCurrentUptime_ShouldIncrease()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            var uptime1 = service.GetCurrentUptime();
            System.Threading.Thread.Sleep(100);
            var uptime2 = service.GetCurrentUptime();

            // Assert
            Assert.True(uptime2 > uptime1, "Uptime should increase over time");
        }

        [Fact]
        public void RecordDataQuality_ShouldStoreCorrectly()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.RecordDataQuality("completion", 0.995);
            service.RecordDataQuality("accuracy", 0.973);

            // Assert
            Assert.Equal(0.995, service.GetDataQualityScore("completion"));
            Assert.Equal(0.973, service.GetDataQualityScore("accuracy"));
        }

        [Fact]
        public void GetDataQualityMetrics_ShouldReturnAllScores()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.RecordDataQuality("completion", 0.99);
            service.RecordDataQuality("accuracy", 0.97);
            var metrics = service.GetDataQualityMetrics();

            // Assert
            Assert.Equal(0.99, metrics.CompletionRate);
            Assert.Equal(0.97, metrics.AccuracyScore);
            Assert.True(metrics.LastUpdated <= DateTime.UtcNow);
        }

        [Fact]
        public void IncrementRequestCounter_ShouldTrackRequests()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.IncrementRequestCounter("/api/vessel-data");
            service.IncrementRequestCounter("/api/vessel-data");
            service.IncrementRequestCounter("/api/monitoring");

            // Assert
            Assert.Equal(2, service.GetRequestCount("/api/vessel-data"));
            Assert.Equal(1, service.GetRequestCount("/api/monitoring"));
            Assert.Equal(3, service.GetTotalRequests());
        }

        [Fact]
        public void GetThroughputMetrics_ShouldReturnCompleteData()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.IncrementEventCounter("test", 1000);
            var metrics = service.GetThroughputMetrics();

            // Assert
            Assert.Equal(1000, metrics.EventsProcessedTotal);
            Assert.Equal(1000, metrics.EventsLastHour);
            Assert.True(metrics.EventsPerSecond > 0);
            Assert.NotNull(metrics.FormattedThroughput);
            Assert.True(metrics.LastCalculated <= DateTime.UtcNow);
        }

        [Fact]
        public void CleanupOldMetrics_ShouldNotFailWithNoData()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act & Assert - Should not throw
            service.CleanupOldMetrics();
        }

        [Fact]
        public void MultipleEventTypes_ShouldTrackSeparately()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            service.IncrementEventCounter("ais_data", 100);
            service.IncrementEventCounter("vessel_telemetry", 200);
            service.IncrementEventCounter("streaming_events", 300);

            // Assert
            var total = service.GetThroughputMetrics().EventsProcessedTotal;
            Assert.Equal(600, total);
        }

        [Fact]
        public void GetUptimePercentage_ShouldReturnReasonableValue()
        {
            // Arrange
            var service = new MetricsCollectorService(_telemetryClient, _logger, _configuration);

            // Act
            var percentage = service.GetUptimePercentage();

            // Assert
            Assert.True(percentage >= 99.0 && percentage <= 100.0, 
                $"Uptime percentage should be between 99% and 100%, got {percentage}");
        }
    }
}

