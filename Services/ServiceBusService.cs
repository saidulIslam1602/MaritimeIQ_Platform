using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MaritimeIQ.Platform.Services
{
    public class ServiceBusConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string VesselDataQueue { get; set; } = "vessel-data-processing";
        public string AISMessageQueue { get; set; } = "ais-message-processing";
        public string EnvironmentalDataQueue { get; set; } = "environmental-data-processing";
        public string FleetAnalyticsQueue { get; set; } = "fleet-analytics-processing";
        public string PortOperationsQueue { get; set; } = "port-operations";
        public string MaintenanceAlertsQueue { get; set; } = "maintenance-alerts";
    }

    public interface IServiceBusService
    {
        Task SendVesselDataAsync<T>(T message, string queueName);
        Task<IAsyncEnumerable<ServiceBusReceivedMessage>> ReceiveMessagesAsync(string queueName);
    }

    public class ServiceBusService : IServiceBusService
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<ServiceBusService> _logger;
        private readonly ServiceBusConfiguration _config;

        public ServiceBusService(ServiceBusClient client, ServiceBusConfiguration config, ILogger<ServiceBusService> logger)
        {
            _client = client;
            _config = config;
            _logger = logger;
        }

        public async Task SendVesselDataAsync<T>(T message, string queueName)
        {
            try
            {
                var sender = _client.CreateSender(queueName);
                var messageBody = JsonSerializer.Serialize(message);
                var busMessage = new ServiceBusMessage(messageBody)
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ContentType = "application/json",
                    TimeToLive = TimeSpan.FromMinutes(30)
                };

                await sender.SendMessageAsync(busMessage);
                _logger.LogInformation("Message sent to queue {QueueName}: {MessageId}", queueName, busMessage.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to queue {QueueName}", queueName);
                throw;
            }
        }

        public async Task<IAsyncEnumerable<ServiceBusReceivedMessage>> ReceiveMessagesAsync(string queueName)
        {
            var receiver = _client.CreateReceiver(queueName);
            await Task.CompletedTask; // Placeholder for async requirement
            return receiver.ReceiveMessagesAsync();
        }
    }

    // Background service for processing vessel data messages
    public class VesselDataProcessor : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly ILogger<VesselDataProcessor> _logger;
        private readonly IServiceScope _scope;

        public VesselDataProcessor(ServiceBusClient serviceBusClient, ServiceBusConfiguration config, 
            IServiceProvider serviceProvider, ILogger<VesselDataProcessor> logger)
        {
            _scope = serviceProvider.CreateScope();
            _logger = logger;
            
            _processor = serviceBusClient.CreateProcessor(config.VesselDataQueue, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false
            });

            _processor.ProcessMessageAsync += ProcessVesselDataMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _processor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("Vessel data processor started");
        }

        private async Task ProcessVesselDataMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var messageBody = args.Message.Body.ToString();
                _logger.LogInformation("Processing vessel data message: {MessageId}", args.Message.MessageId);

                // Process vessel data here
                // Example: Update vessel position, calculate fuel consumption, etc.
                
                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation("Vessel data message processed successfully: {MessageId}", args.Message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing vessel data message {MessageId}", args.Message.MessageId);
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing message from {QueueName}", args.EntityPath);
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _processor.StopProcessingAsync(stoppingToken);
            await _processor.DisposeAsync();
            _scope.Dispose();
            await base.StopAsync(stoppingToken);
        }
    }

    // Extension methods for dependency injection
    public static class ServiceBusExtensions
    {
        public static IServiceCollection AddMaritimeServiceBus(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("ServiceBus").Get<ServiceBusConfiguration>();
            services.AddSingleton(config!);
            
            services.AddSingleton(provider =>
            {
                return new ServiceBusClient(config!.ConnectionString);
            });

            services.AddScoped<IServiceBusService, ServiceBusService>();
            services.AddHostedService<VesselDataProcessor>();

            return services;
        }
    }
}