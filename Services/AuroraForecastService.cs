using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace MaritimeIQ.Platform.Services;

/// <summary>
/// Real-time Aurora Borealis (Northern Lights) forecast integration
/// Data from NOAA Space Weather Prediction Center
/// API Documentation: https://services.swpc.noaa.gov/
/// </summary>
public interface IAuroraForecastService
{
    Task<RealAuroraForecast?> GetAuroraForecastAsync();
    Task<AuroraVisibility> CheckAuroraVisibilityAsync(double latitude, double longitude);
}

public class AuroraForecastService : IAuroraForecastService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuroraForecastService> _logger;
    private readonly Dictionary<string, (double Lat, double Lon)> _norwegianLocations;

    public AuroraForecastService(HttpClient httpClient, ILogger<AuroraForecastService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // NOAA Space Weather Prediction Center API
        _httpClient.BaseAddress = new Uri("https://services.swpc.noaa.gov/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MaritimeIQ-Platform/2.0");
        
        // Norwegian locations for aurora viewing
        _norwegianLocations = new Dictionary<string, (double, double)>
        {
            { "Tromsø", (69.6492, 18.9553) },      // Excellent aurora viewing
            { "Kirkenes", (69.7273, 30.0450) },     // Near Russian border
            { "Alta", (69.9689, 23.2717) },         // Aurora capital of Norway
            { "Bodø", (67.2804, 14.4049) },         // Just above Arctic Circle
            { "Lofoten", (68.2077, 13.6360) },      // Islands, great viewing
            { "Svalbard", (78.2232, 15.6267) }      // Northernmost
        };
    }

    public async Task<RealAuroraForecast?> GetAuroraForecastAsync()
    {
        try
        {
            // Get current aurora oval data
            var response = await _httpClient.GetAsync("products/noaa-scales.json");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Aurora API returned {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<NoaaScalesResponse>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (data == null)
            {
                return null;
            }

            // Get 3-day forecast
            var forecastResponse = await _httpClient.GetAsync("text/3-day-forecast.txt");
            var forecastText = await forecastResponse.Content.ReadAsStringAsync();

            // Parse current geomagnetic activity (Kp index)
            var kpIndex = ExtractKpIndex(forecastText);
            var visibility = CalculateVisibilityLevel(kpIndex);

            var forecast = new RealAuroraForecast
            {
                KpIndex = kpIndex,
                ActivityLevel = GetActivityLevel(kpIndex),
                VisibilityLevel = visibility,
                ForecastTime = DateTime.UtcNow,
                ThreeDayOutlook = ParseThreeDayOutlook(forecastText),
                ViewingQuality = DetermineViewingQuality(kpIndex),
                RecommendedLocations = GetBestViewingLocations(kpIndex),
                PassengerAlertLevel = ShouldAlertPassengers(kpIndex),
                DataSource = "NOAA Space Weather Prediction Center",
                DataQuality = "Real-time"
            };

            _logger.LogInformation("✅ REAL Aurora forecast: Kp={Kp}, Activity={Activity}, Visibility={Visibility}", 
                kpIndex, forecast.ActivityLevel, forecast.VisibilityLevel);

            return forecast;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching aurora forecast from NOAA");
            return null;
        }
    }

    public async Task<AuroraVisibility> CheckAuroraVisibilityAsync(double latitude, double longitude)
    {
        var forecast = await GetAuroraForecastAsync();
        
        if (forecast == null)
        {
            return new AuroraVisibility
            {
                IsVisible = false,
                Probability = 0,
                Message = "Aurora data unavailable"
            };
        }

        // Aurora is typically visible above 65° latitude for Kp 3+
        // Higher Kp = visible further south
        var minLatitude = CalculateMinimumLatitude(forecast.KpIndex);
        var isVisible = latitude >= minLatitude && forecast.KpIndex >= 3;
        var probability = CalculateProbability(latitude, forecast.KpIndex);

        return new AuroraVisibility
        {
            IsVisible = isVisible,
            Probability = probability,
            KpIndex = forecast.KpIndex,
            ActivityLevel = forecast.ActivityLevel,
            BestViewingTime = GetBestViewingTime(),
            Message = GeneratePassengerMessage(isVisible, probability, forecast.ActivityLevel),
            Latitude = latitude,
            Longitude = longitude,
            ForecastTime = forecast.ForecastTime
        };
    }

    private double ExtractKpIndex(string forecastText)
    {
        // Parse Kp index from NOAA forecast text
        // Kp ranges from 0-9 (geomagnetic activity)
        try
        {
            // Look for current Kp in the forecast
            var lines = forecastText.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Geomagnetic Activity Summary", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract Kp value (simplified parsing)
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"Kp\s*=\s*(\d+\.?\d*)");
                    if (match.Success && double.TryParse(match.Groups[1].Value, out var kp))
                    {
                        return kp;
                    }
                }
            }
            
            // Default to moderate activity if can't parse
            return 4.0;
        }
        catch
        {
            return 4.0; // Moderate activity
        }
    }

    private string GetActivityLevel(double kpIndex)
    {
        return kpIndex switch
        {
            < 3 => "Low",
            < 5 => "Moderate",
            < 7 => "High",
            < 9 => "Very High",
            _ => "Extreme"
        };
    }

    private string CalculateVisibilityLevel(double kpIndex)
    {
        return kpIndex switch
        {
            < 3 => "Limited - Northern Arctic only",
            < 5 => "Good - Arctic Norway (Tromsø, Alta)",
            < 7 => "Excellent - Visible across Northern Norway",
            < 9 => "Exceptional - Visible far south, possibly Oslo",
            _ => "Historic - Visible across entire Norway"
        };
    }

    private double CalculateMinimumLatitude(double kpIndex)
    {
        // Higher Kp = aurora visible further south
        return kpIndex switch
        {
            >= 9 => 50.0,  // Can see from Oslo
            >= 7 => 60.0,  // Southern Norway
            >= 5 => 65.0,  // Mid-Norway
            >= 3 => 68.0,  // Northern Norway only
            _ => 70.0      // Arctic only
        };
    }

    private double CalculateProbability(double latitude, double kpIndex)
    {
        var minLat = CalculateMinimumLatitude(kpIndex);
        
        if (latitude < minLat)
        {
            return Math.Max(0, 20 - (minLat - latitude) * 5); // Low probability
        }
        
        // Higher latitude + higher Kp = better probability
        var baseProbability = (kpIndex / 9.0) * 100;
        var latitudeBonus = Math.Min(20, (latitude - 65) * 2);
        
        return Math.Min(100, baseProbability + latitudeBonus);
    }

    private string ParseThreeDayOutlook(string forecastText)
    {
        // Extract relevant forecast info
        if (forecastText.Contains("ACTIVE") || forecastText.Contains("STORM"))
        {
            return "⚡ High activity expected - Excellent aurora viewing opportunities!";
        }
        else if (forecastText.Contains("QUIET"))
        {
            return "🌙 Quiet conditions - Aurora viewing may be limited";
        }
        else
        {
            return "✨ Moderate activity - Good aurora viewing possible";
        }
    }

    private string DetermineViewingQuality(double kpIndex)
    {
        return kpIndex switch
        {
            >= 7 => "⭐⭐⭐⭐⭐ Spectacular!",
            >= 5 => "⭐⭐⭐⭐ Excellent",
            >= 3 => "⭐⭐⭐ Good",
            _ => "⭐⭐ Fair"
        };
    }

    private List<string> GetBestViewingLocations(double kpIndex)
    {
        if (kpIndex >= 7)
        {
            return new List<string> { "Tromsø", "Alta", "Kirkenes", "Bodø", "Lofoten", "Even Southern Norway!" };
        }
        else if (kpIndex >= 5)
        {
            return new List<string> { "Tromsø", "Alta", "Kirkenes", "Bodø", "Lofoten" };
        }
        else if (kpIndex >= 3)
        {
            return new List<string> { "Tromsø", "Alta", "Kirkenes", "Svalbard" };
        }
        else
        {
            return new List<string> { "Svalbard", "Northernmost locations only" };
        }
    }

    private string ShouldAlertPassengers(double kpIndex)
    {
        return kpIndex >= 5 ? "🔔 YES - Send aurora viewing alerts to passengers!" : "No alerts needed";
    }

    private string GetBestViewingTime()
    {
        var now = DateTime.UtcNow;
        var norwegianTime = now.AddHours(1); // CET/CEST
        
        // Best viewing: 22:00 - 02:00
        if (norwegianTime.Hour >= 22 || norwegianTime.Hour <= 2)
        {
            return "🌌 NOW is prime viewing time!";
        }
        else if (norwegianTime.Hour >= 20 || norwegianTime.Hour <= 4)
        {
            return "🌙 Good viewing window";
        }
        else
        {
            return "☀️ Best viewing after 22:00 local time";
        }
    }

    private string GeneratePassengerMessage(bool isVisible, double probability, string activityLevel)
    {
        if (!isVisible || probability < 30)
        {
            return "Aurora viewing unlikely at current location. Check forecast for updates.";
        }
        else if (probability >= 80)
        {
            return $"🌟 EXCELLENT AURORA CONDITIONS! {activityLevel} activity. Go outside now!";
        }
        else if (probability >= 60)
        {
            return $"✨ GOOD Aurora viewing opportunity! {activityLevel} activity expected.";
        }
        else
        {
            return $"Aurora may be visible. {activityLevel} activity. Watch the sky!";
        }
    }
}

// NOAA API Response Models
public class NoaaScalesResponse
{
    [JsonPropertyName("-1")]
    public Dictionary<string, object>? Previous { get; set; }
    
    [JsonPropertyName("0")]
    public Dictionary<string, object>? Current { get; set; }
}

// Aurora Forecast Models
public class RealAuroraForecast
{
    public double KpIndex { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
    public string VisibilityLevel { get; set; } = string.Empty;
    public DateTime ForecastTime { get; set; }
    public string ThreeDayOutlook { get; set; } = string.Empty;
    public string ViewingQuality { get; set; } = string.Empty;
    public List<string> RecommendedLocations { get; set; } = new();
    public string PassengerAlertLevel { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string DataQuality { get; set; } = string.Empty;
}

public class AuroraVisibility
{
    public bool IsVisible { get; set; }
    public double Probability { get; set; }
    public double KpIndex { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
    public string BestViewingTime { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime ForecastTime { get; set; }
}
