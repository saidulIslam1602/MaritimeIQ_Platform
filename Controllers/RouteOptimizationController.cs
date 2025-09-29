using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Services;
using MaritimeIQ.Platform.Models;
using ServiceWeatherForecast = MaritimeIQ.Platform.Services.WeatherForecast;
using ServiceAuroraForecast = MaritimeIQ.Platform.Services.AuroraForecast;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// Route Optimization Controller - Provides REST API endpoints for
    /// AI-driven route planning and fleet optimization
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Route Optimization")]
    public class RouteOptimizationController : ControllerBase
    {
        private readonly RouteOptimizationService _routeOptimizationService;
        private readonly ILogger<RouteOptimizationController> _logger;

        public RouteOptimizationController(
            RouteOptimizationService routeOptimizationService,
            ILogger<RouteOptimizationController> logger)
        {
            _routeOptimizationService = routeOptimizationService;
            _logger = logger;
        }

        /// <summary>
        /// Optimize routes for all active fleet voyages
        /// </summary>
        /// <returns>Fleet optimization results including fuel and time savings</returns>
        [HttpPost("optimize-fleet-routes")]
        [ProducesResponseType(typeof(FleetOptimizationResult), 200)]
        public async Task<ActionResult<FleetOptimizationResult>> OptimizeFleetRoutes()
        {
            try
            {
                _logger.LogInformation("Starting fleet route optimization");
                var result = await _routeOptimizationService.OptimizeFleetRoutesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing fleet routes");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get route optimization for a specific voyage
        /// </summary>
        /// <param name="voyageId">The ID of the voyage to optimize</param>
        /// <returns>Optimization suggestions for the specified voyage</returns>
        [HttpGet("optimize-voyage/{voyageId}")]
        [ProducesResponseType(typeof(VoyageOptimizationResult), 200)]
        public async Task<ActionResult<VoyageOptimizationResult>> GetRouteOptimization(string voyageId)
        {
            try
            {
                if (string.IsNullOrEmpty(voyageId))
                {
                    return BadRequest("Voyage ID is required");
                }

                _logger.LogInformation($"Getting route optimization for voyage {voyageId}");
                var result = await _routeOptimizationService.GetRouteOptimizationAsync(voyageId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting route optimization for voyage {voyageId}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get current route optimization status for the fleet
        /// </summary>
        /// <returns>Current optimization status and active routes</returns>
        [HttpGet("optimization-status")]
        [ProducesResponseType(typeof(RouteOptimizationStatus), 200)]
        public async Task<ActionResult<RouteOptimizationStatus>> GetOptimizationStatus()
        {
            try
            {
                _logger.LogInformation("Retrieving route optimization status");
                var status = await _routeOptimizationService.GetOptimizationStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimization status");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get weather impact analysis for route planning
        /// </summary>
        /// <returns>Weather conditions and their impact on routes</returns>
        [HttpGet("weather-impact")]
        [ProducesResponseType(typeof(WeatherImpactAnalysis), 200)]
        public async Task<ActionResult<WeatherImpactAnalysis>> GetWeatherImpact()
        {
            try
            {
                _logger.LogInformation("Getting weather impact analysis");
                var analysis = await _routeOptimizationService.GetWeatherImpactAsync();
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather impact analysis");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get sample voyage data for testing optimization
        /// </summary>
        /// <returns>Sample voyage information</returns>
        [HttpGet("sample-voyage-data")]
        [ProducesResponseType(typeof(List<VoyageInfo>), 200)]
        public ActionResult<List<VoyageInfo>> GetSampleVoyageData()
        {
            var sampleVoyages = new List<VoyageInfo>
            {
                new VoyageInfo
                {
                    VoyageId = "HK-2024-001",
                    VesselName = "MS Arctic Explorer",
                    CurrentPosition = new VoyagePosition { Latitude = 69.6492m, Longitude = 18.9553m, NearestPort = "Tromsø" },
                    Destination = "Kirkenes",
                    EstimatedArrival = DateTime.UtcNow.AddHours(8),
                    PassengerCount = 324
                },
                new VoyageInfo
                {
                    VoyageId = "HK-2024-002",
                    VesselName = "MS Nordic Aurora",
                    CurrentPosition = new VoyagePosition { Latitude = 70.2143m, Longitude = 19.7621m, NearestPort = "Bodø" },
                    Destination = "Bergen",
                    EstimatedArrival = DateTime.UtcNow.AddHours(12),
                    PassengerCount = 298
                },
                new VoyageInfo
                {
                    VoyageId = "HK-2024-003",
                    VesselName = "MS Coastal Voyager",
                    CurrentPosition = new VoyagePosition { Latitude = 68.7964m, Longitude = 16.0471m, NearestPort = "Narvik" },
                    Destination = "Tromsø",
                    EstimatedArrival = DateTime.UtcNow.AddHours(6),
                    PassengerCount = 412
                }
            };

            return Ok(sampleVoyages);
        }

        /// <summary>
        /// Get current weather forecast for route planning
        /// </summary>
        /// <returns>Current weather conditions affecting routes</returns>
        [HttpGet("weather-forecast")]
        [ProducesResponseType(typeof(ServiceWeatherForecast), 200)]
        public ActionResult<ServiceWeatherForecast> GetWeatherForecast()
        {
            var forecast = new ServiceWeatherForecast
            {
                WindSpeed = 15.2,
                WindDirection = 240,
                WaveHeight = 2.1,
                Visibility = 8.5,
                SeaState = 3
            };

            return Ok(forecast);
        }

        /// <summary>
        /// Get Northern Lights forecast for route planning
        /// </summary>
        /// <returns>Aurora forecast for optimal viewing opportunities</returns>
        [HttpGet("aurora-forecast")]
        [ProducesResponseType(typeof(ServiceAuroraForecast), 200)]
        public ActionResult<ServiceAuroraForecast> GetAuroraForecast()
        {
            var forecast = new ServiceAuroraForecast
            {
                Activity = "High",
                Probability = 85,
                OptimalViewingTime = DateTime.UtcNow.AddHours(4)
            };

            return Ok(forecast);
        }
    }
}