using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Enterprise Kafka Producer Service for real-time maritime data streaming
    /// Handles high-throughput AIS data, environmental sensors, and event notifications
    /// </summary>
    public class KafkaProducerService : IDisposable
    {
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly KafkaConfiguration _config;
        private readonly IProducer<string, string> _producer;
        private bool _disposed = false;

        public KafkaProducerService(
            ILogger<KafkaProducerService> logger,
            IOptions<KafkaConfiguration> config)
        {
            _logger = logger;
            _config = config.Value;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                ClientId = $"maritime-producer-{Environment.MachineName}",
                Acks = Acks.All, // Wait for all in-sync replicas
                EnableIdempotence = true, // Exactly-once semantics
                MaxInFlight = 5,
                CompressionType = CompressionType.Snappy,
                LingerMs = 10, // Batch messages for efficiency
                BatchSize = 32768, // 32KB batches
                MessageTimeoutMs = 30000,
                
                // Security configuration
                SecurityProtocol = string.IsNullOrEmpty(_config.SaslUsername) 
                    ? SecurityProtocol.Plaintext 
                    : SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = _config.SaslUsername,
                SaslPassword = _config.SaslPassword
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) => _logger.LogError($"Kafka error: {e.Reason}"))
                .SetStatisticsHandler((_, json) => _logger.LogDebug($"Kafka stats: {json}"))
                .Build();

            _logger.LogInformation("✅ Kafka Producer Service initialized - Bootstrap: {Servers}", 
                _config.BootstrapServers);
        }

        /// <summary>
        /// Publish AIS vessel position data to Kafka topic
        /// High-throughput real-time streaming with partitioning by vessel ID
        /// </summary>
        public async Task PublishAISDataAsync(AISVesselData aisData)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = aisData.MMSI, // Partition by vessel MMSI for ordering
                    Value = JsonSerializer.Serialize(aisData),
                    Timestamp = new Timestamp(aisData.Timestamp),
                    Headers = new Headers
                    {
                        { "source", System.Text.Encoding.UTF8.GetBytes("maritime-platform") },
                        { "event-type", System.Text.Encoding.UTF8.GetBytes("ais-position") },
                        { "vessel-name", System.Text.Encoding.UTF8.GetBytes(aisData.VesselName) }
                    }
                };

                var result = await _producer.ProduceAsync(_config.AISDataTopic, message);
                
                _logger.LogDebug("📡 AIS data published: Vessel {Vessel} to partition {Partition} at offset {Offset}",
                    aisData.VesselName, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "❌ Failed to publish AIS data for vessel {Vessel}", aisData.VesselName);
                throw;
            }
        }

        /// <summary>
        /// Publish environmental sensor data to Kafka
        /// Streams CO2, NOx, SOx emissions and battery metrics
        /// </summary>
        public async Task PublishEnvironmentalDataAsync(EnvironmentalSensorData sensorData)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = sensorData.VesselId.ToString(),
                    Value = JsonSerializer.Serialize(sensorData),
                    Timestamp = new Timestamp(sensorData.MeasurementTime),
                    Headers = new Headers
                    {
                        { "source", System.Text.Encoding.UTF8.GetBytes("environmental-sensors") },
                        { "event-type", System.Text.Encoding.UTF8.GetBytes("sensor-reading") },
                        { "vessel-id", System.Text.Encoding.UTF8.GetBytes(sensorData.VesselId.ToString()) }
                    }
                };

                var result = await _producer.ProduceAsync(_config.EnvironmentalDataTopic, message);
                
                _logger.LogDebug("🌊 Environmental data published: Vessel {VesselId} - CO2: {CO2}kg, Partition: {Partition}",
                    sensorData.VesselId, sensorData.CO2EmissionKg, result.Partition.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "❌ Failed to publish environmental data for vessel {VesselId}", sensorData.VesselId);
                throw;
            }
        }

        /// <summary>
        /// Publish maritime alerts and notifications
        /// Event-driven alerting for safety, compliance, and operational events
        /// </summary>
        public async Task PublishAlertAsync(MaritimeAlert alert)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = alert.AlertId,
                    Value = JsonSerializer.Serialize(alert),
                    Timestamp = new Timestamp(alert.Timestamp),
                    Headers = new Headers
                    {
                        { "source", System.Text.Encoding.UTF8.GetBytes("maritime-platform") },
                        { "event-type", System.Text.Encoding.UTF8.GetBytes("alert") },
                        { "severity", System.Text.Encoding.UTF8.GetBytes(alert.Severity) },
                        { "alert-type", System.Text.Encoding.UTF8.GetBytes(alert.AlertType) }
                    }
                };

                var result = await _producer.ProduceAsync(_config.AlertsTopic, message);
                
                _logger.LogInformation("🚨 Alert published: {AlertType} - {Message} to partition {Partition}",
                    alert.AlertType, alert.Message, result.Partition.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "❌ Failed to publish alert: {AlertId}", alert.AlertId);
                throw;
            }
        }

        /// <summary>
        /// Publish batch of messages for high-throughput scenarios
        /// </summary>
        public async Task PublishBatchAsync<T>(string topic, IEnumerable<T> items, Func<T, string> keySelector)
        {
            var tasks = new List<Task<DeliveryResult<string, string>>>();

            foreach (var item in items)
            {
                var message = new Message<string, string>
                {
                    Key = keySelector(item),
                    Value = JsonSerializer.Serialize(item),
                    Timestamp = Timestamp.Default
                };

                tasks.Add(_producer.ProduceAsync(topic, message));
            }

            try
            {
                await Task.WhenAll(tasks);
                _logger.LogInformation("✅ Published batch of {Count} messages to topic {Topic}", tasks.Count, topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to publish batch to topic {Topic}", topic);
                throw;
            }
        }

        /// <summary>
        /// Flush pending messages (call before disposal)
        /// </summary>
        public void Flush(TimeSpan timeout)
        {
            _producer.Flush(timeout);
            _logger.LogInformation("🔄 Kafka producer flushed");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
                _disposed = true;
                _logger.LogInformation("🛑 Kafka Producer Service disposed");
            }
        }
    }

    /// <summary>
    /// Kafka Configuration Model
    /// </summary>
    public class KafkaConfiguration
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string AISDataTopic { get; set; } = "maritime.ais.data";
        public string EnvironmentalDataTopic { get; set; } = "maritime.environmental.sensors";
        public string AlertsTopic { get; set; } = "maritime.alerts";
        public string VoyageEventsTopic { get; set; } = "maritime.voyage.events";
        public string SaslUsername { get; set; } = "";
        public string SaslPassword { get; set; } = "";
        public string GroupId { get; set; } = "maritime-platform-consumers";
    }

    /// <summary>
    /// Maritime Alert Model for Kafka
    /// </summary>
    public class MaritimeAlert
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public string AlertType { get; set; } = "";
        public string Severity { get; set; } = ""; // Critical, High, Medium, Low
        public string Message { get; set; } = "";
        public string VesselId { get; set; } = "";
        public string VesselName { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}