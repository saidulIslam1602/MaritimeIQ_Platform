using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace MaritimeIQ.Platform.Services
{
    public class IoTHubConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DeviceConnectionString { get; set; } = string.Empty;
        public string HubName { get; set; } = "havila-maritime-iothub";
        public int TelemetryIntervalSeconds { get; set; } = 30;
    }

    public interface IIoTHubService
    {
        Task SendVesselTelemetryAsync(VesselTelemetry telemetry);
        Task<CloudToDeviceMethodResult> InvokeDeviceMethodAsync(string deviceId, string methodName, object payload);
        Task UpdateDeviceTwinAsync(string deviceId, TwinCollection desiredProperties);
        Task<Twin> GetDeviceTwinAsync(string deviceId);
        Task RegisterVesselDeviceAsync(string vesselId, string deviceId);
    }

    public class IoTHubService : IIoTHubService
    {
        private readonly ServiceClient _serviceClient;
        private readonly RegistryManager _registryManager;
        private readonly DeviceClient? _deviceClient;
        private readonly ILogger<IoTHubService> _logger;
        private readonly IoTHubConfiguration _config;

        public IoTHubService(IoTHubConfiguration config, ILogger<IoTHubService> logger)
        {
            _config = config;
            _logger = logger;
            
            _serviceClient = ServiceClient.CreateFromConnectionString(config.ConnectionString);
            _registryManager = RegistryManager.CreateFromConnectionString(config.ConnectionString);
            
            if (!string.IsNullOrEmpty(config.DeviceConnectionString))
            {
                _deviceClient = DeviceClient.CreateFromConnectionString(config.DeviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
            }
        }

        public async Task SendVesselTelemetryAsync(VesselTelemetry telemetry)
        {
            try
            {
                var telemetryJson = JsonSerializer.Serialize(telemetry);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(telemetryJson));
                
                // Add message properties for routing
                message.Properties.Add("vesselId", telemetry.VesselId);
                message.Properties.Add("messageType", "telemetry");
                message.Properties.Add("urgency", telemetry.IsEmergency ? "high" : "normal");
                
                if (_deviceClient != null)
                {
                    await _deviceClient.SendEventAsync(message);
                }
                _logger.LogInformation("Telemetry sent for vessel {VesselId}", telemetry.VesselId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending telemetry for vessel {VesselId}", telemetry.VesselId);
                throw;
            }
        }

        public async Task<CloudToDeviceMethodResult> InvokeDeviceMethodAsync(string deviceId, string methodName, object payload)
        {
            try
            {
                var methodRequest = new CloudToDeviceMethod(methodName);
                methodRequest.SetPayloadJson(JsonSerializer.Serialize(payload));
                methodRequest.ResponseTimeout = TimeSpan.FromSeconds(30);
                methodRequest.ConnectionTimeout = TimeSpan.FromSeconds(10);

                var result = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodRequest);
                _logger.LogInformation("Device method {MethodName} invoked on device {DeviceId}", methodName, deviceId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking device method {MethodName} on device {DeviceId}", methodName, deviceId);
                throw;
            }
        }

        public async Task UpdateDeviceTwinAsync(string deviceId, TwinCollection desiredProperties)
        {
            try
            {
                var twin = await _registryManager.GetTwinAsync(deviceId);
                var patch = new Twin();
                patch.Properties.Desired = desiredProperties;
                await _registryManager.UpdateTwinAsync(deviceId, patch, twin.ETag);
                
                _logger.LogInformation("Device twin updated for device {DeviceId}", deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device twin for device {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<Twin> GetDeviceTwinAsync(string deviceId)
        {
            try
            {
                var twin = await _registryManager.GetTwinAsync(deviceId);
                _logger.LogInformation("Retrieved device twin for device {DeviceId}", deviceId);
                return twin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving device twin for device {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task RegisterVesselDeviceAsync(string vesselId, string deviceId)
        {
            try
            {
                var device = new Device(deviceId);
                device.Status = DeviceStatus.Enabled;
                
                // Add vessel-specific tags
                var twin = new Twin(deviceId);
                twin.Tags["vesselId"] = vesselId;
                twin.Tags["deviceType"] = "vessel";
                twin.Tags["location"] = "norwegian-coast";
                twin.Tags["operator"] = "havila-kystruten";

                await _registryManager.AddDeviceAsync(device);
                await _registryManager.UpdateTwinAsync(deviceId, twin, "*");
                
                _logger.LogInformation("Registered vessel device {DeviceId} for vessel {VesselId}", deviceId, vesselId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering vessel device {DeviceId} for vessel {VesselId}", deviceId, vesselId);
                throw;
            }
        }
    }

    // Background service for processing telemetry from vessels
    public class VesselTelemetryProcessor : BackgroundService
    {
        private readonly IIoTHubService _iotHubService;
        private readonly ILogger<VesselTelemetryProcessor> _logger;
        private readonly IoTHubConfiguration _config;

        public VesselTelemetryProcessor(IIoTHubService iotHubService, IoTHubConfiguration config, ILogger<VesselTelemetryProcessor> logger)
        {
            _iotHubService = iotHubService;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Vessel telemetry processor started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Simulate telemetry from Havila Kystruten vessels
                    await ProcessHavilaFleetTelemetry();
                    await Task.Delay(TimeSpan.FromSeconds(_config.TelemetryIntervalSeconds), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in vessel telemetry processing");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Wait before retry
                }
            }
        }

        private async Task ProcessHavilaFleetTelemetry()
        {
            // Simulate telemetry from Havila's 4 vessels
            var vessels = new[]
            {
                new { Id = "HAVILA_CASTOR", Name = "MS Arctic Explorer" },
                new { Id = "HAVILA_POLLUX", Name = "MS Nordic Spirit" },
                new { Id = "HAVILA_CAPELLA", Name = "MS Nordic Aurora" },
                new { Id = "HAVILA_POLARIS", Name = "MS Coastal Voyager" }
            };

            foreach (var vessel in vessels)
            {
                var telemetry = GenerateMockTelemetry(vessel.Id, vessel.Name);
                await _iotHubService.SendVesselTelemetryAsync(telemetry);
            }
        }

        private VesselTelemetry GenerateMockTelemetry(string vesselId, string vesselName)
        {
            var random = new Random();
            
            return new VesselTelemetry
            {
                VesselId = vesselId,
                VesselName = vesselName,
                Timestamp = DateTime.UtcNow,
                Position = new VesselTelemetryPosition
                {
                    Latitude = 60.0 + random.NextDouble() * 10.0, // Norwegian coast range
                    Longitude = 5.0 + random.NextDouble() * 25.0,
                    Speed = 12.0 + random.NextDouble() * 8.0, // 12-20 knots typical
                    Heading = random.NextDouble() * 360.0
                },
                EngineData = new EngineTelemetryData
                {
                    FuelConsumptionLitersPerHour = 800 + random.NextDouble() * 400,
                    EngineRPM = 1200 + random.NextDouble() * 300,
                    EngineTemperature = 75 + random.NextDouble() * 15,
                    BatteryLevel = 85 + random.NextDouble() * 15 // Hybrid vessels
                },
                EnvironmentalData = new EnvironmentalTelemetryData
                {
                    CO2EmissionKg = 40 + random.NextDouble() * 20,
                    WaterTemperature = 8 + random.NextDouble() * 7,
                    AirTemperature = 5 + random.NextDouble() * 15,
                    WindSpeed = random.NextDouble() * 25,
                    WaveHeight = random.NextDouble() * 3
                },
                PassengerData = new PassengerTelemetryData
                {
                    PassengerCount = random.Next(200, 600),
                    CrewCount = random.Next(30, 50),
                    CabinOccupancy = 0.7 + random.NextDouble() * 0.3
                },
                IsEmergency = random.NextDouble() < 0.01 // 1% chance of emergency
            };
        }
    }

    // Extension methods for dependency injection
    public static class IoTHubExtensions
    {
        public static IServiceCollection AddHavilaIoTHub(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("IoTHub").Get<IoTHubConfiguration>();
            services.AddSingleton(config!);
            
            services.AddScoped<IIoTHubService, IoTHubService>();
            services.AddHostedService<VesselTelemetryProcessor>();

            return services;
        }
    }

    // Telemetry models
    public class VesselTelemetry
    {
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public VesselTelemetryPosition Position { get; set; } = new();
        public EngineTelemetryData EngineData { get; set; } = new();
        public EnvironmentalTelemetryData EnvironmentalData { get; set; } = new();
        public PassengerTelemetryData PassengerData { get; set; } = new();
        public bool IsEmergency { get; set; }
    }

    public class VesselTelemetryPosition
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
    }

    public class EngineTelemetryData
    {
        public double FuelConsumptionLitersPerHour { get; set; }
        public double EngineRPM { get; set; }
        public double EngineTemperature { get; set; }
        public double BatteryLevel { get; set; } // For hybrid vessels
    }

    public class EnvironmentalTelemetryData
    {
        public double CO2EmissionKg { get; set; }
        public double WaterTemperature { get; set; }
        public double AirTemperature { get; set; }
        public double WindSpeed { get; set; }
        public double WaveHeight { get; set; }
    }

    public class PassengerTelemetryData
    {
        public int PassengerCount { get; set; }
        public int CrewCount { get; set; }
        public double CabinOccupancy { get; set; }
    }
}