using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HavilaKystruten.Maritime.Services
{
    public class EventHubConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string AISDataStreamHub { get; set; } = "ais-data-stream";
        public string VesselTrackingHub { get; set; } = "vessel-telemetry";
        public string EnvironmentalSensorsHub { get; set; } = "environmental-sensors";
        public string ConsumerGroup { get; set; } = "$Default";
    }

    public interface IEventHubService
    {
        Task SendAISDataAsync(object data);
        Task SendVesselTrackingDataAsync(object data);
        Task SendEnvironmentalDataAsync(object data);
    }

    public class EventHubService : IEventHubService
    {
        private readonly EventHubConfiguration _config;
        private readonly EventHubProducerClient _aisProducer;
        private readonly EventHubProducerClient _vesselProducer;
        private readonly EventHubProducerClient _environmentalProducer;
        private readonly ILogger<EventHubService> _logger;

        public EventHubService(EventHubConfiguration config, ILogger<EventHubService> logger)
        {
            _config = config;
            _logger = logger;
            _aisProducer = new EventHubProducerClient(config.ConnectionString, config.AISDataStreamHub);
            _vesselProducer = new EventHubProducerClient(config.ConnectionString, config.VesselTrackingHub);
            _environmentalProducer = new EventHubProducerClient(config.ConnectionString, config.EnvironmentalSensorsHub);
        }

        public async Task SendAISDataAsync(object data)
        {
            try
            {
                _logger.LogInformation("Sending AIS data to Event Hub");
                var eventData = new EventData(JsonSerializer.SerializeToUtf8Bytes(data));
                eventData.Properties.Add("MessageType", "AIS");
                eventData.Properties.Add("Timestamp", DateTimeOffset.UtcNow);

                await _aisProducer.SendAsync(new[] { eventData });
                _logger.LogInformation("AIS data sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending AIS data to Event Hub");
                throw;
            }
        }

        public async Task SendVesselTrackingDataAsync(object data)
        {
            try
            {
                _logger.LogInformation("Sending vessel tracking data to Event Hub");
                var eventData = new EventData(JsonSerializer.SerializeToUtf8Bytes(data));
                eventData.Properties.Add("MessageType", "VesselTracking");
                eventData.Properties.Add("Timestamp", DateTimeOffset.UtcNow);

                await _vesselProducer.SendAsync(new[] { eventData });
                _logger.LogInformation("Vessel tracking data sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending vessel tracking data to Event Hub");
                throw;
            }
        }

        public async Task SendEnvironmentalDataAsync(object data)
        {
            try
            {
                _logger.LogInformation("Sending environmental data to Event Hub");
                var eventData = new EventData(JsonSerializer.SerializeToUtf8Bytes(data));
                eventData.Properties.Add("MessageType", "Environmental");
                eventData.Properties.Add("Timestamp", DateTimeOffset.UtcNow);

                await _environmentalProducer.SendAsync(new[] { eventData });
                _logger.LogInformation("Environmental data sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending environmental data to Event Hub");
                throw;
            }
        }

        public void Dispose()
        {
            _aisProducer?.DisposeAsync().GetAwaiter().GetResult();
            _vesselProducer?.DisposeAsync().GetAwaiter().GetResult();
            _environmentalProducer?.DisposeAsync().GetAwaiter().GetResult();
        }
    }

    // Background service for processing AIS data from Event Hub
    public class AISDataConsumerService : BackgroundService
    {
        private readonly EventHubConfiguration _config;
        private readonly ILogger<AISDataConsumerService> _logger;
        private readonly EventHubConsumerClient _consumer;

        public AISDataConsumerService(EventHubConfiguration config, ILogger<AISDataConsumerService> logger)
        {
            _config = config;
            _logger = logger;
            _consumer = new EventHubConsumerClient(config.ConsumerGroup, config.ConnectionString, config.AISDataStreamHub);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting AIS data consumer service");

            try
            {
                await foreach (PartitionEvent partitionEvent in _consumer.ReadEventsAsync(stoppingToken))
                {
                    if (partitionEvent.Data != null)
                    {
                        var eventBody = partitionEvent.Data.EventBody.ToString();
                        _logger.LogInformation("Received AIS data: {EventBody}", eventBody);

                        // Process the AIS data here
                        await ProcessAISData(eventBody);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("AIS data consumer service stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AIS data consumer service");
            }
        }

        private async Task ProcessAISData(string aisData)
        {
            try
            {
                // Parse and process AIS data
                _logger.LogInformation("Processing AIS data: {AISData}", aisData);
                
                // Add your AIS data processing logic here
                await Task.Delay(100); // Simulate processing time
                
                _logger.LogInformation("AIS data processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AIS data");
            }
        }

        public override void Dispose()
        {
            _consumer?.DisposeAsync().GetAwaiter().GetResult();
            base.Dispose();
        }
    }

    // Extension methods for dependency injection
    public static class EventHubExtensions
    {
        public static IServiceCollection AddHavilaEventHubs(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("EventHub").Get<EventHubConfiguration>() ?? new EventHubConfiguration();
            services.AddSingleton(config);
            
            services.AddScoped<IEventHubService, EventHubService>();
            services.AddHostedService<AISDataConsumerService>();

            return services;
        }
    }
}