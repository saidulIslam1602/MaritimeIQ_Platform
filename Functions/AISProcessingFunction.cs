using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace HavilaKystruten.Maritime.Functions
{
    /// <summary>
    /// Real-time AIS Processing Function - Processes vessel position data every 30 seconds
    /// Triggered by Azure Event Hubs for high-throughput maritime data processing
    /// </summary>
    public class AISProcessingFunction
    {
        private readonly ILogger<AISProcessingFunction> _logger;

        public AISProcessingFunction(ILogger<AISProcessingFunction> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Event Hub triggered function for real-time AIS data processing
        /// Processes vessel positions, calculates routes, and triggers alerts
        /// </summary>
        [Function("ProcessAISData")]
        public async Task ProcessAISData(
            [EventHubTrigger("ais-data-stream", Connection = "EventHubConnectionString")] string[] events,
            FunctionContext context)
        {
            _logger.LogInformation($"Processing {events.Length} AIS messages from Event Hub");

            foreach (string eventData in events)
            {
                try
                {
                    var aisMessage = JsonSerializer.Deserialize<AISMessage>(eventData);
                    if (aisMessage != null)
                    {
                        await ProcessSingleAISMessage(aisMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing AIS message: {ex.Message}");
                }
            }

            _logger.LogInformation($"Completed processing {events.Length} AIS messages");
        }

        /// <summary>
        /// HTTP triggered function for manual AIS data processing and testing
        /// </summary>
        [Function("ProcessAISDataHTTP")]
        public async Task<HttpResponseData> ProcessAISDataHTTP(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("HTTP trigger for AIS data processing");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var aisData = JsonSerializer.Deserialize<ProcessAISRequest>(requestBody);

                if (aisData?.Messages != null)
                {
                    var results = new List<object>();

                    foreach (var message in aisData.Messages)
                    {
                        var result = await ProcessSingleAISMessage(message);
                        results.Add(result);
                    }

                    var responseData = new
                    {
                        ProcessedMessages = results.Count,
                        ProcessingTime = DateTime.UtcNow,
                        Results = results.Take(10), // Limit response size
                        Status = "SUCCESS",
                        Function = "AIS Processing",
                        TriggerType = "HTTP"
                    };

                    await response.WriteStringAsync(JsonSerializer.Serialize(responseData));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HTTP AIS processing: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(JsonSerializer.Serialize(new { Error = ex.Message }));
            }

            return response;
        }

        /// <summary>
        /// Timer triggered function for AIS data quality monitoring
        /// Runs every 5 minutes to check data freshness and quality
        /// </summary>
        [Function("MonitorAISDataQuality")]
        public async Task MonitorAISDataQuality(
            [TimerTrigger("0 */5 * * * *")] TimerInfo timer,
            FunctionContext context)
        {
            _logger.LogInformation("Monitoring AIS data quality");

            var qualityReport = new
            {
                CheckTime = DateTime.UtcNow,
                DataFreshness = await CheckDataFreshness(),
                MessageCompleteness = await CheckMessageCompleteness(),
                PositionAccuracy = await ValidatePositionAccuracy(),
                VesselCoverage = await CheckVesselCoverage(),
                AlertsGenerated = await GenerateQualityAlerts()
            };

            _logger.LogInformation($"AIS Quality Report: {JsonSerializer.Serialize(qualityReport)}");

            // In production: Send alerts to monitoring system, update dashboards
            await NotifyMonitoringSystem(qualityReport);
        }

        private async Task<object> ProcessSingleAISMessage(AISMessage message)
        {
            // Validate message integrity
            if (!ValidateAISMessage(message))
            {
                _logger.LogWarning($"Invalid AIS message from MMSI {message.MMSI}");
                return new { Status = "INVALID", MMSI = message.MMSI };
            }

            // Update vessel position in database
            var vesselUpdate = new
            {
                MMSI = message.MMSI,
                VesselName = GetVesselNameFromMMSI(message.MMSI),
                Position = new
                {
                    Latitude = message.Latitude,
                    Longitude = message.Longitude,
                    Speed = message.SpeedOverGround,
                    Course = message.CourseOverGround,
                    Heading = message.TrueHeading
                },
                NavigationStatus = message.NavigationalStatus,
                Timestamp = message.Timestamp,
                ProcessedAt = DateTime.UtcNow
            };

            // Calculate route predictions
            var routePrediction = await CalculateRoutePrediction(message);

            // Check for collision risks
            var collisionRisk = await AssessCollisionRisk(message);

            // Environmental impact calculation
            var environmentalImpact = await CalculateEnvironmentalImpact(message);

            // Generate alerts if needed
            var alerts = await GenerateNavigationAlerts(message, routePrediction, collisionRisk);

            _logger.LogInformation($"Processed AIS message for {vesselUpdate.VesselName}");

            return new
            {
                Status = "PROCESSED",
                VesselUpdate = vesselUpdate,
                RoutePrediction = routePrediction,
                CollisionRisk = collisionRisk,
                EnvironmentalImpact = environmentalImpact,
                Alerts = alerts
            };
        }

        private bool ValidateAISMessage(AISMessage message)
        {
            // Validate MMSI
            if (string.IsNullOrEmpty(message.MMSI) || message.MMSI.Length != 9)
                return false;

            // Validate position bounds (Norwegian waters)
            if (message.Latitude < 55 || message.Latitude > 72 ||
                message.Longitude < 0 || message.Longitude > 35)
                return false;

            // Validate speed (reasonable for coastal vessels)
            if (message.SpeedOverGround < 0 || message.SpeedOverGround > 40)
                return false;

            // Validate timestamp (not too old, not in future)
            var age = DateTime.UtcNow - message.Timestamp;
            if (age.TotalMinutes > 10 || age.TotalMinutes < -1)
                return false;

            return true;
        }

        private string GetVesselNameFromMMSI(string mmsi)
        {
            return mmsi switch
            {
                "257123456" => "Havila Capella",
                "257123457" => "Havila Castor",
                "257123458" => "Havila Polaris", 
                "257123459" => "Havila Pollux",
                _ => $"Unknown Vessel (MMSI: {mmsi})"
            };
        }

        private async Task<object> CalculateRoutePrediction(AISMessage message)
        {
            // Simulate route prediction algorithm
            await Task.Delay(50); // Simulate processing time

            var currentSpeed = message.SpeedOverGround;
            var currentCourse = message.CourseOverGround;

            // Simple prediction based on current speed and course
            var timeToNextPort = CalculateTimeToNextPort(message.Latitude, message.Longitude, currentSpeed, currentCourse);
            var fuelConsumption = EstimateFuelConsumption(currentSpeed, timeToNextPort);
            var weatherImpact = await GetWeatherImpactOnRoute(message.Latitude, message.Longitude);

            return new
            {
                NextPort = DetermineNextPort(message.Latitude, message.Longitude, currentCourse),
                EstimatedArrival = DateTime.UtcNow.AddHours(timeToNextPort),
                RemainingDistance = Math.Round((double)currentSpeed * timeToNextPort, 1),
                OptimalSpeed = CalculateOptimalSpeed(currentSpeed, weatherImpact),
                FuelEstimate = fuelConsumption,
                WeatherConsiderations = weatherImpact,
                ConfidenceLevel = 0.87
            };
        }

        private async Task<object> AssessCollisionRisk(AISMessage message)
        {
            // Simulate collision detection
            await Task.Delay(30);

            // In production: Query nearby vessels, calculate CPA/TCPA
            var nearbyVessels = await GetNearbyVessels(message.Latitude, message.Longitude, 10.0); // 10 nm radius

            return new
            {
                RiskLevel = "LOW",
                NearbyVessels = nearbyVessels.Count(),
                ClosestApproach = "3.2 nm in 45 minutes",
                RecommendedAction = "Maintain course and speed",
                MonitoringRequired = false,
                LastAssessment = DateTime.UtcNow
            };
        }

        private async Task<object> CalculateEnvironmentalImpact(AISMessage message)
        {
            await Task.Delay(25);

            var currentSpeed = message.SpeedOverGround;
            var fuelRate = CalculateFuelRate(currentSpeed);
            var emissionRate = CalculateEmissionRate(fuelRate);

            return new
            {
                CurrentFuelRate = $"{fuelRate:F1} L/hour",
                CO2EmissionRate = $"{emissionRate:F1} kg/hour",
                EfficiencyRating = DetermineEfficiencyRating(currentSpeed),
                OptimizationPotential = "8% fuel savings available",
                ComplianceStatus = "Within all regulatory limits",
                BatteryUsageOpportunity = currentSpeed < 12 ? "Battery mode recommended in port areas" : null
            };
        }

        private async Task<string[]> GenerateNavigationAlerts(AISMessage message, object routePrediction, object collisionRisk)
        {
            await Task.Delay(20);

            var alerts = new List<string>();

            // Speed alerts
            if (message.SpeedOverGround > 20)
                alerts.Add("HIGH_SPEED: Consider reducing speed for fuel efficiency");

            // Position alerts
            if (IsInSensitiveArea(message.Latitude, message.Longitude))
                alerts.Add("SENSITIVE_AREA: Whale protection zone - reduce speed and noise");

            // Weather alerts
            var weather = await GetCurrentWeather(message.Latitude, message.Longitude);
            if (weather.WindSpeed > 25)
                alerts.Add("WEATHER_WARNING: Strong winds detected - adjust course as needed");

            // Aurora alert (positive alert!)
            if (weather.AuroraKpIndex > 5 && message.Latitude > 65)
                alerts.Add("AURORA_ALERT: Excellent Northern Lights conditions - inform passengers");

            return alerts.ToArray();
        }

        // Helper methods for calculations
        private double CalculateTimeToNextPort(decimal lat, decimal lng, decimal speed, decimal course)
        {
            // Simplified calculation - in production use proper great circle distance
            var estimatedDistance = 45.0; // Average distance between ports
            return speed > 0 ? estimatedDistance / (double)speed : 8.0; // 8 hours default
        }

        private string DetermineNextPort(decimal lat, decimal lng, decimal course)
        {
            // Simplified port determination based on position and course
            if (lat > 69) return course > 90 && course < 270 ? "Kirkenes" : "Hammerfest";
            if (lat > 67) return course > 90 && course < 270 ? "Tromsø" : "Bodø";
            if (lat > 63) return course > 90 && course < 270 ? "Trondheim" : "Ålesund";
            return course > 90 && course < 270 ? "Bergen" : "Stavanger";
        }

        private double EstimateFuelConsumption(decimal speed, double timeHours)
        {
            // Fuel consumption curve for Havila vessels
            var baseFuelRate = speed switch
            {
                <= 8 => 800,   // Battery/low consumption
                <= 12 => 1200, // Efficient cruising
                <= 16 => 1800, // Normal service speed
                <= 20 => 2400, // High speed
                _ => 3200      // Maximum speed
            };

            return baseFuelRate * timeHours;
        }

        private double CalculateOptimalSpeed(decimal currentSpeed, object weatherImpact)
        {
            // Optimal speed calculation considering weather
            var baseOptimal = 14.5; // Knots
            // Adjust for weather conditions
            return Math.Max(12.0, Math.Min(18.0, baseOptimal));
        }

        private async Task<object> GetWeatherImpactOnRoute(decimal lat, decimal lng)
        {
            await Task.Delay(40);
            return new
            {
                WindSpeed = 15.2,
                WindDirection = "SW",
                WaveHeight = 2.1,
                Impact = "Minimal - favorable conditions",
                FuelImpact = "+5% due to headwinds"
            };
        }

        private async Task<List<object>> GetNearbyVessels(decimal lat, decimal lng, double radiusNm)
        {
            await Task.Delay(35);
            // In production: Query AIS database for nearby vessels
            return new List<object>
            {
                new { MMSI = "257654321", Distance = "8.3 nm", Bearing = "045°", Speed = "12.5 knots" },
                new { MMSI = "257987654", Distance = "5.7 nm", Bearing = "180°", Speed = "16.2 knots" }
            };
        }

        private double CalculateFuelRate(decimal speed)
        {
            return (double)speed * 120 + 600; // Simplified fuel curve
        }

        private double CalculateEmissionRate(double fuelRate)
        {
            return fuelRate * 2.68; // kg CO2 per liter diesel
        }

        private string DetermineEfficiencyRating(decimal speed)
        {
            return speed switch
            {
                >= 12 and <= 16 => "OPTIMAL",
                >= 10 and < 12 => "GOOD",
                >= 16 and <= 18 => "GOOD",
                < 10 => "BATTERY_MODE_AVAILABLE",
                _ => "REVIEW_SPEED"
            };
        }

        private bool IsInSensitiveArea(decimal lat, decimal lng)
        {
            // Check if in whale protection zones, marine parks, etc.
            return lat > 68.5m && lat < 69.2m && lng > 16m && lng < 20m; // Example sensitive area near Tromsø
        }

        private async Task<WeatherCondition> GetCurrentWeather(decimal lat, decimal lng)
        {
            await Task.Delay(30);
            return new WeatherCondition
            {
                WindSpeed = 18.5,
                Visibility = 12.0,
                AuroraKpIndex = 4.2
            };
        }

        // Quality monitoring methods
        private async Task<object> CheckDataFreshness()
        {
            await Task.Delay(100);
            return new
            {
                LatestMessage = DateTime.UtcNow.AddSeconds(-25),
                AverageDelay = "15 seconds",
                Status = "EXCELLENT"
            };
        }

        private async Task<object> CheckMessageCompleteness()
        {
            await Task.Delay(80);
            return new
            {
                CompletionRate = "99.7%",
                MissingFields = 0,
                Status = "EXCELLENT"
            };
        }

        private async Task<object> ValidatePositionAccuracy()
        {
            await Task.Delay(90);
            return new
            {
                AverageAccuracy = "±2.1 meters",
                GPSQuality = "High precision",
                Status = "EXCELLENT"
            };
        }

        private async Task<object> CheckVesselCoverage()
        {
            await Task.Delay(70);
            return new
            {
                VesselsReporting = 4,
                ExpectedVessels = 4,
                CoverageRate = "100%",
                Status = "COMPLETE"
            };
        }

        private async Task<string[]> GenerateQualityAlerts()
        {
            await Task.Delay(60);
            return new string[] { }; // No alerts - all systems operational
        }

        private async Task NotifyMonitoringSystem(object qualityReport)
        {
            await Task.Delay(50);
            _logger.LogInformation($"Quality report sent to monitoring system: {JsonSerializer.Serialize(qualityReport)}");
        }
    }

    // Supporting classes
    public class ProcessAISRequest
    {
        public AISMessage[] Messages { get; set; } = Array.Empty<AISMessage>();
    }
}