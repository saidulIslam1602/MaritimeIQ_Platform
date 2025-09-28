using Microsoft.AspNetCore.Mvc;
using Azure.AI.Vision.ImageAnalysis;
using Azure;
using System.Text.Json;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaritimeVisionController : ControllerBase
    {
        private readonly ImageAnalysisClient _visionClient;
        private readonly ILogger<MaritimeVisionController> _logger;

        public MaritimeVisionController(
            ImageAnalysisClient visionClient,
            ILogger<MaritimeVisionController> logger)
        {
            _visionClient = visionClient;
            _logger = logger;
        }

        [HttpPost("analyze-vessel-image")]
        public async Task<IActionResult> AnalyzeVesselImage([FromForm] IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return BadRequest("No image provided");

                using var stream = image.OpenReadStream();
                var imageData = BinaryData.FromStream(stream);

                var features = VisualFeatures.Caption | 
                              VisualFeatures.Read | 
                              VisualFeatures.Tags | 
                              VisualFeatures.Objects | 
                              VisualFeatures.People;

                var result = await _visionClient.AnalyzeAsync(imageData, features);

                var vesselImageAnalysis = new VesselImageAnalysis
                {
                    Caption = result.Value.Caption?.Text ?? "No caption available",
                    Confidence = result.Value.Caption?.Confidence ?? 0,
                    DetectedObjects = result.Value.Objects?.Values?.Select(obj => new DetectedObject
                    {
                        Name = obj.Tags.FirstOrDefault()?.Name ?? "Unknown",
                        Confidence = obj.Tags.FirstOrDefault()?.Confidence ?? 0,
                        BoundingBox = new BoundingBox
                        {
                            X = obj.BoundingBox.X,
                            Y = obj.BoundingBox.Y,
                            Width = obj.BoundingBox.Width,
                            Height = obj.BoundingBox.Height
                        }
                    }).ToList() ?? new List<DetectedObject>(),
                    Tags = result.Value.Tags?.Values?.Select(tag => new ImageTag
                    {
                        Name = tag.Name,
                        Confidence = tag.Confidence
                    }).ToList() ?? new List<ImageTag>(),
                    ReadResults = result.Value.Read?.Blocks?.SelectMany(block => 
                        block.Lines.Select(line => line.Text)).ToList() ?? new List<string>(),
                    PeopleCount = result.Value.People?.Values?.Count() ?? 0,
                    SafetyAssessment = AnalyzeSafetyFromImage(result.Value),
                    WeatherConditions = DetermineWeatherFromImage(result.Value),
                    VesselType = DetermineVesselType(result.Value),
                    MaritimeFeatures = IdentifyMaritimeFeatures(result.Value)
                };

                // Generate recommendations based on analysis
                vesselImageAnalysis.Recommendations = GenerateImageRecommendations(vesselImageAnalysis);

                _logger.LogInformation($"Successfully analyzed vessel image: {image.FileName}");
                
                return Ok(vesselImageAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing vessel image");
                return StatusCode(500, "Error processing image");
            }
        }

        [HttpPost("analyze-northern-lights")]
        public async Task<IActionResult> AnalyzeNorthernLights([FromForm] IFormFile image)
        {
            try
            {
                using var stream = image.OpenReadStream();
                var imageData = BinaryData.FromStream(stream);

                var features = VisualFeatures.Caption | VisualFeatures.Tags;
                var result = await _visionClient.AnalyzeAsync(imageData, features);

                var northernLightsAnalysis = new NorthernLightsAnalysis
                {
                    IsNorthernLightsDetected = DetectNorthernLights(result.Value),
                    LightIntensity = CalculateLightIntensity(result.Value),
                    ColorProfile = new List<string>(), // TODO: Color analysis not available in current SDK version
                    VisibilityScore = CalculateVisibilityScore(result.Value),
                    OptimalViewingConditions = AssessViewingConditions(result.Value),
                    RecommendedCameraSettings = GenerateCameraRecommendations(result.Value),
                    BestViewingTime = PredictBestViewingTime(),
                    LocationRecommendations = GetViewingLocationRecommendations()
                };

                return Ok(northernLightsAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing northern lights image");
                return StatusCode(500, "Error processing northern lights image");
            }
        }

        [HttpPost("analyze-port-facilities")]
        public async Task<IActionResult> AnalyzePortFacilities([FromForm] IFormFile image)
        {
            try
            {
                using var stream = image.OpenReadStream();
                var imageData = BinaryData.FromStream(stream);

                var features = VisualFeatures.Caption | VisualFeatures.Objects | VisualFeatures.Tags;
                var result = await _visionClient.AnalyzeAsync(imageData, features);

                var portAnalysis = new PortFacilitiesAnalysis
                {
                    DetectedFacilities = IdentifyPortFacilities(result.Value),
                    AccessibilityFeatures = IdentifyAccessibilityFeatures(result.Value),
                    SafetyEquipment = IdentifySafetyEquipment(result.Value),
                    CrowdDensity = EstimateCrowdDensity(result.Value),
                    WeatherConditions = DetermineWeatherFromImage(result.Value),
                    OperationalStatus = AssessOperationalStatus(result.Value),
                    PassengerAmenities = IdentifyPassengerAmenities(result.Value)
                };

                return Ok(portAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing port facilities");
                return StatusCode(500, "Error processing port image");
            }
        }

        [HttpPost("analyze-safety-equipment")]
        public async Task<IActionResult> AnalyzeSafetyEquipment([FromForm] IFormFile image)
        {
            try
            {
                using var stream = image.OpenReadStream();
                var imageData = BinaryData.FromStream(stream);

                var features = VisualFeatures.Objects | VisualFeatures.Tags;
                var result = await _visionClient.AnalyzeAsync(imageData, features);

                var safetyAnalysis = new SafetyEquipmentAnalysis
                {
                    LifeJackets = CountSafetyItem(result.Value, "life jacket", "vest"),
                    LifeBoats = CountSafetyItem(result.Value, "lifeboat", "boat"),
                    FireExtinguishers = CountSafetyItem(result.Value, "fire extinguisher", "extinguisher"),
                    EmergencyExits = CountSafetyItem(result.Value, "exit", "emergency exit"),
                    SafetySignage = CountSafetyItem(result.Value, "sign", "safety sign"),
                    FirstAidStations = CountSafetyItem(result.Value, "first aid", "medical kit"),
                    ComplianceScore = CalculateSafetyComplianceScore(result.Value),
                    Recommendations = GenerateSafetyRecommendations(result.Value),
                    InspectionNeeded = DetermineInspectionNeed(result.Value)
                };

                return Ok(safetyAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing safety equipment");
                return StatusCode(500, "Error processing safety image");
            }
        }

        [HttpPost("analyze-environmental-conditions")]
        public async Task<IActionResult> AnalyzeEnvironmentalConditions([FromForm] IFormFile image)
        {
            try
            {
                using var stream = image.OpenReadStream();
                var imageData = BinaryData.FromStream(stream);

                var features = VisualFeatures.Caption | VisualFeatures.Tags;
                var result = await _visionClient.AnalyzeAsync(imageData, features);

                var environmentalAnalysis = new EnvironmentalConditionsAnalysis
                {
                    WeatherConditions = DetermineWeatherFromImage(result.Value),
                    SeaState = DetermineSeaState(result.Value),
                    Visibility = CalculateVisibility(result.Value),
                    IceConditions = DetectIceConditions(result.Value),
                    WildlifePresence = DetectWildlife(result.Value),
                    PollutionIndicators = DetectPollution(result.Value),
                    EnvironmentalRisk = AssessEnvironmentalRisk(result.Value),
                    NavigationRecommendations = GenerateNavigationRecommendations(result.Value)
                };

                return Ok(environmentalAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing environmental conditions");
                return StatusCode(500, "Error processing environmental image");
            }
        }

        // Private helper methods
        private string AnalyzeSafetyFromImage(ImageAnalysisResult result)
        {
            var safetyTags = new[] { "life jacket", "safety", "emergency", "helmet", "vest" };
            var safetyItems = result.Tags?.Values?.Count(tag => 
                safetyTags.Any(safety => tag.Name.ToLower().Contains(safety))) ?? 0;

            if (safetyItems >= 3) return "Good Safety Compliance";
            if (safetyItems >= 1) return "Moderate Safety Compliance";
            return "Safety Review Recommended";
        }

        private string DetermineWeatherFromImage(ImageAnalysisResult result)
        {
            var weatherKeywords = new Dictionary<string, string>
            {
                ["sunny"] = "Clear and Sunny",
                ["cloudy"] = "Cloudy Conditions",
                ["rain"] = "Rainy Weather",
                ["storm"] = "Stormy Conditions",
                ["fog"] = "Foggy Conditions",
                ["snow"] = "Snowy Weather"
            };

            foreach (var keyword in weatherKeywords)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(keyword.Key)) == true ||
                    result.Caption?.Text?.ToLower().Contains(keyword.Key) == true)
                {
                    return keyword.Value;
                }
            }

            return "Weather Conditions Unknown";
        }

        private string DetermineVesselType(ImageAnalysisResult result)
        {
            var vesselTypes = new Dictionary<string, string>
            {
                ["cruise ship"] = "Cruise Vessel",
                ["ferry"] = "Ferry",
                ["cargo ship"] = "Cargo Vessel",
                ["fishing boat"] = "Fishing Vessel",
                ["yacht"] = "Private Yacht",
                ["sailboat"] = "Sailing Vessel"
            };

            foreach (var vesselType in vesselTypes)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(vesselType.Key)) == true ||
                    result.Caption?.Text?.ToLower().Contains(vesselType.Key) == true)
                {
                    return vesselType.Value;
                }
            }

            return "Vessel Type Unknown";
        }

        private List<string> IdentifyMaritimeFeatures(ImageAnalysisResult result)
        {
            var maritimeFeatures = new List<string>();
            var features = new[] { "deck", "bridge", "mast", "funnel", "anchor", "railing", "cabin", "window" };

            foreach (var feature in features)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(feature)) == true ||
                    result.Objects?.Values?.Any(obj => obj.Tags.Any(tag => tag.Name.ToLower().Contains(feature))) == true)
                {
                    maritimeFeatures.Add(feature.ToTitleCase());
                }
            }

            return maritimeFeatures;
        }

        private List<string> GenerateImageRecommendations(VesselImageAnalysis analysis)
        {
            var recommendations = new List<string>();

            if (analysis.PeopleCount > 10)
                recommendations.Add("High passenger density detected - ensure adequate safety measures");

            if (analysis.SafetyAssessment.Contains("Review"))
                recommendations.Add("Conduct safety equipment inspection");

            if (analysis.WeatherConditions.Contains("Storm") || analysis.WeatherConditions.Contains("Fog"))
                recommendations.Add("Exercise caution due to adverse weather conditions");

            if (analysis.VesselType.Contains("Unknown"))
                recommendations.Add("Verify vessel identification and classification");

            return recommendations;
        }

        private bool DetectNorthernLights(ImageAnalysisResult result)
        {
            var auroraKeywords = new[] { "aurora", "northern lights", "green light", "night sky" };
            return auroraKeywords.Any(keyword => 
                result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(keyword)) == true ||
                result.Caption?.Text?.ToLower().Contains(keyword) == true);
        }

        private double CalculateLightIntensity(ImageAnalysisResult result)
        {
            // TODO: Color analysis not available in current SDK version
            // Use tag-based analysis instead
            var lightKeywords = new[] { "bright", "light", "luminous", "glowing" };
            var lightIntensity = result.Tags?.Values?.Count(tag => 
                lightKeywords.Any(keyword => tag.Name.ToLower().Contains(keyword))) ?? 0;

            return Math.Min(1.0, lightIntensity * 0.25);
        }

        private double CalculateVisibilityScore(ImageAnalysisResult result)
        {
            if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains("clear")) == true)
                return 0.9;
            if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains("cloudy")) == true)
                return 0.6;
            if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains("fog")) == true)
                return 0.3;

            return 0.5; // Default moderate visibility
        }

        private List<string> AssessViewingConditions(ImageAnalysisResult result)
        {
            var conditions = new List<string>();

            if (DetectNorthernLights(result))
                conditions.Add("Aurora activity detected");

            if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains("dark")) == true)
                conditions.Add("Good darkness for viewing");

            if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains("clear")) == true)
                conditions.Add("Clear sky conditions");

            return conditions.Any() ? conditions : new List<string> { "Standard viewing conditions" };
        }

        private List<string> GenerateCameraRecommendations(ImageAnalysisResult result)
        {
            return new List<string>
            {
                "Use ISO 1600-3200 for optimal light sensitivity",
                "Set exposure time to 15-30 seconds",
                "Use wide-angle lens (14-24mm)",
                "Focus to infinity",
                "Use tripod for stability"
            };
        }

        private DateTime PredictBestViewingTime()
        {
            // Simple prediction - typically best between 9 PM and 2 AM
            var today = DateTime.Today;
            return today.AddHours(21); // 9 PM today
        }

        private List<string> GetViewingLocationRecommendations()
        {
            return new List<string>
            {
                "Upper deck - minimal light pollution",
                "Bow area - unobstructed north view",
                "Port side during eastbound sailing",
                "Starboard side during westbound sailing"
            };
        }

        private List<string> IdentifyPortFacilities(ImageAnalysisResult result)
        {
            var facilities = new List<string>();
            var facilityKeywords = new[] { "terminal", "dock", "pier", "building", "crane", "warehouse" };

            foreach (var keyword in facilityKeywords)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(keyword)) == true ||
                    result.Objects?.Values?.Any(obj => obj.Tags.Any(tag => tag.Name.ToLower().Contains(keyword))) == true)
                {
                    facilities.Add(keyword.ToTitleCase());
                }
            }

            return facilities;
        }

        private List<string> IdentifyAccessibilityFeatures(ImageAnalysisResult result)
        {
            var features = new List<string>();
            var accessibilityKeywords = new[] { "ramp", "elevator", "wheelchair", "handrail", "accessible" };

            foreach (var keyword in accessibilityKeywords)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(keyword)) == true)
                {
                    features.Add($"{keyword.ToTitleCase()} access available");
                }
            }

            return features.Any() ? features : new List<string> { "Standard accessibility features" };
        }

        private List<string> IdentifySafetyEquipment(ImageAnalysisResult result)
        {
            var equipment = new List<string>();
            var safetyItems = new[] { "life jacket", "fire extinguisher", "emergency exit", "safety sign" };

            foreach (var item in safetyItems)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(item)) == true)
                {
                    equipment.Add(item.ToTitleCase());
                }
            }

            return equipment;
        }

        private string EstimateCrowdDensity(ImageAnalysisResult result)
        {
            var peopleCount = result.People?.Values?.Count() ?? 0;

            if (peopleCount > 20) return "High Density";
            if (peopleCount > 10) return "Moderate Density";
            if (peopleCount > 0) return "Low Density";
            return "No Crowd Detected";
        }

        private string AssessOperationalStatus(ImageAnalysisResult result)
        {
            var operationalIndicators = new[] { "open", "active", "working", "operational" };
            var closedIndicators = new[] { "closed", "empty", "maintenance" };

            if (result.Tags?.Values?.Any(tag => operationalIndicators.Any(ind => tag.Name.ToLower().Contains(ind))) == true)
                return "Operational";

            if (result.Tags?.Values?.Any(tag => closedIndicators.Any(ind => tag.Name.ToLower().Contains(ind))) == true)
                return "Closed/Maintenance";

            return "Status Unknown";
        }

        private List<string> IdentifyPassengerAmenities(ImageAnalysisResult result)
        {
            var amenities = new List<string>();
            var amenityKeywords = new[] { "restaurant", "shop", "cafe", "restroom", "seating", "information" };

            foreach (var amenity in amenityKeywords)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(amenity)) == true)
                {
                    amenities.Add(amenity.ToTitleCase());
                }
            }

            return amenities.Any() ? amenities : new List<string> { "Basic amenities available" };
        }

        private int CountSafetyItem(ImageAnalysisResult result, params string[] itemKeywords)
        {
            return result.Objects?.Values?.Count(obj => 
                obj.Tags.Any(tag => itemKeywords.Any(keyword => 
                    tag.Name.ToLower().Contains(keyword)))) ?? 0;
        }

        private double CalculateSafetyComplianceScore(ImageAnalysisResult result)
        {
            var totalSafetyItems = CountSafetyItem(result, "life jacket", "lifeboat", "fire extinguisher", 
                                                  "emergency exit", "safety sign", "first aid");
            
            // Score based on number of safety items detected
            return Math.Min(1.0, totalSafetyItems * 0.15);
        }

        private List<string> GenerateSafetyRecommendations(ImageAnalysisResult result)
        {
            var recommendations = new List<string>();
            var complianceScore = CalculateSafetyComplianceScore(result);

            if (complianceScore < 0.5)
                recommendations.Add("Increase visibility of safety equipment");

            if (CountSafetyItem(result, "life jacket") < 2)
                recommendations.Add("Ensure adequate life jacket availability");

            if (CountSafetyItem(result, "emergency exit") < 1)
                recommendations.Add("Verify emergency exit signage is visible");

            return recommendations.Any() ? recommendations : new List<string> { "Safety compliance appears adequate" };
        }

        private bool DetermineInspectionNeed(ImageAnalysisResult result)
        {
            return CalculateSafetyComplianceScore(result) < 0.6;
        }

        private string DetermineSeaState(ImageAnalysisResult result)
        {
            var seaStates = new Dictionary<string, string>
            {
                ["calm"] = "Calm (0-1)",
                ["smooth"] = "Smooth (1-2)",
                ["slight"] = "Slight (2-3)",
                ["moderate"] = "Moderate (3-4)",
                ["rough"] = "Rough (4-5)",
                ["very rough"] = "Very Rough (5-6)"
            };

            foreach (var state in seaStates)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(state.Key)) == true ||
                    result.Caption?.Text?.ToLower().Contains(state.Key) == true)
                {
                    return state.Value;
                }
            }

            return "Sea State Unknown";
        }

        private double CalculateVisibility(ImageAnalysisResult result)
        {
            if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains("clear")) == true)
                return 10.0; // Excellent visibility (10+ km)
            if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains("hazy")) == true)
                return 5.0; // Moderate visibility (5 km)
            if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains("fog")) == true)
                return 1.0; // Poor visibility (1 km)

            return 7.0; // Default good visibility
        }

        private string DetectIceConditions(ImageAnalysisResult result)
        {
            var iceKeywords = new[] { "ice", "snow", "frozen", "winter" };
            
            if (iceKeywords.Any(keyword => 
                result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(keyword)) == true))
            {
                return "Ice conditions detected - exercise caution";
            }

            return "No ice conditions detected";
        }

        private List<string> DetectWildlife(ImageAnalysisResult result)
        {
            var wildlife = new List<string>();
            var wildlifeKeywords = new[] { "bird", "whale", "seal", "fish", "eagle", "seagull" };

            foreach (var animal in wildlifeKeywords)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(animal)) == true ||
                    result.Objects?.Values?.Any(obj => obj.Tags.Any(tag => tag.Name.ToLower().Contains(animal))) == true)
                {
                    wildlife.Add(animal.ToTitleCase());
                }
            }

            return wildlife;
        }

        private List<string> DetectPollution(ImageAnalysisResult result)
        {
            var pollution = new List<string>();
            var pollutionKeywords = new[] { "smoke", "oil", "debris", "waste", "pollution" };

            foreach (var pollutant in pollutionKeywords)
            {
                if (result.Tags?.Values?.Any(tag => tag.Name.ToLower().Contains(pollutant)) == true)
                {
                    pollution.Add($"{pollutant.ToTitleCase()} detected");
                }
            }

            return pollution;
        }

        private string AssessEnvironmentalRisk(ImageAnalysisResult result)
        {
            var riskFactors = 0;

            if (DetectPollution(result).Any()) riskFactors++;
            if (DetermineSeaState(result).Contains("Rough")) riskFactors++;
            if (CalculateVisibility(result) < 2.0) riskFactors++;
            if (DetectIceConditions(result).Contains("detected")) riskFactors++;

            return riskFactors switch
            {
                0 => "Low Environmental Risk",
                1 => "Moderate Environmental Risk",
                2 => "High Environmental Risk",
                _ => "Very High Environmental Risk"
            };
        }

        private List<string> GenerateNavigationRecommendations(ImageAnalysisResult result)
        {
            var recommendations = new List<string>();

            if (CalculateVisibility(result) < 2.0)
                recommendations.Add("Reduce speed due to limited visibility");

            if (DetermineSeaState(result).Contains("Rough"))
                recommendations.Add("Exercise caution in rough sea conditions");

            if (DetectIceConditions(result).Contains("detected"))
                recommendations.Add("Monitor ice conditions and adjust route if necessary");

            if (DetectPollution(result).Any())
                recommendations.Add("Report pollution sighting to maritime authorities");

            return recommendations.Any() ? recommendations : new List<string> { "Normal navigation conditions" };
        }
    }

    // Data models for maritime vision analysis
    public class VesselImageAnalysis
    {
        public string Caption { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public List<DetectedObject> DetectedObjects { get; set; } = new();
        public List<ImageTag> Tags { get; set; } = new();
        public List<string> ReadResults { get; set; } = new();
        public int PeopleCount { get; set; }
        public string SafetyAssessment { get; set; } = string.Empty;
        public string WeatherConditions { get; set; } = string.Empty;
        public string VesselType { get; set; } = string.Empty;
        public List<string> MaritimeFeatures { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class DetectedObject
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public BoundingBox BoundingBox { get; set; } = new();
    }

    public class BoundingBox
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ImageTag
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class NorthernLightsAnalysis
    {
        public bool IsNorthernLightsDetected { get; set; }
        public double LightIntensity { get; set; }
        public List<string> ColorProfile { get; set; } = new();
        public double VisibilityScore { get; set; }
        public List<string> OptimalViewingConditions { get; set; } = new();
        public List<string> RecommendedCameraSettings { get; set; } = new();
        public DateTime BestViewingTime { get; set; }
        public List<string> LocationRecommendations { get; set; } = new();
    }

    public class PortFacilitiesAnalysis
    {
        public List<string> DetectedFacilities { get; set; } = new();
        public List<string> AccessibilityFeatures { get; set; } = new();
        public List<string> SafetyEquipment { get; set; } = new();
        public string CrowdDensity { get; set; } = string.Empty;
        public string WeatherConditions { get; set; } = string.Empty;
        public string OperationalStatus { get; set; } = string.Empty;
        public List<string> PassengerAmenities { get; set; } = new();
    }

    public class SafetyEquipmentAnalysis
    {
        public int LifeJackets { get; set; }
        public int LifeBoats { get; set; }
        public int FireExtinguishers { get; set; }
        public int EmergencyExits { get; set; }
        public int SafetySignage { get; set; }
        public int FirstAidStations { get; set; }
        public double ComplianceScore { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public bool InspectionNeeded { get; set; }
    }

    public class EnvironmentalConditionsAnalysis
    {
        public string WeatherConditions { get; set; } = string.Empty;
        public string SeaState { get; set; } = string.Empty;
        public double Visibility { get; set; }
        public string IceConditions { get; set; } = string.Empty;
        public List<string> WildlifePresence { get; set; } = new();
        public List<string> PollutionIndicators { get; set; } = new();
        public string EnvironmentalRisk { get; set; } = string.Empty;
        public List<string> NavigationRecommendations { get; set; } = new();
    }
}

// Extension method for string formatting
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }
}