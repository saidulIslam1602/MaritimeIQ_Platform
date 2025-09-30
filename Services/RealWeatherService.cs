using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaritimeIQ.Platform.Services;

/// <summary>
/// Real-time weather data integration with Norwegian Meteorological Institute (MET Norway)
/// API Documentation: https://api.met.no/weatherapi/locationforecast/2.0/documentation
/// </summary>
public interface IRealWeatherService
{
    Task<RealWeatherData?> GetWeatherForLocationAsync(double latitude, double longitude);
    Task<RealWeatherData?> GetWeatherForPortAsync(string portName);
}

public class RealWeatherService : IRealWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RealWeatherService> _logger;
    private readonly Dictionary<string, (double Lat, double Lon)> _norwegianPorts;

    public RealWeatherService(HttpClient httpClient, ILogger<RealWeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configure HttpClient for MET Norway API
        _httpClient.BaseAddress = new Uri("https://api.met.no/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MaritimeIQ-Platform/2.0 github.com/maritimeiq");
        
        // Major Norwegian ports coordinates
        _norwegianPorts = new Dictionary<string, (double, double)>
        {
            { "Bergen", (60.3913, 5.3221) },
            { "Tromsø", (69.6492, 18.9553) },
            { "Kirkenes", (69.7273, 30.0450) },
            { "Ålesund", (62.4722, 6.1549) },
            { "Trondheim", (63.4305, 10.3951) },
            { "Hammerfest", (70.6634, 23.6821) },
            { "Bodø", (67.2804, 14.4049) },
            { "Stavanger", (58.9700, 5.7331) }
        };
    }

    public async Task<RealWeatherData?> GetWeatherForPortAsync(string portName)
    {
        if (!_norwegianPorts.TryGetValue(portName, out var coordinates))
        {
            _logger.LogWarning("Unknown port: {PortName}", portName);
            return null;
        }

        return await GetWeatherForLocationAsync(coordinates.Lat, coordinates.Lon);
    }

    public async Task<RealWeatherData?> GetWeatherForLocationAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"weatherapi/locationforecast/2.0/compact?lat={latitude:F4}&lon={longitude:F4}";
            
            _logger.LogInformation("Fetching weather data for lat={Lat}, lon={Lon}", latitude, longitude);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Weather API returned {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var weatherResponse = JsonSerializer.Deserialize<MetNorwayResponse>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (weatherResponse?.Properties?.Timeseries == null || !weatherResponse.Properties.Timeseries.Any())
            {
                _logger.LogWarning("No weather data in response");
                return null;
            }

            // Get current weather (first timeseries entry)
            var current = weatherResponse.Properties.Timeseries[0];
            var instant = current.Data?.Instant?.Details;

            if (instant == null)
            {
                return null;
            }

            var weatherData = new RealWeatherData
            {
                Latitude = latitude,
                Longitude = longitude,
                Temperature = instant.AirTemperature ?? 0,
                WindSpeed = instant.WindSpeed ?? 0,
                WindDirection = instant.WindFromDirection ?? 0,
                Humidity = instant.RelativeHumidity ?? 0,
                Pressure = instant.AirPressureAtSeaLevel ?? 1013.25,
                CloudCover = instant.CloudAreaFraction ?? 0,
                Visibility = 10.0, // MET API doesn't always provide this
                Timestamp = current.Time,
                Source = "Norwegian Meteorological Institute",
                DataQuality = "Real-time"
            };

            _logger.LogInformation("Successfully fetched real weather: {Temp}°C, Wind: {Wind} m/s", 
                weatherData.Temperature, weatherData.WindSpeed);

            return weatherData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data from MET Norway");
            return null;
        }
    }
}

// MET Norway API Response Models
public class MetNorwayResponse
{
    [JsonPropertyName("properties")]
    public MetProperties? Properties { get; set; }
}

public class MetProperties
{
    [JsonPropertyName("timeseries")]
    public List<MetTimeseries>? Timeseries { get; set; }
}

public class MetTimeseries
{
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }
    
    [JsonPropertyName("data")]
    public MetData? Data { get; set; }
}

public class MetData
{
    [JsonPropertyName("instant")]
    public MetInstant? Instant { get; set; }
}

public class MetInstant
{
    [JsonPropertyName("details")]
    public MetDetails? Details { get; set; }
}

public class MetDetails
{
    [JsonPropertyName("air_temperature")]
    public double? AirTemperature { get; set; }
    
    [JsonPropertyName("wind_speed")]
    public double? WindSpeed { get; set; }
    
    [JsonPropertyName("wind_from_direction")]
    public double? WindFromDirection { get; set; }
    
    [JsonPropertyName("relative_humidity")]
    public double? RelativeHumidity { get; set; }
    
    [JsonPropertyName("air_pressure_at_sea_level")]
    public double? AirPressureAtSeaLevel { get; set; }
    
    [JsonPropertyName("cloud_area_fraction")]
    public double? CloudAreaFraction { get; set; }
}

// Real Weather Data Model
public class RealWeatherData
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public double WindDirection { get; set; }
    public double Humidity { get; set; }
    public double Pressure { get; set; }
    public double CloudCover { get; set; }
    public double Visibility { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public string DataQuality { get; set; } = string.Empty;
}
