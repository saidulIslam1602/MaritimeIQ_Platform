using Microsoft.AspNetCore.Mvc;
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure.AI.OpenAI;
using Azure;
using System.Text.Json;
using HavilaKystruten.Maritime.Models;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaritimeIntelligenceController : ControllerBase
    {
        private readonly DocumentIntelligenceClient _documentIntelligenceClient;
        private readonly TextAnalyticsClient _textAnalyticsClient;
        private readonly SearchClient _searchClient;
        // TODO: Add correct OpenAI client when types are available
        // private readonly OpenAIClient _openAIClient;
        private readonly ILogger<MaritimeIntelligenceController> _logger;

        public MaritimeIntelligenceController(
            DocumentIntelligenceClient documentIntelligenceClient,
            TextAnalyticsClient textAnalyticsClient,
            SearchClient searchClient,
            // TODO: Add OpenAI client when types are available
            // OpenAIClient openAIClient,
            ILogger<MaritimeIntelligenceController> logger)
        {
            _documentIntelligenceClient = documentIntelligenceClient;
            _textAnalyticsClient = textAnalyticsClient;
            _searchClient = searchClient;
            // TODO: Initialize OpenAI client when types are available
            // _openAIClient = openAIClient;
            _logger = logger;
        }

        [HttpPost("analyze-maritime-document")]
        public async Task<IActionResult> AnalyzeMaritimeDocument([FromForm] IFormFile document)
        {
            try
            {
                if (document == null || document.Length == 0)
                    return BadRequest("No document provided");

                using var stream = document.OpenReadStream();
                
                // Analyze document with Document Intelligence
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var documentData = BinaryData.FromBytes(memoryStream.ToArray());
                
                var operation = await _documentIntelligenceClient.AnalyzeDocumentAsync(
                    WaitUntil.Completed, 
                    "prebuilt-document",
                    documentData);
                var result = operation.Value;
                
                var maritimeDocumentInfo = new MaritimeDocumentAnalysis
                {
                    DocumentType = DetermineMaritimeDocumentType(result),
                    ExtractedText = string.Join("\n", result.Pages.SelectMany(p => p.Lines.Select(l => l.Content))),
                    KeyValuePairs = result.KeyValuePairs.ToDictionary(
                        kvp => kvp.Key.Content,
                        kvp => kvp.Value?.Content ?? string.Empty),
                    Tables = result.Tables.Select(t => new DocumentTable
                    {
                        RowCount = t.RowCount,
                        ColumnCount = t.ColumnCount,
                        Cells = t.Cells.Select(c => new TableCell
                        {
                            Content = c.Content,
                            RowIndex = c.RowIndex,
                            ColumnIndex = c.ColumnIndex
                        }).ToList()
                    }).ToList()
                };

                // Perform text analytics on extracted content
                var sentimentAnalysis = await AnalyzeSentiment(maritimeDocumentInfo.ExtractedText);
                var keyPhrases = await ExtractKeyPhrases(maritimeDocumentInfo.ExtractedText);
                var entities = await RecognizeEntities(maritimeDocumentInfo.ExtractedText);

                maritimeDocumentInfo.SentimentScore = (double)sentimentAnalysis.ConfidenceScores.Positive;
                maritimeDocumentInfo.SentimentLabel = sentimentAnalysis.Sentiment.ToString();
                maritimeDocumentInfo.KeyPhrases = keyPhrases;
                maritimeDocumentInfo.RecognizedEntities = entities;

                // Index document for search
                await IndexMaritimeDocument(maritimeDocumentInfo);

                _logger.LogInformation($"Successfully analyzed maritime document: {document.FileName}");
                
                return Ok(maritimeDocumentInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing maritime document");
                return StatusCode(500, "Error processing document");
            }
        }

        [HttpPost("analyze-vessel-manifest")]
        public async Task<IActionResult> AnalyzeVesselManifest([FromForm] IFormFile manifest)
        {
            try
            {
                using var stream = manifest.OpenReadStream();
                
                // Use custom model for vessel manifests
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var documentData = BinaryData.FromBytes(memoryStream.ToArray());
                
                var operation = await _documentIntelligenceClient.AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    "prebuilt-invoice", // Using invoice model as proxy for structured maritime documents
                    documentData);

                var result = operation.Value;
                
                var vesselManifest = new VesselManifestAnalysis
                {
                    VesselName = ExtractVesselName(result),
                    DeparturePort = ExtractDeparturePort(result),
                    ArrivalPort = ExtractArrivalPort(result),
                    PassengerCount = ExtractPassengerCount(result),
                    CargoManifest = ExtractCargoDetails(result),
                    ComplianceDocuments = ExtractComplianceInfo(result),
                    CertificationStatus = ValidateCertifications(result)
                };

                return Ok(vesselManifest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing vessel manifest");
                return StatusCode(500, "Error processing manifest");
            }
        }

        [HttpPost("maritime-chat")]
        public async Task<IActionResult> MaritimeChatCompletion([FromBody] ChatRequest request)
        {
            try
            {
                // Search for relevant maritime information first
                var searchResults = await SearchMaritimeKnowledgeInternal(request.Query);
                
                                // TODO: Update to use the correct OpenAI SDK types
                // For now, return a simplified response based on search results
                var chatResponse = $"Based on maritime knowledge search, I found {searchResults.Count} relevant results. {string.Join(" ", searchResults.Take(2).Select(r => r.Content.Substring(0, Math.Min(100, r.Content.Length))))}";;

                return Ok(new ChatResponse
                {
                    Response = chatResponse,
                    RelevantSources = searchResults.Select(r => r.Source).ToList(),
                    Confidence = CalculateResponseConfidence(searchResults, request.Query)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing maritime chat request");
                return StatusCode(500, "Error processing chat request");
            }
        }

        [HttpGet("search-maritime-knowledge")]
        public async Task<IActionResult> SearchMaritimeKnowledge([FromQuery] string query, [FromQuery] int top = 10)
        {
            try
            {
                var searchResults = await SearchMaritimeKnowledgeInternal(query, top);
                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching maritime knowledge");
                return StatusCode(500, "Error performing search");
            }
        }

        [HttpPost("analyze-passenger-feedback")]
        public async Task<IActionResult> AnalyzePassengerFeedback([FromBody] List<string> feedbackTexts)
        {
            try
            {
                var feedbackAnalysis = new List<PassengerFeedbackAnalysis>();

                foreach (var feedback in feedbackTexts)
                {
                    var sentiment = await AnalyzeSentiment(feedback);
                    var keyPhrases = await ExtractKeyPhrases(feedback);
                    var entities = await RecognizeEntities(feedback);

                    feedbackAnalysis.Add(new PassengerFeedbackAnalysis
                    {
                        FeedbackText = feedback,
                        SentimentScore = (double)sentiment.ConfidenceScores.Positive,
                        SentimentLabel = sentiment.Sentiment.ToString(),
                        KeyPhrases = keyPhrases,
                        Topics = DetermineMaritimeFeedbackTopics(keyPhrases, entities),
                        Priority = CalculateFeedbackPriority(sentiment, keyPhrases),
                        ActionRequired = RequiresImmedateAction(sentiment, keyPhrases)
                    });
                }

                // Generate summary insights
                var summary = GenerateFeedbackSummary(feedbackAnalysis);

                return Ok(new
                {
                    FeedbackAnalysis = feedbackAnalysis,
                    Summary = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing passenger feedback");
                return StatusCode(500, "Error processing feedback");
            }
        }

        // Private helper methods
        private async Task<DocumentSentiment> AnalyzeSentiment(string text)
        {
            var response = await _textAnalyticsClient.AnalyzeSentimentAsync(text);
            return response.Value;
        }

        private async Task<List<string>> ExtractKeyPhrases(string text)
        {
            var response = await _textAnalyticsClient.ExtractKeyPhrasesAsync(text);
            return response.Value.ToList();
        }

        private async Task<List<CategorizedEntity>> RecognizeEntities(string text)
        {
            var response = await _textAnalyticsClient.RecognizeEntitiesAsync(text);
            return response.Value.ToList();
        }

        private async Task<List<MaritimeSearchResult>> SearchMaritimeKnowledgeInternal(string query, int top = 10)
        {
            var searchOptions = new SearchOptions()
            {
                Size = top,
                IncludeTotalCount = true,
                SearchFields = { "content", "title", "category" },
                Select = { "id", "title", "content", "category", "source", "timestamp" }
            };

            var searchResults = await _searchClient.SearchAsync<MaritimeDocument>(query, searchOptions);
            
            return searchResults.Value.GetResults().Select(result => new MaritimeSearchResult
            {
                Id = result.Document.Id,
                Title = result.Document.Title,
                Content = result.Document.Content,
                Source = result.Document.Source,
                Category = result.Document.Category,
                Score = result.Score ?? 0
            }).ToList();
        }

        private async Task IndexMaritimeDocument(MaritimeDocumentAnalysis document)
        {
            var maritimeDoc = new MaritimeDocument
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"Maritime Document - {document.DocumentType}",
                Content = document.ExtractedText,
                Category = document.DocumentType,
                Source = "Form Recognizer Analysis",
                Timestamp = DateTime.UtcNow,
                KeyPhrases = document.KeyPhrases,
                SentimentScore = document.SentimentScore
            };

            await _searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(new[] { maritimeDoc }));
        }

        private string DetermineMaritimeDocumentType(AnalyzeResult result)
        {
            var content = string.Join(" ", result.Pages.SelectMany(p => p.Lines.Select(l => l.Content))).ToLower();

            if (content.Contains("manifest") || content.Contains("cargo") || content.Contains("passenger list"))
                return "Vessel Manifest";
            if (content.Contains("certificate") || content.Contains("certification") || content.Contains("compliance"))
                return "Certification Document";
            if (content.Contains("incident") || content.Contains("report") || content.Contains("safety"))
                return "Incident Report";
            if (content.Contains("maintenance") || content.Contains("inspection") || content.Contains("service"))
                return "Maintenance Log";
            if (content.Contains("weather") || content.Contains("forecast") || content.Contains("conditions"))
                return "Weather Report";

            return "General Maritime Document";
        }

        private string ExtractVesselName(AnalyzeResult result)
        {
            // Logic to extract vessel name from structured document
            var vesselKeywords = new[] { "vessel", "ship", "mv", "ms" };
            foreach (var kvp in result.KeyValuePairs)
            {
                if (vesselKeywords.Any(k => kvp.Key.Content.ToLower().Contains(k)))
                {
                    return kvp.Value?.Content ?? "Unknown Vessel";
                }
            }
            return "Unknown Vessel";
        }

        private string ExtractDeparturePort(AnalyzeResult result) => ExtractPortInfo(result, "departure", "from", "origin");
        private string ExtractArrivalPort(AnalyzeResult result) => ExtractPortInfo(result, "arrival", "to", "destination");

        private string ExtractPortInfo(AnalyzeResult result, params string[] keywords)
        {
            foreach (var kvp in result.KeyValuePairs)
            {
                if (keywords.Any(k => kvp.Key.Content.ToLower().Contains(k)))
                {
                    return kvp.Value?.Content ?? "Unknown Port";
                }
            }
            return "Unknown Port";
        }

        private int ExtractPassengerCount(AnalyzeResult result)
        {
            foreach (var kvp in result.KeyValuePairs)
            {
                if (kvp.Key.Content.ToLower().Contains("passenger") && int.TryParse(kvp.Value?.Content, out int count))
                {
                    return count;
                }
            }
            return 0;
        }

        private List<string> ExtractCargoDetails(AnalyzeResult result)
        {
            var cargo = new List<string>();
            foreach (var table in result.Tables)
            {
                if (table.Cells.Any(c => c.Content.ToLower().Contains("cargo") || c.Content.ToLower().Contains("freight")))
                {
                    cargo.AddRange(table.Cells.Select(c => c.Content));
                }
            }
            return cargo;
        }

        private List<string> ExtractComplianceInfo(AnalyzeResult result)
        {
            return result.KeyValuePairs
                .Where(kvp => kvp.Key.Content.ToLower().Contains("certificate") || 
                             kvp.Key.Content.ToLower().Contains("compliance") ||
                             kvp.Key.Content.ToLower().Contains("license"))
                .Select(kvp => $"{kvp.Key.Content}: {kvp.Value?.Content}")
                .ToList();
        }

        private string ValidateCertifications(AnalyzeResult result)
        {
            var certifications = ExtractComplianceInfo(result);
            if (certifications.Count >= 3)
                return "Fully Compliant";
            else if (certifications.Count >= 1)
                return "Partially Compliant";
            else
                return "Compliance Review Required";
        }

        private List<string> DetermineMaritimeFeedbackTopics(List<string> keyPhrases, List<CategorizedEntity> entities)
        {
            var topics = new List<string>();
            var maritimeTopics = new Dictionary<string, string[]>
            {
                ["Service Quality"] = new[] { "service", "staff", "crew", "reception", "help" },
                ["Accommodation"] = new[] { "cabin", "room", "bed", "bathroom", "comfort" },
                ["Dining"] = new[] { "food", "restaurant", "meal", "breakfast", "dinner" },
                ["Entertainment"] = new[] { "entertainment", "activity", "music", "show", "event" },
                ["Route & Scenery"] = new[] { "route", "scenery", "view", "northern lights", "fjord" },
                ["Safety"] = new[] { "safety", "emergency", "life jacket", "drill", "security" },
                ["Environmental"] = new[] { "environment", "pollution", "green", "sustainable", "eco" }
            };

            foreach (var topic in maritimeTopics)
            {
                if (keyPhrases.Any(phrase => topic.Value.Any(keyword => 
                    phrase.ToLower().Contains(keyword))) ||
                    entities.Any(entity => topic.Value.Any(keyword => 
                    entity.Text.ToLower().Contains(keyword))))
                {
                    topics.Add(topic.Key);
                }
            }

            return topics.Any() ? topics : new List<string> { "General Feedback" };
        }

        private string CalculateFeedbackPriority(DocumentSentiment sentiment, List<string> keyPhrases)
        {
            if (sentiment.Sentiment == TextSentiment.Negative && sentiment.ConfidenceScores.Negative > 0.8)
                return "High";
            
            var urgentKeywords = new[] { "urgent", "emergency", "immediate", "serious", "dangerous", "unsafe" };
            if (keyPhrases.Any(phrase => urgentKeywords.Any(keyword => phrase.ToLower().Contains(keyword))))
                return "High";

            if (sentiment.Sentiment == TextSentiment.Negative)
                return "Medium";

            return "Low";
        }

        private bool RequiresImmedateAction(DocumentSentiment sentiment, List<string> keyPhrases)
        {
            var actionKeywords = new[] { "emergency", "urgent", "immediate", "safety", "medical", "help" };
            return keyPhrases.Any(phrase => actionKeywords.Any(keyword => phrase.ToLower().Contains(keyword))) ||
                   (sentiment.Sentiment == TextSentiment.Negative && sentiment.ConfidenceScores.Negative > 0.9);
        }

        private double CalculateResponseConfidence(List<MaritimeSearchResult> searchResults, string query)
        {
            if (!searchResults.Any()) return 0.1;
            
            var avgScore = searchResults.Average(r => r.Score);
            var relevantResults = searchResults.Count(r => r.Score > 0.5);
            
            return Math.Min(1.0, (avgScore + (relevantResults * 0.1)) / 2);
        }

        private FeedbackSummary GenerateFeedbackSummary(List<PassengerFeedbackAnalysis> feedbackList)
        {
            return new FeedbackSummary
            {
                TotalFeedbacks = feedbackList.Count,
                AverageSentimentScore = feedbackList.Average(f => f.SentimentScore),
                PositiveFeedbacks = feedbackList.Count(f => f.SentimentLabel == "Positive"),
                NegativeFeedbacks = feedbackList.Count(f => f.SentimentLabel == "Negative"),
                HighPriorityFeedbacks = feedbackList.Count(f => f.Priority == "High"),
                ActionRequiredFeedbacks = feedbackList.Count(f => f.ActionRequired),
                TopTopics = feedbackList.SelectMany(f => f.Topics)
                    .GroupBy(t => t)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => $"{g.Key} ({g.Count()})")
                    .ToList()
            };
        }
    }

    // Data models
    public class MaritimeDocumentAnalysis
    {
        public string DocumentType { get; set; } = string.Empty;
        public string ExtractedText { get; set; } = string.Empty;
        public Dictionary<string, string> KeyValuePairs { get; set; } = new();
        public List<DocumentTable> Tables { get; set; } = new();
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; } = string.Empty;
        public List<string> KeyPhrases { get; set; } = new();
        public List<CategorizedEntity> RecognizedEntities { get; set; } = new();
    }

    public class DocumentTable
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<TableCell> Cells { get; set; } = new();
    }

    public class TableCell
    {
        public string Content { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
    }

    public class VesselManifestAnalysis
    {
        public string VesselName { get; set; } = string.Empty;
        public string DeparturePort { get; set; } = string.Empty;
        public string ArrivalPort { get; set; } = string.Empty;
        public int PassengerCount { get; set; }
        public List<string> CargoManifest { get; set; } = new();
        public List<string> ComplianceDocuments { get; set; } = new();
        public string CertificationStatus { get; set; } = string.Empty;
    }

    public class MaritimeDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<string> KeyPhrases { get; set; } = new();
        public double SentimentScore { get; set; }
    }

    public class PassengerFeedbackAnalysis
    {
        public string FeedbackText { get; set; } = string.Empty;
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; } = string.Empty;
        public List<string> KeyPhrases { get; set; } = new();
        public List<string> Topics { get; set; } = new();
        public string Priority { get; set; } = string.Empty;
        public bool ActionRequired { get; set; }
    }

    public class FeedbackSummary
    {
        public int TotalFeedbacks { get; set; }
        public double AverageSentimentScore { get; set; }
        public int PositiveFeedbacks { get; set; }
        public int NegativeFeedbacks { get; set; }
        public int HighPriorityFeedbacks { get; set; }
        public int ActionRequiredFeedbacks { get; set; }
        public List<string> TopTopics { get; set; } = new();
    }
}