using Azure.AI.TextAnalytics;
using Azure.AI.Vision.ImageAnalysis;
using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Services
{
    public class CognitiveServicesConfiguration
    {
        public string TextAnalyticsEndpoint { get; set; } = string.Empty;
        public string TextAnalyticsKey { get; set; } = string.Empty;
        public string VisionEndpoint { get; set; } = string.Empty;
        public string VisionKey { get; set; } = string.Empty;
        public string TranslatorKey { get; set; } = string.Empty;
        public string TranslatorEndpoint { get; set; } = string.Empty;
        public string TranslatorRegion { get; set; } = string.Empty;
        public string SpeechServiceKey { get; set; } = string.Empty;
        public string SpeechServiceRegion { get; set; } = string.Empty;
    }

    public interface ICognitiveServicesService
    {
        Task<MaritimeTextAnalysisResult> AnalyzeMaritimeTextAsync(string text, string language = "en");
        Task<MaritimeImageAnalysisResult> AnalyzeMaritimeImageAsync(byte[] imageData);
        Task<string> TranslateMaritimeContentAsync(string text, string fromLanguage, string toLanguage);
        Task<MaritimeConversationResult> ProcessPassengerInquiryAsync(string inquiry, string language = "en");
        Task<WeatherAlertAnalysis> AnalyzeWeatherReportAsync(string weatherReport);
        Task<SafetyIncidentAnalysis> AnalyzeSafetyIncidentAsync(string incidentReport);
    }

    public class CognitiveServicesService : ICognitiveServicesService
    {
        private readonly TextAnalyticsClient _textAnalyticsClient;
        private readonly ImageAnalysisClient _visionClient;
        private readonly ILogger<CognitiveServicesService> _logger;
        private readonly CognitiveServicesConfiguration _config;

        public CognitiveServicesService(CognitiveServicesConfiguration config, ILogger<CognitiveServicesService> logger)
        {
            _config = config;
            _logger = logger;

            // Initialize Text Analytics client
            _textAnalyticsClient = new TextAnalyticsClient(
                new Uri(config.TextAnalyticsEndpoint),
                new AzureKeyCredential(config.TextAnalyticsKey));

            // Initialize Vision client
            _visionClient = new ImageAnalysisClient(
                new Uri(config.VisionEndpoint),
                new AzureKeyCredential(config.VisionKey));
        }

        public async Task<MaritimeTextAnalysisResult> AnalyzeMaritimeTextAsync(string text, string language = "en")
        {
            try
            {
                _logger.LogInformation("Analyzing maritime text content");

                // Sentiment analysis
                var sentimentResponse = await _textAnalyticsClient.AnalyzeSentimentAsync(text, language);
                var sentiment = sentimentResponse.Value;

                // Key phrase extraction
                var keyPhrasesResponse = await _textAnalyticsClient.ExtractKeyPhrasesAsync(text, language);
                var keyPhrases = keyPhrasesResponse.Value;

                // Entity recognition
                var entitiesResponse = await _textAnalyticsClient.RecognizeEntitiesAsync(text, language);
                var entities = entitiesResponse.Value;

                // Custom maritime entity extraction
                var maritimeEntities = ExtractMaritimeSpecificEntities(text);

                var result = new MaritimeTextAnalysisResult
                {
                    Text = text,
                    Language = language,
                    Sentiment = new SentimentAnalysis
                    {
                        Overall = sentiment.Sentiment.ToString(),
                        PositiveScore = (double)sentiment.ConfidenceScores.Positive,
                        NegativeScore = (double)sentiment.ConfidenceScores.Negative,
                        NeutralScore = (double)sentiment.ConfidenceScores.Neutral
                    },
                    KeyPhrases = keyPhrases.ToList(),
                    Entities = entities.Select(e => new EntityInfo
                    {
                        Text = e.Text,
                        Category = e.Category.ToString(),
                        Confidence = (double)e.ConfidenceScore
                    }).ToList(),
                    MaritimeEntities = maritimeEntities,
                    ProcessedAtUtc = DateTime.UtcNow
                };

                _logger.LogInformation("Maritime text analysis completed");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing maritime text");
                throw;
            }
        }

        public async Task<MaritimeImageAnalysisResult> AnalyzeMaritimeImageAsync(byte[] imageData)
        {
            try
            {
                _logger.LogInformation("Analyzing maritime image");

                var imageDataStream = BinaryData.FromBytes(imageData);
                var features = VisualFeatures.Caption | VisualFeatures.Tags | VisualFeatures.Objects | VisualFeatures.People;
                
                var result = await _visionClient.AnalyzeAsync(imageDataStream, features);
                var analysis = result.Value;

                // Detect maritime-specific elements
                var maritimeElements = DetectMaritimeElements(analysis);
                var safetyAssessment = AssessImageSafety(analysis);
                var weatherConditions = AnalyzeWeatherFromImage(analysis);

                var maritimeResult = new MaritimeImageAnalysisResult
                {
                    Caption = analysis.Caption?.Text ?? "No caption available",
                    Confidence = analysis.Caption?.Confidence ?? 0.0,
                    Tags = analysis.Tags?.Values?.Select(t => new ImageTag
                    {
                        Name = t.Name,
                        Confidence = t.Confidence
                    }).ToList() ?? new List<ImageTag>(),
                    Objects = analysis.Objects?.Values?.Select(o => new DetectedObject
                    {
                        Type = o.Tags?.FirstOrDefault()?.Name ?? "Unknown",
                        Label = o.Tags?.FirstOrDefault()?.Name ?? "Unknown",
                        ObjectType = o.Tags?.FirstOrDefault()?.Name ?? "Unknown",
                        Confidence = o.Tags?.FirstOrDefault()?.Confidence ?? 0.0,
                        BoundingBox = $"{o.BoundingBox.X},{o.BoundingBox.Y},{o.BoundingBox.Width},{o.BoundingBox.Height}"
                    }).ToList() ?? new List<DetectedObject>(),
                    MaritimeElements = maritimeElements,
                    SafetyAssessment = safetyAssessment,
                    WeatherConditions = weatherConditions,
                    ProcessedAtUtc = DateTime.UtcNow
                };

                _logger.LogInformation("Maritime image analysis completed");
                return maritimeResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing maritime image");
                throw;
            }
        }

        public async Task<string> TranslateMaritimeContentAsync(string text, string fromLanguage, string toLanguage)
        {
            try
            {
                _logger.LogInformation("Translating maritime content from {FromLang} to {ToLang}", fromLanguage, toLanguage);

                // Simulate async operation
                await Task.Delay(100);

                // For demo purposes, return a simple translation
                // In real implementation, use Azure Translator service
                var translations = new Dictionary<string, Dictionary<string, string>>
                {
                    ["en"] = new Dictionary<string, string>
                    {
                        ["no"] = TranslateToNorwegian(text),
                        ["da"] = TranslateToDanish(text),
                        ["sv"] = TranslateToSwedish(text),
                        ["de"] = TranslateToGerman(text)
                    },
                    ["no"] = new Dictionary<string, string>
                    {
                        ["en"] = TranslateFromNorwegian(text)
                    }
                };

                if (translations.ContainsKey(fromLanguage) && translations[fromLanguage].ContainsKey(toLanguage))
                {
                    return translations[fromLanguage][toLanguage];
                }

                return text; // Return original if translation not available
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating maritime content");
                throw;
            }
        }

        public async Task<MaritimeConversationResult> ProcessPassengerInquiryAsync(string inquiry, string language = "en")
        {
            try
            {
                _logger.LogInformation("Processing passenger inquiry");

                // Analyze inquiry intent
                var intent = DetermineInquiryIntent(inquiry);
                var entities = ExtractInquiryEntities(inquiry);
                
                // Generate appropriate response based on intent
                var response = await GenerateMaritimeResponse(intent, entities, language);

                var result = new MaritimeConversationResult
                {
                    OriginalInquiry = inquiry,
                    DetectedIntent = intent,
                    ExtractedEntities = entities,
                    Response = response,
                    Language = language,
                    ProcessedAtUtc = DateTime.UtcNow,
                    RequiresHumanFollowup = DeterminesHumanFollowup(intent, inquiry)
                };

                _logger.LogInformation("Passenger inquiry processed - Intent: {Intent}", intent);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing passenger inquiry");
                throw;
            }
        }

        public async Task<WeatherAlertAnalysis> AnalyzeWeatherReportAsync(string weatherReport)
        {
            await Task.CompletedTask; // Placeholder for async requirement

            var severity = DetermineWeatherSeverity(weatherReport);
            var conditions = ExtractWeatherConditions(weatherReport);
            var recommendations = GenerateWeatherRecommendations(severity, conditions);

            return new WeatherAlertAnalysis
            {
                WeatherReport = weatherReport,
                Severity = severity,
                Conditions = conditions,
                Recommendations = recommendations,
                AlertLevel = DetermineAlertLevel(severity),
                ProcessedAtUtc = DateTime.UtcNow
            };
        }

        public async Task<SafetyIncidentAnalysis> AnalyzeSafetyIncidentAsync(string incidentReport)
        {
            await Task.CompletedTask; // Placeholder for async requirement

            var classification = ClassifyIncident(incidentReport);
            var severity = DetermineIncidentSeverity(incidentReport);
            var requiredActions = DetermineRequiredActions(classification, severity);

            return new SafetyIncidentAnalysis
            {
                IncidentReport = incidentReport,
                Classification = classification,
                Severity = severity,
                RequiredActions = requiredActions,
                RequiresImmediateAttention = severity == "Critical" || severity == "High",
                ProcessedAtUtc = DateTime.UtcNow
            };
        }

        // Private helper methods
        private List<MaritimeEntity> ExtractMaritimeSpecificEntities(string text)
        {
            var entities = new List<MaritimeEntity>();
            var lowerText = text.ToLower();

            // Detect ports
            var norwegianPorts = new[] { "bergen", "ålesund", "alesund", "trondheim", "bodø", "bodo", "tromsø", "tromso", "kirkenes", "honningsvåg", "honningsvag" };
            foreach (var port in norwegianPorts)
            {
                if (lowerText.Contains(port))
                {
                    entities.Add(new MaritimeEntity { Type = "Port", Value = port, Confidence = 0.95 });
                }
            }

            // Detect vessel names
            var vesselNames = new[] { "havila castor", "havila pollux", "havila capella", "havila polaris" };
            foreach (var vessel in vesselNames)
            {
                if (lowerText.Contains(vessel))
                {
                    entities.Add(new MaritimeEntity { Type = "Vessel", Value = vessel, Confidence = 0.9 });
                }
            }

            return entities;
        }

        private List<string> DetectMaritimeElements(ImageAnalysisResult analysis)
        {
            var elements = new List<string>();
            var maritimeKeywords = new[] { "ship", "boat", "vessel", "sea", "ocean", "port", "harbor", "dock", "lighthouse", "fjord" };

            if (analysis.Tags?.Values != null)
            {
                foreach (var tag in analysis.Tags.Values)
                {
                    if (maritimeKeywords.Any(k => tag.Name.ToLower().Contains(k)))
                    {
                        elements.Add($"{tag.Name} (confidence: {tag.Confidence:F2})");
                    }
                }
            }

            return elements;
        }

        private string AssessImageSafety(ImageAnalysisResult analysis)
        {
            // Analyze image for safety concerns
            var safetyKeywords = new[] { "emergency", "fire", "smoke", "storm", "rough", "danger" };
            
            if (analysis.Tags?.Values != null)
            {
                var concerningTags = analysis.Tags.Values
                    .Where(t => safetyKeywords.Any(s => t.Name.ToLower().Contains(s)))
                    .ToList();

                if (concerningTags.Any())
                {
                    return $"Safety concerns detected: {string.Join(", ", concerningTags.Select(t => t.Name))}";
                }
            }

            return "Normal conditions";
        }

        private string AnalyzeWeatherFromImage(ImageAnalysisResult analysis)
        {
            var weatherKeywords = new Dictionary<string, string>
            {
                { "clear", "Clear skies" },
                { "cloud", "Cloudy conditions" },
                { "storm", "Stormy weather" },
                { "rain", "Rainy conditions" },
                { "snow", "Snowy conditions" },
                { "fog", "Foggy conditions" }
            };

            if (analysis.Tags?.Values != null)
            {
                foreach (var tag in analysis.Tags.Values)
                {
                    foreach (var weather in weatherKeywords)
                    {
                        if (tag.Name.ToLower().Contains(weather.Key))
                        {
                            return $"{weather.Value} (confidence: {tag.Confidence:F2})";
                        }
                    }
                }
            }

            return "Weather conditions unclear from image";
        }

        // Additional helper methods for translation and conversation processing
        private string TranslateToNorwegian(string text)
        {
            var translations = new Dictionary<string, string>
            {
                { "hello", "hei" },
                { "thank you", "takk" },
                { "vessel", "fartøy" },
                { "port", "havn" },
                { "departure", "avgang" },
                { "arrival", "ankomst" },
                { "weather", "vær" },
                { "safety", "sikkerhet" }
            };

            var result = text.ToLower();
            foreach (var translation in translations)
            {
                result = result.Replace(translation.Key, translation.Value);
            }
            return result;
        }

        private string TranslateToDanish(string text) => text; // Simplified for demo
        private string TranslateToSwedish(string text) => text; // Simplified for demo
        private string TranslateToGerman(string text) => text; // Simplified for demo
        private string TranslateFromNorwegian(string text) => text; // Simplified for demo

        private string DetermineInquiryIntent(string inquiry)
        {
            var inquiryLower = inquiry.ToLower();
            
            if (inquiryLower.Contains("booking") || inquiryLower.Contains("reservation"))
                return "BookingInquiry";
            if (inquiryLower.Contains("schedule") || inquiryLower.Contains("departure") || inquiryLower.Contains("arrival"))
                return "ScheduleInquiry";
            if (inquiryLower.Contains("weather"))
                return "WeatherInquiry";
            if (inquiryLower.Contains("cabin") || inquiryLower.Contains("room"))
                return "AccommodationInquiry";
            if (inquiryLower.Contains("food") || inquiryLower.Contains("restaurant") || inquiryLower.Contains("meal"))
                return "DiningInquiry";
            if (inquiryLower.Contains("northern lights") || inquiryLower.Contains("aurora"))
                return "NorthernLightsInquiry";
            
            return "GeneralInquiry";
        }

        private Dictionary<string, string> ExtractInquiryEntities(string inquiry)
        {
            var entities = new Dictionary<string, string>();
            
            // Extract dates, ports, vessel names, etc.
            var norwegianPorts = new[] { "bergen", "ålesund", "trondheim", "bodø", "tromsø", "kirkenes" };
            foreach (var port in norwegianPorts)
            {
                if (inquiry.ToLower().Contains(port))
                {
                    entities["port"] = port;
                    break;
                }
            }

            return entities;
        }

        private async Task<string> GenerateMaritimeResponse(string intent, Dictionary<string, string> entities, string language)
        {
            await Task.CompletedTask; // Placeholder for async requirement

            var responses = new Dictionary<string, string>
            {
                ["BookingInquiry"] = "I can help you with your booking. Please provide your confirmation number or contact our booking office at +47 76 96 76 00.",
                ["ScheduleInquiry"] = "Our vessels operate daily on the Bergen-Kirkenes route. The journey takes approximately 6.5 days. Would you like specific departure times?",
                ["WeatherInquiry"] = "Current weather conditions along the Norwegian coast are generally good for sailing. Check our real-time weather updates in the app.",
                ["NorthernLightsInquiry"] = "The Northern Lights season runs from September to March. Our best viewing locations are north of Bodø, particularly around Tromsø.",
                ["GeneralInquiry"] = "Thank you for contacting Havila Kystruten. How can I assist you with your journey along the beautiful Norwegian coast?"
            };

            return responses.ContainsKey(intent) ? responses[intent] : responses["GeneralInquiry"];
        }

        private bool DeterminesHumanFollowup(string intent, string inquiry)
        {
            var complexIntents = new[] { "ComplaintInquiry", "EmergencyInquiry", "RefundInquiry" };
            return complexIntents.Contains(intent) || inquiry.ToLower().Contains("complaint") || inquiry.ToLower().Contains("problem");
        }

        private string DetermineWeatherSeverity(string weatherReport)
        {
            var lowerReport = weatherReport.ToLower();
            
            if (lowerReport.Contains("storm") || lowerReport.Contains("gale") || lowerReport.Contains("severe"))
                return "High";
            if (lowerReport.Contains("rough") || lowerReport.Contains("moderate"))
                return "Medium";
            
            return "Low";
        }

        private List<string> ExtractWeatherConditions(string weatherReport)
        {
            var conditions = new List<string>();
            var weatherTerms = new[] { "wind", "rain", "snow", "fog", "clear", "cloudy", "storm" };
            
            foreach (var term in weatherTerms)
            {
                if (weatherReport.ToLower().Contains(term))
                {
                    conditions.Add(term);
                }
            }

            return conditions;
        }

        private List<string> GenerateWeatherRecommendations(string severity, List<string> conditions)
        {
            var recommendations = new List<string>();
            
            if (severity == "High")
            {
                recommendations.Add("Consider route adjustments");
                recommendations.Add("Increase safety monitoring");
                recommendations.Add("Inform passengers of conditions");
            }
            else if (severity == "Medium")
            {
                recommendations.Add("Monitor conditions closely");
                recommendations.Add("Prepare contingency plans");
            }
            else
            {
                recommendations.Add("Normal operations can continue");
            }

            return recommendations;
        }

        private string DetermineAlertLevel(string severity)
        {
            return severity switch
            {
                "High" => "Critical",
                "Medium" => "Warning",
                _ => "Information"
            };
        }

        private string ClassifyIncident(string incidentReport)
        {
            var lowerReport = incidentReport.ToLower();
            
            if (lowerReport.Contains("medical") || lowerReport.Contains("injury"))
                return "Medical";
            if (lowerReport.Contains("fire") || lowerReport.Contains("smoke"))
                return "Fire";
            if (lowerReport.Contains("collision") || lowerReport.Contains("grounding"))
                return "Navigation";
            if (lowerReport.Contains("security") || lowerReport.Contains("threat"))
                return "Security";
            
            return "General";
        }

        private string DetermineIncidentSeverity(string incidentReport)
        {
            var lowerReport = incidentReport.ToLower();
            
            if (lowerReport.Contains("fatal") || lowerReport.Contains("critical") || lowerReport.Contains("emergency"))
                return "Critical";
            if (lowerReport.Contains("serious") || lowerReport.Contains("major"))
                return "High";
            if (lowerReport.Contains("minor") || lowerReport.Contains("slight"))
                return "Low";
            
            return "Medium";
        }

        private List<string> DetermineRequiredActions(string classification, string severity)
        {
            var actions = new List<string>();
            
            if (severity == "Critical")
            {
                actions.Add("Immediate emergency response");
                actions.Add("Contact emergency services");
                actions.Add("Notify management");
            }
            
            if (classification == "Medical")
            {
                actions.Add("Medical assistance required");
                actions.Add("Contact ship's doctor");
            }
            
            if (classification == "Fire")
            {
                actions.Add("Activate fire suppression");
                actions.Add("Evacuate if necessary");
            }

            return actions;
        }
    }

    // Result models
    public class MaritimeTextAnalysisResult
    {
        public string Text { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public SentimentAnalysis Sentiment { get; set; } = new();
        public List<string> KeyPhrases { get; set; } = new();
        public List<EntityInfo> Entities { get; set; } = new();
        public List<MaritimeEntity> MaritimeEntities { get; set; } = new();
        public DateTime ProcessedAtUtc { get; set; }
    }

    public class MaritimeImageAnalysisResult
    {
        public string Caption { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public List<ImageTag> Tags { get; set; } = new();
        public List<DetectedObject> Objects { get; set; } = new();
        public List<string> MaritimeElements { get; set; } = new();
        public string SafetyAssessment { get; set; } = string.Empty;
        public string WeatherConditions { get; set; } = string.Empty;
        public DateTime ProcessedAtUtc { get; set; }
    }

    public class SentimentAnalysis
    {
        public string Overall { get; set; } = string.Empty;
        public double PositiveScore { get; set; }
        public double NegativeScore { get; set; }
        public double NeutralScore { get; set; }
    }

    public class EntityInfo
    {
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class MaritimeEntity
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class ImageTag
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class DetectedObject
    {
        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string BoundingBox { get; set; } = string.Empty;
    }

    public class MaritimeConversationResult
    {
        public string OriginalInquiry { get; set; } = string.Empty;
        public string DetectedIntent { get; set; } = string.Empty;
        public Dictionary<string, string> ExtractedEntities { get; set; } = new();
        public string Response { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public DateTime ProcessedAtUtc { get; set; }
        public bool RequiresHumanFollowup { get; set; }
    }

    public class WeatherAlertAnalysis
    {
        public string WeatherReport { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public List<string> Conditions { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string AlertLevel { get; set; } = string.Empty;
        public DateTime ProcessedAtUtc { get; set; }
    }

    public class SafetyIncidentAnalysis
    {
        public string IncidentReport { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public List<string> RequiredActions { get; set; } = new();
        public bool RequiresImmediateAttention { get; set; }
        public DateTime ProcessedAtUtc { get; set; }
    }

    // Extension methods for dependency injection
    public static class CognitiveServicesExtensions
    {
        public static IServiceCollection AddHavilaCognitiveServices(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("CognitiveServices").Get<CognitiveServicesConfiguration>();
            services.AddSingleton(config!);
            
            services.AddScoped<ICognitiveServicesService, CognitiveServicesService>();

            return services;
        }
    }
}