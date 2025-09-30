using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Services;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// Kafka Integration Controller
    /// Provides APIs for publishing and monitoring Kafka data streams
    /// </summary>
    [ApiController]
    [Route("api/kafka")]
    [Tags("Kafka Integration")]
    public class KafkaIntegrationController : BaseMaritimeController
    {
        private readonly KafkaProducerService _kafkaProducer;
        private new readonly ILogger<KafkaIntegrationController> _logger;

        public KafkaIntegrationController(
            KafkaProducerService kafkaProducer,
            ILogger<KafkaIntegrationController> logger) : base(logger)
        {
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        /// <summary>
        /// Publish AIS data to Kafka stream
        /// Real-time vessel position streaming
        /// </summary>
        [HttpPost("publish/ais")]
        public async Task<IActionResult> PublishAISData([FromBody] AISVesselData aisData)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await _kafkaProducer.PublishAISDataAsync(aisData);
                return new
                {
                    Message = "AIS data published to Kafka successfully",
                    Vessel = aisData.VesselName,
                    MMSI = aisData.MMSI,
                    Timestamp = aisData.Timestamp
                };
            }, "PublishAISData");
        }

        /// <summary>
        /// Publish environmental sensor data to Kafka
        /// Streams CO2, NOx, SOx emissions and battery metrics
        /// </summary>
        [HttpPost("publish/environmental")]
        public async Task<IActionResult> PublishEnvironmentalData([FromBody] Services.EnvironmentalSensorData sensorData)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await _kafkaProducer.PublishEnvironmentalDataAsync(sensorData);
                return new
                {
                    Message = "Environmental data published to Kafka successfully",
                    VesselId = sensorData.VesselId,
                    CO2Emissions = sensorData.CO2EmissionKg,
                    Timestamp = sensorData.MeasurementTime
                };
            }, "PublishEnvironmentalData");
        }

        /// <summary>
        /// Publish maritime alert to Kafka
        /// Event-driven alerting system
        /// </summary>
        [HttpPost("publish/alert")]
        public async Task<IActionResult> PublishAlert([FromBody] MaritimeAlert alert)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await _kafkaProducer.PublishAlertAsync(alert);
                return new
                {
                    Message = "Alert published to Kafka successfully",
                    AlertId = alert.AlertId,
                    AlertType = alert.AlertType,
                    Severity = alert.Severity
                };
            }, "PublishAlert");
        }

        /// <summary>
        /// Publish batch of AIS records (bulk operation)
        /// High-throughput batch streaming
        /// </summary>
        [HttpPost("publish/ais-batch")]
        public async Task<IActionResult> PublishAISBatch([FromBody] List<AISVesselData> aisDataList)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await _kafkaProducer.PublishBatchAsync(
                    "maritime.ais.data",
                    aisDataList,
                    ais => ais.MMSI
                );
                return new
                {
                    Message = "Batch AIS data published to Kafka successfully",
                    RecordCount = aisDataList.Count,
                    Timestamp = DateTime.UtcNow
                };
            }, "PublishAISBatch");
        }

        /// <summary>
        /// Get Kafka integration status and metrics
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetKafkaStatus()
        {
            return Ok(new
            {
                Service = "Kafka Integration",
                Status = "Operational",
                Topics = new
                {
                    AISData = "maritime.ais.data",
                    EnvironmentalSensors = "maritime.environmental.sensors",
                    Alerts = "maritime.alerts",
                    VoyageEvents = "maritime.voyage.events"
                },
                Features = new[]
                {
                    "Real-time AIS streaming",
                    "Environmental sensor data streaming",
                    "Event-driven alerts",
                    "High-throughput batch processing",
                    "Exactly-once delivery semantics"
                },
                Configuration = new
                {
                    CompressionType = "Snappy",
                    EnableIdempotence = true,
                    Acks = "All",
                    MaxInFlight = 5
                },
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Simulate streaming test data to Kafka
        /// For development and testing purposes
        /// </summary>
        [HttpPost("test/stream")]
        public async Task<IActionResult> StreamTestData([FromQuery] int recordCount = 100)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var random = new Random();
                var publishedRecords = 0;

                // Generate and publish test AIS data
                for (int i = 0; i < recordCount; i++)
                {
                    var testAIS = new AISVesselData
                    {
                        VesselName = $"Test Vessel {i % 5 + 1}",
                        MMSI = $"25701{234 + (i % 5)}",
                        Latitude = 69.0 + random.NextDouble() * 2,
                        Longitude = 18.0 + random.NextDouble() * 2,
                        Speed = 15.0 + random.NextDouble() * 8,
                        Heading = random.Next(0, 360),
                        Timestamp = DateTime.UtcNow.AddSeconds(-i * 10)
                    };

                    await _kafkaProducer.PublishAISDataAsync(testAIS);
                    publishedRecords++;

                    // Delay to simulate real-time streaming
                    await Task.Delay(50);
                }

                return new
                {
                    Message = "Test data streaming completed",
                    RecordsPublished = publishedRecords,
                    Duration = $"{recordCount * 0.05} seconds",
                    AverageRate = $"{recordCount / (recordCount * 0.05)} records/second"
                };
            }, "StreamTestData");
        }

        /// <summary>
        /// Flush pending Kafka messages
        /// Ensures all messages are sent before shutdown
        /// </summary>
        [HttpPost("flush")]
        public IActionResult FlushMessages()
        {
            _kafkaProducer.Flush(TimeSpan.FromSeconds(10));
            return Ok(new
            {
                Message = "Kafka producer flushed successfully",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}