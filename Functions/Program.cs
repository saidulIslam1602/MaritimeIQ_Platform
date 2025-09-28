using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HavilaKystruten.Maritime.Functions
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    // Add logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                    });
                    
                    // Add HTTP client for external API calls
                    services.AddHttpClient();
                    
                    // Add custom services here
                    services.AddSingleton<IMaritimeDataService, MaritimeDataService>();
                    services.AddSingleton<IWeatherService, WeatherService>();
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<IAIRouteOptimizer, AIRouteOptimizer>();
                })
                .Build();

            host.Run();
        }
    }
}

// Service interfaces and implementations
public interface IMaritimeDataService
{
    Task<string> ProcessVesselDataAsync(string vesselId, double latitude, double longitude);
    Task<bool> CheckEnvironmentalComplianceAsync(string vesselId, double co2Level);
}

public interface IWeatherService
{
    Task<WeatherData> GetWeatherForecastAsync(double latitude, double longitude);
    Task<bool> CheckNorthernLightsVisibilityAsync(double latitude, double longitude);
}

public interface INotificationService
{
    Task SendBoardingNotificationAsync(string passengerId, string vesselId, DateTime boardingTime);
    Task SendNorthernLightsAlertAsync(List<string> passengerIds, string location);
}

public interface IAIRouteOptimizer
{
    Task<RouteOptimizationResult> OptimizeRouteAsync(string startPort, string endPort, WeatherData weather);
}

// Simple implementations
public class MaritimeDataService : IMaritimeDataService
{
    private readonly ILogger<MaritimeDataService> _logger;

    public MaritimeDataService(ILogger<MaritimeDataService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ProcessVesselDataAsync(string vesselId, double latitude, double longitude)
    {
        _logger.LogInformation($"Processing vessel data for {vesselId} at {latitude}, {longitude}");
        await Task.Delay(100); // Simulate processing
        return $"Processed vessel {vesselId} position";
    }

    public async Task<bool> CheckEnvironmentalComplianceAsync(string vesselId, double co2Level)
    {
        _logger.LogInformation($"Checking environmental compliance for {vesselId}, CO2: {co2Level}");
        await Task.Delay(50);
        return co2Level < 100.0; // Simple threshold check
    }
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WeatherData> GetWeatherForecastAsync(double latitude, double longitude)
    {
        _logger.LogInformation($"Getting weather forecast for {latitude}, {longitude}");
        await Task.Delay(200);
        return new WeatherData
        {
            Temperature = 5.0,
            WindSpeed = 15.0,
            Visibility = 8.0,
            SeaState = "Moderate"
        };
    }

    public async Task<bool> CheckNorthernLightsVisibilityAsync(double latitude, double longitude)
    {
        await Task.Delay(100);
        return latitude > 65.0; // Northern latitudes
    }
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendBoardingNotificationAsync(string passengerId, string vesselId, DateTime boardingTime)
    {
        _logger.LogInformation($"Sending boarding notification to {passengerId} for vessel {vesselId}");
        await Task.Delay(100);
    }

    public async Task SendNorthernLightsAlertAsync(List<string> passengerIds, string location)
    {
        _logger.LogInformation($"Sending Northern Lights alert to {passengerIds.Count} passengers at {location}");
        await Task.Delay(150);
    }
}

public class AIRouteOptimizer : IAIRouteOptimizer
{
    private readonly ILogger<AIRouteOptimizer> _logger;

    public AIRouteOptimizer(ILogger<AIRouteOptimizer> logger)
    {
        _logger = logger;
    }

    public async Task<RouteOptimizationResult> OptimizeRouteAsync(string startPort, string endPort, WeatherData weather)
    {
        _logger.LogInformation($"Optimizing route from {startPort} to {endPort}");
        await Task.Delay(300);
        
        return new RouteOptimizationResult
        {
            OptimizedRoute = $"{startPort} -> {endPort}",
            EstimatedFuelConsumption = 150.5,
            EstimatedTravelTime = TimeSpan.FromHours(8.5),
            WeatherImpact = "Favorable conditions"
        };
    }
}

// Data models
public class WeatherData
{
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public double Visibility { get; set; }
    public string SeaState { get; set; } = string.Empty;
}

public class RouteOptimizationResult
{
    public string OptimizedRoute { get; set; } = string.Empty;
    public double EstimatedFuelConsumption { get; set; }
    public TimeSpan EstimatedTravelTime { get; set; }
    public string WeatherImpact { get; set; } = string.Empty;
}