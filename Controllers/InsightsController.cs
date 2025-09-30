using MaritimeIQ.Platform.Models;
using MaritimeIQ.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaritimeIQ.Platform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InsightsController : ControllerBase
{
    private readonly IMaritimeDataService _maritimeData;
    private readonly IRealWeatherService _weatherService;
    private readonly IAuroraForecastService _auroraService;
    private readonly ILogger<InsightsController> _logger;

    public InsightsController(
        IMaritimeDataService maritimeData, 
        IRealWeatherService weatherService,
        IAuroraForecastService auroraService,
        ILogger<InsightsController> logger)
    {
        _maritimeData = maritimeData;
        _weatherService = weatherService;
        _auroraService = auroraService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<MaritimeInsightsOverview> GetInsights()
    {
        var insights = _maritimeData.GetPlatformInsights();
        _logger.LogInformation("Generated maritime insights overview at {Timestamp}", insights.GeneratedAt);
        return Ok(insights);
    }

    [HttpGet("fleet-status")]
    public ActionResult<FleetStatusSummary> GetFleetStatus()
    {
        var status = _maritimeData.GetFleetStatus();
        return Ok(status);
    }

    [HttpGet("environmental")]
    public ActionResult<EnvironmentalSummary> GetEnvironmentalSummary()
    {
        var summary = _maritimeData.GetEnvironmentalSummary();
        return Ok(summary);
    }

    [HttpGet("voyages")]
    public ActionResult<VoyageScheduleSnapshot> GetVoyageSchedule()
    {
        var schedule = _maritimeData.GetVoyageSchedule();
        return Ok(schedule);
    }

    /// <summary>
    /// Get REAL weather data for major Norwegian ports
    /// Integrates with Norwegian Meteorological Institute API
    /// </summary>
    [HttpGet("weather/ports")]
    public async Task<ActionResult> GetPortsWeather()
    {
        var ports = new[] { "Bergen", "Tromsø", "Kirkenes", "Ålesund", "Trondheim" };
        var weatherData = new List<object>();

        foreach (var port in ports)
        {
            var weather = await _weatherService.GetWeatherForPortAsync(port);
            if (weather != null)
            {
                weatherData.Add(new
                {
                    Port = port,
                    Temperature = Math.Round(weather.Temperature, 1),
                    WindSpeed = Math.Round(weather.WindSpeed, 1),
                    WindDirection = Math.Round(weather.WindDirection, 0),
                    Humidity = Math.Round(weather.Humidity, 0),
                    Pressure = Math.Round(weather.Pressure, 1),
                    CloudCover = Math.Round(weather.CloudCover, 0),
                    Conditions = DetermineWeatherConditions(weather.CloudCover),
                    Timestamp = weather.Timestamp,
                    DataSource = "✅ REAL DATA - Norwegian Meteorological Institute",
                    DataQuality = weather.DataQuality
                });
            }
        }

        _logger.LogInformation("✅ Retrieved REAL weather data for {Count} ports", weatherData.Count);

        return Ok(new
        {
            DataType = "REAL-TIME WEATHER DATA",
            Source = "Norwegian Meteorological Institute (api.met.no)",
            PortsCount = weatherData.Count,
            Timestamp = DateTime.UtcNow,
            Data = weatherData
        });
    }

    /// <summary>
    /// Get REAL Northern Lights (Aurora Borealis) forecast
    /// Integrates with NOAA Space Weather Prediction Center
    /// </summary>
    [HttpGet("aurora")]
    public async Task<ActionResult> GetAuroraForecast()
    {
        var forecast = await _auroraService.GetAuroraForecastAsync();
        
        if (forecast == null)
        {
            return Ok(new
            {
                Message = "Aurora forecast temporarily unavailable",
                DataSource = "NOAA Space Weather Prediction Center"
            });
        }

        _logger.LogInformation("✅ REAL Aurora forecast: Kp={Kp}, Activity={Activity}", 
            forecast.KpIndex, forecast.ActivityLevel);

        return Ok(new
        {
            DataType = "REAL-TIME NORTHERN LIGHTS FORECAST",
            DataSource = forecast.DataSource,
            DataQuality = forecast.DataQuality,
            KpIndex = forecast.KpIndex,
            ActivityLevel = forecast.ActivityLevel,
            VisibilityLevel = forecast.VisibilityLevel,
            ViewingQuality = forecast.ViewingQuality,
            BestLocations = forecast.RecommendedLocations,
            PassengerAlert = forecast.PassengerAlertLevel,
            ThreeDayOutlook = forecast.ThreeDayOutlook,
            ForecastTime = forecast.ForecastTime,
            Explanation = new
            {
                KpIndexMeaning = "Kp Index measures geomagnetic activity (0-9). Higher = better aurora viewing.",
                CurrentLevel = $"Kp {forecast.KpIndex:F1} indicates {forecast.ActivityLevel} geomagnetic activity",
                ViewingAdvice = forecast.KpIndex >= 5 
                    ? "🌟 Excellent aurora viewing conditions! Alert passengers on deck!" 
                    : forecast.KpIndex >= 3 
                        ? "✨ Good aurora viewing possible in Northern Norway" 
                        : "Limited aurora activity. Best viewing in Arctic regions only."
            }
        });
    }

    /// <summary>
    /// Check aurora visibility for specific vessel location
    /// </summary>
    [HttpGet("aurora/vessel/{vesselId}")]
    public async Task<ActionResult> GetVesselAuroraVisibility(int vesselId)
    {
        // Get vessel position (using sample positions for demo)
        var position = vesselId switch
        {
            1 => (Lat: 62.4722, Lon: 6.1492),    // Ålesund
            2 => (Lat: 69.6492, Lon: 18.9553),   // Tromsø - excellent for aurora!
            3 => (Lat: 63.4305, Lon: 10.3951),   // Trondheim
            4 => (Lat: 69.7273, Lon: 30.0450),   // Kirkenes - best location!
            _ => (Lat: 65.0, Lon: 15.0)          // Mid-Norway
        };

        var visibility = await _auroraService.CheckAuroraVisibilityAsync(position.Lat, position.Lon);

        return Ok(new
        {
            VesselId = vesselId,
            Location = new { Latitude = position.Lat, Longitude = position.Lon },
            AuroraStatus = visibility.IsVisible ? "🌌 VISIBLE" : "Not Currently Visible",
            Probability = $"{visibility.Probability:F0}%",
            KpIndex = visibility.KpIndex,
            ActivityLevel = visibility.ActivityLevel,
            BestViewingTime = visibility.BestViewingTime,
            PassengerMessage = visibility.Message,
            ForecastTime = visibility.ForecastTime,
            DataSource = "✅ REAL DATA - NOAA Space Weather"
        });
    }

    private static string DetermineWeatherConditions(double cloudCover)
    {
        return cloudCover switch
        {
            < 20 => "Clear sky ☀️",
            < 40 => "Mostly clear 🌤️",
            < 60 => "Partly cloudy ⛅",
            < 80 => "Mostly cloudy ☁️",
            _ => "Overcast 🌥️"
        };
    }
}
