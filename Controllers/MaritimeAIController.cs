using Microsoft.AspNetCore.Mvc;
using HavilaKystruten.Maritime.Models;
using HavilaKystruten.Maritime.Data;

namespace HavilaKystruten.Maritime.Controllers
{
    /// <summary>
    /// Maritime AI Processing Service - Handles intelligent route optimization, 
    /// weather prediction, and fuel efficiency using Azure AI services
    /// </summary>
    [ApiController]
    [Route("api/maritime-ai")]
    [Tags("Maritime AI Processing")]
    public class MaritimeAIController : ControllerBase
    {
        private readonly ILogger<MaritimeAIController> _logger;

        public MaritimeAIController(ILogger<MaritimeAIController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get route optimization recommendations using AI models
        /// </summary>
        [HttpPost("route-optimization")]
        public ActionResult<object> OptimizeRoute([FromBody] RouteOptimizationRequest request)
        {
            _logger.LogInformation($"Processing route optimization for vessel {request.VesselId}");

            // Simulate AI model processing
            var model = HavilaFleetData.GetMaritimeAIModels().First(m => m.ModelType == "RouteOptimization");
            
            var optimization = new
            {
                RequestId = Guid.NewGuid(),
                VesselId = request.VesselId,
                RecommendedRoute = new
                {
                    RouteId = "OPTIMIZED_001",
                    Waypoints = new[]
                    {
                        new { Name = "Bergen", Lat = 60.3913, Lng = 5.3221, EstimatedArrival = DateTime.UtcNow.AddHours(2) },
                        new { Name = "Ålesund", Lat = 62.4722, Lng = 6.1492, EstimatedArrival = DateTime.UtcNow.AddHours(8) },
                        new { Name = "Trondheim", Lat = 63.4305, Lng = 10.3951, EstimatedArrival = DateTime.UtcNow.AddHours(16) },
                        new { Name = "Bodø", Lat = 67.2804, Lng = 14.4040, EstimatedArrival = DateTime.UtcNow.AddHours(28) },
                        new { Name = "Tromsø", Lat = 69.6496, Lng = 18.9560, EstimatedArrival = DateTime.UtcNow.AddHours(36) },
                        new { Name = "Kirkenes", Lat = 69.7258, Lng = 30.0426, EstimatedArrival = DateTime.UtcNow.AddHours(48) }
                    },
                    OptimizationBenefits = new
                    {
                        FuelSavings = "12% reduction",
                        CO2Reduction = "8.5 tons less emissions",
                        TimeOptimization = "2.5 hours faster",
                        WeatherAvoidance = "Avoiding storm system near Lofoten Islands"
                    }
                },
                ModelUsed = new
                {
                    Name = model.ModelName,
                    Version = model.Version,
                    Accuracy = model.Accuracy,
                    ProcessingTime = $"{model.ProcessingTimeMs}ms"
                },
                WeatherConsiderations = new
                {
                    CurrentConditions = "Favorable sailing conditions",
                    Forecast48h = "Moderate winds, good visibility",
                    AuroraActivity = "High KP index 6.2 - excellent Northern Lights viewing potential",
                    SeaState = "Calm to moderate (1-3m waves)"
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(optimization);
        }

        /// <summary>
        /// Real-time weather prediction for Norwegian coastal waters
        /// </summary>
        [HttpGet("weather-prediction/{location}")]
        public ActionResult<object> GetWeatherPrediction(string location)
        {
            _logger.LogInformation($"Generating weather prediction for {location}");

            var model = HavilaFleetData.GetMaritimeAIModels().First(m => m.ModelType == "WeatherPrediction");
            
            var prediction = new
            {
                Location = location,
                ModelInfo = new
                {
                    Name = model.ModelName,
                    Specialized = "Arctic and Norwegian coastal waters",
                    Accuracy = model.Accuracy,
                    LastTraining = model.TrainingDate
                },
                Forecast = new
                {
                    Current = new
                    {
                        Temperature = "8°C",
                        WindSpeed = "15 knots",
                        WindDirection = "SW",
                        Visibility = "12 km",
                        WaveHeight = "2.1m",
                        Conditions = "Partly cloudy with good visibility"
                    },
                    Next24Hours = new[]
                    {
                        new { Time = "06:00", Temp = "6°C", Wind = "12 knots NW", Waves = "1.8m", Visibility = "15 km", Conditions = "Clear" },
                        new { Time = "12:00", Temp = "10°C", Wind = "18 knots W", Waves = "2.5m", Visibility = "10 km", Conditions = "Partly cloudy" },
                        new { Time = "18:00", Temp = "7°C", Wind = "22 knots SW", Waves = "3.2m", Visibility = "8 km", Conditions = "Overcast" },
                        new { Time = "00:00", Temp = "5°C", Wind = "16 knots W", Waves = "2.8m", Visibility = "12 km", Conditions = "Clear, aurora possible" }
                    },
                    ExtendedForecast = new
                    {
                        Day2 = "Moderate winds, good sailing conditions",
                        Day3 = "Storm system approaching from west - adjust route",
                        Day4 = "Clearing conditions, excellent visibility",
                        Day5 = "Calm seas, perfect for Northern Lights viewing"
                    }
                },
                MaritimeAlerts = new[]
                {
                    "Strong aurora activity expected tonight (KP index 6.8)",
                    "Minor storm system approaching western coast (Day 3)",
                    "Excellent visibility conditions for next 48 hours"
                },
                OptimalAuroraViewing = new
                {
                    Tonight = "Excellent (KP 6.8)",
                    BestPorts = new[] { "Tromsø", "Alta", "Hammerfest", "Kirkenes" },
                    ViewingTime = "22:00 - 02:00 local time",
                    MoonPhase = "New moon - perfect darkness"
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(prediction);
        }

        /// <summary>
        /// Fuel and battery optimization for hybrid vessels
        /// </summary>
        [HttpPost("fuel-optimization")]
        public ActionResult<object> OptimizeFuelConsumption([FromBody] FuelOptimizationRequest request)
        {
            _logger.LogInformation($"Processing fuel optimization for vessel {request.VesselId}");

            var model = HavilaFleetData.GetMaritimeAIModels().First(m => m.ModelType == "FuelOptimization");
            var environmentalData = HavilaFleetData.GetSampleEnvironmentalData()
                                                  .Where(e => e.VesselId == request.VesselId)
                                                  .OrderByDescending(e => e.MeasurementTime)
                                                  .Take(24);

            var optimization = new
            {
                VesselId = request.VesselId,
                ModelUsed = new
                {
                    Name = model.ModelName,
                    Specialized = "Hybrid vessel fuel and battery optimization",
                    Version = model.Version,
                    Accuracy = model.Accuracy
                },
                CurrentPerformance = new
                {
                    FuelConsumption = $"{environmentalData.Average(e => e.FuelConsumptionLiters):F1} L/hour",
                    BatterySOC = $"{environmentalData.Average(e => e.BatteryStateOfCharge):F1}%",
                    CO2Emissions = $"{environmentalData.Average(e => e.CO2EmissionKg):F1} kg/hour",
                    EfficiencyRating = "Good (8.2/10)"
                },
                Recommendations = new
                {
                    BatteryUsage = new
                    {
                        Action = "Increase battery usage in port areas",
                        Benefit = "15% reduction in emissions during port operations",
                        Implementation = "Use battery power for hotel loads and maneuvering"
                    },
                    RouteOptimization = new
                    {
                        Action = "Adjust speed profile based on sea conditions",
                        Benefit = "8% fuel savings on current route",
                        Implementation = "Reduce speed in rough seas, increase in calm conditions"
                    },
                    HybridOperation = new
                    {
                        Action = "Optimize diesel-electric power split",
                        Benefit = "12% improvement in overall efficiency",
                        Implementation = "Use battery power for peak loads, diesel for baseline"
                    }
                },
                PredictedSavings = new
                {
                    Daily = new { Fuel = "450 liters", CO2 = "1.2 tons", Cost = "€420" },
                    Monthly = new { Fuel = "13,500 liters", CO2 = "36 tons", Cost = "€12,600" },
                    Annual = new { Fuel = "162,000 liters", CO2 = "432 tons", Cost = "€151,200" }
                },
                EnvironmentalImpact = new
                {
                    CO2Reduction = "15% below industry average",
                    NOxReduction = "45% lower emissions",
                    ComplianceStatus = "Exceeds IMO 2030 targets",
                    SustainabilityRating = "A+ for Norwegian coastal operations"
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(optimization);
        }

        /// <summary>
        /// Document processing using Azure Form Recognizer for maritime documents
        /// </summary>
        [HttpPost("document-processing")]
        public ActionResult<object> ProcessMaritimeDocument([FromBody] DocumentProcessingRequest request)
        {
            _logger.LogInformation($"Processing maritime document: {request.DocumentType}");

            var processing = new
            {
                DocumentId = Guid.NewGuid(),
                DocumentType = request.DocumentType,
                ProcessingStatus = "Completed",
                AzureServices = new
                {
                    FormRecognizer = "Extracted structured data",
                    TextAnalytics = "Analyzed content sentiment and key phrases",
                    Translation = "Multi-language support (Norwegian, English, German)"
                },
                ExtractedData = new
                {
                    VesselName = "Havila Capella",
                    PortOfCall = "Tromsø",
                    PassengerCount = 487,
                    CargoManifest = new[] { "Passenger vehicles: 85", "Commercial cargo: 12 tons", "Mail and packages: 150 items" },
                    ComplianceCheck = "All requirements met",
                    DigitalSignature = "Verified - Captain's electronic signature valid"
                },
                AIInsights = new
                {
                    RiskAssessment = "Low risk - standard passenger operations",
                    WeatherImpact = "Favorable conditions for scheduled operations",
                    RoutingRecommendation = "Proceed as planned with standard route",
                    AlertsGenerated = 0
                },
                ProcessingTime = "2.3 seconds",
                Confidence = "96.7%",
                Timestamp = DateTime.UtcNow
            };

            return Ok(processing);
        }

        /// <summary>
        /// Real-time passenger sentiment analysis from feedback and social media
        /// </summary>
        [HttpGet("passenger-sentiment")]
        public ActionResult<object> GetPassengerSentiment()
        {
            _logger.LogInformation("Analyzing passenger sentiment data");

            var sentiment = new
            {
                OverallSentiment = "Positive",
                SentimentScore = 4.7,
                AnalysisSource = "Azure Text Analytics with Norwegian language support",
                FeedbackChannels = new
                {
                    OnboardSurveys = new { Count = 342, AvgRating = 4.8, Sentiment = "Very Positive" },
                    SocialMedia = new { Mentions = 156, PositiveRatio = "87%", Sentiment = "Positive" },
                    TripAdvisor = new { Reviews = 89, AvgRating = 4.6, Sentiment = "Positive" },
                    GoogleReviews = new { Reviews = 234, AvRating = 4.7, Sentiment = "Very Positive" }
                },
                KeyTopics = new[]
                {
                    new { Topic = "Northern Lights Viewing", Sentiment = "Extremely Positive", Mentions = 89 },
                    new { Topic = "Cabin Comfort", Sentiment = "Positive", Mentions = 67 },
                    new { Topic = "Food Quality", Sentiment = "Very Positive", Mentions = 134 },
                    new { Topic = "Staff Service", Sentiment = "Excellent", Mentions = 156 },
                    new { Topic = "Environmental Awareness", Sentiment = "Positive", Mentions = 45 }
                },
                TrendingPhrases = new[]
                {
                    "Best Northern Lights experience ever!",
                    "Sustainable coastal travel at its finest",
                    "Professional crew and excellent service",
                    "Comfortable hybrid vessels with great views"
                },
                LanguageDistribution = new
                {
                    Norwegian = "45%",
                    English = "32%", 
                    German = "15%",
                    Other = "8%"
                },
                ActionableInsights = new[]
                {
                    "Passengers highly appreciate eco-friendly hybrid technology",
                    "Northern Lights viewing is the top satisfaction driver",
                    "Food service receiving consistently excellent feedback",
                    "Request for more information about environmental initiatives"
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(sentiment);
        }
    }

    // Request models for AI services
    public class RouteOptimizationRequest
    {
        public int VesselId { get; set; }
        public string DeparturePort { get; set; } = string.Empty;
        public string ArrivalPort { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public int PassengerCount { get; set; }
        public string[] WeatherPriorities { get; set; } = Array.Empty<string>();
        public bool OptimizeForFuel { get; set; } = true;
        public bool OptimizeForTime { get; set; } = false;
        public bool AvoidRoughSeas { get; set; } = true;
    }

    public class FuelOptimizationRequest
    {
        public int VesselId { get; set; }
        public string RouteId { get; set; } = string.Empty;
        public double CurrentFuelLevel { get; set; }
        public double BatterySOC { get; set; }
        public string[] OptimizationGoals { get; set; } = Array.Empty<string>();
    }

    public class DocumentProcessingRequest
    {
        public string DocumentType { get; set; } = string.Empty; // "Manifest", "Certificate", "Report"
        public string DocumentUrl { get; set; } = string.Empty;
        public string Language { get; set; } = "no"; // Norwegian by default
        public bool ExtractStructuredData { get; set; } = true;
        public bool PerformSentimentAnalysis { get; set; } = false;
    }
}