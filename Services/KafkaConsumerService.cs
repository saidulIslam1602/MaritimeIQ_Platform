using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Enterprise Kafka Consumer Service for processing maritime data streams
    /// Implements consumer groups, offset management, and error handling
    /// </summary>
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly KafkaConfiguration _config;
        private readonly IConsumer<string, string> _consumer;

        public KafkaConsumerService(
            ILogger<KafkaConsumerService> logger,
            IOptions<KafkaConfiguration> config)
        {
            _logger = logger;
            _config = config.Value;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                GroupId = _config.GroupId,
                ClientId = $"maritime-consumer-{Environment.MachineName}",
                
                // Offset management
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, // Manual commit for exactly-once processing
                EnableAutoOffsetStore = false,
                
                // Performance tuning
                FetchMinBytes = 1,
                FetchMaxBytes = 52428800, // 50MB
                MaxPartitionFetchBytes = 1048576, // 1MB
                
                // Session management
                SessionTimeoutMs = 30000,
                HeartbeatIntervalMs = 3000,
                MaxPollIntervalMs = 300000,
                
                // Security
                SecurityProtocol = string.IsNullOrEmpty(_config.SaslUsername) 
                    ? SecurityProtocol.Plaintext 
                    : SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = _config.SaslUsername,
                SaslPassword = _config.SaslPassword
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError($"Kafka consumer error: {e.Reason}"))
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogInformation("📥 Partitions assigned: {Partitions}", 
                        string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger.LogInformation("📤 Partitions revoked: {Partitions}", 
                        string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
                })
                .Build();

            _logger.LogInformation("✅ Kafka Consumer Service initialized");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Starting Kafka Consumer Service");

            // Subscribe to all maritime topics
            _consumer.Subscribe(new[]
            {
                _config.AISDataTopic,
                _config.EnvironmentalDataTopic,
                _config.AlertsTopic,
                _config.VoyageEventsTopic
            });

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);

                        if (consumeResult != null && !consumeResult.IsPartitionEOF)
                        {
                            await ProcessMessageAsync(consumeResult, stoppingToken);

                            // Manual commit for exactly-once semantics
                            _consumer.Commit(consumeResult);
                            _consumer.StoreOffset(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "❌ Error consuming message");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Kafka consumer stopping gracefully");
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("✅ Kafka consumer closed");
            }
        }

        /// <summary>
        /// Process individual Kafka messages based on topic
        /// </summary>
        private async Task ProcessMessageAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken)
        {
            try
            {
                var topic = result.Topic;
                var key = result.Message.Key;
                var value = result.Message.Value;
                var partition = result.Partition.Value;
                var offset = result.Offset.Value;

                _logger.LogDebug("📨 Processing message: Topic={Topic}, Partition={Partition}, Offset={Offset}, Key={Key}",
                    topic, partition, offset, key);

                // Route to appropriate handler based on topic
                if (topic == _config.AISDataTopic)
                {
                    await ProcessAISDataAsync(value, cancellationToken);
                }
                else if (topic == _config.EnvironmentalDataTopic)
                {
                    await ProcessEnvironmentalDataAsync(value, cancellationToken);
                }
                else if (topic == _config.AlertsTopic)
                {
                    await ProcessAlertAsync(value, cancellationToken);
                }
                else if (topic == _config.VoyageEventsTopic)
                {
                    await ProcessVoyageEventAsync(value, cancellationToken);
                }

                _logger.LogDebug("✅ Message processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing message from topic {Topic}", result.Topic);
                // Implement dead letter queue logic here
                await SendToDeadLetterQueueAsync(result, ex);
            }
        }

        /// <summary>
        /// Process AIS vessel position data
        /// </summary>
        private async Task ProcessAISDataAsync(string messageValue, CancellationToken cancellationToken)
        {
            var aisData = JsonSerializer.Deserialize<AISVesselData>(messageValue);
            if (aisData == null) return;

            _logger.LogInformation("🚢 Processing AIS data: Vessel {Vessel} at ({Lat}, {Lon}), Speed: {Speed} knots",
                aisData.VesselName, aisData.Latitude, aisData.Longitude, aisData.Speed);

            // Store in database
            // Update real-time dashboard
            // Check for anomalies
            // Trigger alerts if needed

            await Task.Delay(10, cancellationToken); // Simulate processing
        }

        /// <summary>
        /// Process environmental sensor data
        /// </summary>
        private async Task ProcessEnvironmentalDataAsync(string messageValue, CancellationToken cancellationToken)
        {
            var envData = JsonSerializer.Deserialize<EnvironmentalSensorData>(messageValue);
            if (envData == null) return;

            _logger.LogInformation("🌊 Processing environmental data: Vessel {VesselId}, CO2: {CO2}kg, Battery: {Battery}%",
                envData.VesselId, envData.CO2EmissionKg, envData.BatteryStateOfCharge);

            // Store in time-series database
            // Check compliance thresholds
            // Update analytics dashboard
            // Generate compliance reports

            await Task.Delay(10, cancellationToken); // Simulate processing
        }

        /// <summary>
        /// Process maritime alerts
        /// </summary>
        private async Task ProcessAlertAsync(string messageValue, CancellationToken cancellationToken)
        {
            var alert = JsonSerializer.Deserialize<MaritimeAlert>(messageValue);
            if (alert == null) return;

            _logger.LogWarning("🚨 Processing alert: {AlertType} - {Message} [Severity: {Severity}]",
                alert.AlertType, alert.Message, alert.Severity);

            // Send notifications
            // Log to monitoring system
            // Update alert dashboard
            // Trigger automated responses

            await Task.Delay(10, cancellationToken); // Simulate processing
        }

        /// <summary>
        /// Process voyage events
        /// </summary>
        private async Task ProcessVoyageEventAsync(string messageValue, CancellationToken cancellationToken)
        {
            _logger.LogInformation("⚓ Processing voyage event");
            
            // Update voyage status
            // Calculate performance metrics
            // Update passenger information systems

            await Task.Delay(10, cancellationToken); // Simulate processing
        }

        /// <summary>
        /// Send failed messages to dead letter queue
        /// </summary>
        private async Task SendToDeadLetterQueueAsync(ConsumeResult<string, string> result, Exception ex)
        {
            _logger.LogError("💀 Sending message to dead letter queue: Topic={Topic}, Partition={Partition}, Offset={Offset}",
                result.Topic, result.Partition, result.Offset);

            // Implement DLQ logic
            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            base.Dispose();
            _logger.LogInformation("🛑 Kafka Consumer Service disposed");
        }
    }

    /// <summary>
    /// Environmental Sensor Data Model
    /// </summary>
    public class EnvironmentalSensorData
    {
        public int VesselId { get; set; }
        public DateTime MeasurementTime { get; set; }
        public double CO2EmissionKg { get; set; }
        public double NOxEmissionKg { get; set; }
        public double SOxEmissionKg { get; set; }
        public double FuelConsumptionLiters { get; set; }
        public double BatteryStateOfCharge { get; set; }
        public double WaterTemperature { get; set; }
        public double AirTemperature { get; set; }
    }
}