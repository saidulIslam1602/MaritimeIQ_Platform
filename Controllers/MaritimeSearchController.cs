using Microsoft.AspNetCore.Mvc;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure;
using System.Text.Json;
using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaritimeSearchController : ControllerBase
    {
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _searchIndexClient;
        private readonly ILogger<MaritimeSearchController> _logger;

        public MaritimeSearchController(
            SearchClient searchClient,
            SearchIndexClient searchIndexClient,
            ILogger<MaritimeSearchController> logger)
        {
            _searchClient = searchClient;
            _searchIndexClient = searchIndexClient;
            _logger = logger;
        }

        [HttpPost("initialize-maritime-index")]
        public async Task<IActionResult> InitializeMaritimeIndex()
        {
            try
            {
                var fieldBuilder = new FieldBuilder();
                var searchFields = fieldBuilder.Build(typeof(MaritimeSearchDocument));

                var definition = new SearchIndex("maritime-knowledge", searchFields);
                
                // Add custom scoring profile for maritime relevance
                definition.ScoringProfiles.Add(new ScoringProfile("maritime-boost")
                {
                    FunctionAggregation = ScoringFunctionAggregation.Sum,
                    Functions =
                    {
                        new MagnitudeScoringFunction("priority", 2.0, new MagnitudeScoringParameters(1, 10) { ShouldBoostBeyondRangeByConstant = true }),
                        new FreshnessScoringFunction("lastUpdated", 1.5, new FreshnessScoringParameters(TimeSpan.FromDays(30)))
                    },
                    TextWeights = new TextWeights(new Dictionary<string, double>
                    {
                        ["title"] = 3.0,
                        ["content"] = 2.0,
                        ["category"] = 1.5,
                        ["tags"] = 1.0
                    })
                });

                // Add suggesters for autocomplete
                definition.Suggesters.Add(new SearchSuggester("maritime-suggester", "title", "content", "tags"));

                await _searchIndexClient.CreateOrUpdateIndexAsync(definition);

                // Populate with initial maritime knowledge
                await PopulateInitialMaritimeKnowledge();

                _logger.LogInformation("Maritime search index initialized successfully");
                return Ok(new { message = "Maritime search index created and populated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing maritime search index");
                return StatusCode(500, "Error creating search index");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] string category = "",
            [FromQuery] int top = 10,
            [FromQuery] int skip = 0,
            [FromQuery] bool includeCount = true)
        {
            try
            {
                var searchOptions = new SearchOptions
                {
                    Size = top,
                    Skip = skip,
                    IncludeTotalCount = includeCount,
                    ScoringProfile = "maritime-boost",
                    SearchFields = { "title", "content", "category", "tags" },
                    Select = { "id", "title", "content", "category", "source", "lastUpdated", "priority", "tags", "vesselType", "routeInfo" },
                    HighlightFields = { "title", "content" },
                    QueryType = SearchQueryType.Full
                };

                // Add category filter if specified
                if (!string.IsNullOrEmpty(category))
                {
                    searchOptions.Filter = $"category eq '{category}'";
                }

                // Add facets for filtering
                searchOptions.Facets.Add("category");
                searchOptions.Facets.Add("vesselType");
                searchOptions.Facets.Add("source");

                var searchResults = await _searchClient.SearchAsync<MaritimeSearchDocument>(query, searchOptions);

                var response = new MaritimeSearchResponse
                {
                    TotalCount = searchResults.Value.TotalCount ?? 0,
                    Results = searchResults.Value.GetResults().Select(result => new MaritimeSearchResult
                    {
                        Id = result.Document.Id,
                        Title = result.Document.Title,
                        Content = TruncateContent(result.Document.Content, 300),
                        Category = result.Document.Category,
                        Source = result.Document.Source,
                        Score = result.Score ?? 0
                    }).ToList(),
                    Facets = searchResults.Value.Facets?.ToDictionary(
                        f => f.Key, 
                        f => f.Value.Select(v => new FacetResult 
                        { 
                            Value = v.Value?.ToString() ?? "", 
                            Count = v.Count ?? 0 
                        }).ToList()
                    ) ?? new Dictionary<string, List<FacetResult>>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing maritime search");
                return StatusCode(500, "Error performing search");
            }
        }

        [HttpGet("suggest")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string query, [FromQuery] int top = 5)
        {
            try
            {
                var suggestOptions = new SuggestOptions
                {
                    Size = top,
                    UseFuzzyMatching = true,
                    Select = { "title", "category" }
                };

                var suggestions = await _searchClient.SuggestAsync<MaritimeSearchDocument>(
                    query, "maritime-suggester", suggestOptions);

                var response = suggestions.Value.Results.Select(suggestion => new MaritimeSuggestion
                {
                    Text = suggestion.Text,
                    Title = suggestion.Document.Title,
                    Category = suggestion.Document.Category
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions");
                return StatusCode(500, "Error getting suggestions");
            }
        }

        [HttpPost("add-document")]
        public async Task<IActionResult> AddDocument([FromBody] MaritimeSearchDocument document)
        {
            try
            {
                document.Id = Guid.NewGuid().ToString();
                document.LastUpdated = DateTime.UtcNow;

                var batch = IndexDocumentsBatch.Upload(new[] { document });
                await _searchClient.IndexDocumentsAsync(batch);

                _logger.LogInformation($"Added maritime document: {document.Title}");
                return Ok(new { id = document.Id, message = "Document added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding document to search index");
                return StatusCode(500, "Error adding document");
            }
        }

        [HttpPut("update-document/{id}")]
        public async Task<IActionResult> UpdateDocument(string id, [FromBody] MaritimeSearchDocument document)
        {
            try
            {
                document.Id = id;
                document.LastUpdated = DateTime.UtcNow;

                var batch = IndexDocumentsBatch.MergeOrUpload(new[] { document });
                await _searchClient.IndexDocumentsAsync(batch);

                _logger.LogInformation($"Updated maritime document: {id}");
                return Ok(new { message = "Document updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document in search index");
                return StatusCode(500, "Error updating document");
            }
        }

        [HttpDelete("delete-document/{id}")]
        public async Task<IActionResult> DeleteDocument(string id)
        {
            try
            {
                var batch = IndexDocumentsBatch.Delete("id", new[] { id });
                await _searchClient.IndexDocumentsAsync(batch);

                _logger.LogInformation($"Deleted maritime document: {id}");
                return Ok(new { message = "Document deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document from search index");
                return StatusCode(500, "Error deleting document");
            }
        }

        [HttpGet("vessel-information")]
        public async Task<IActionResult> SearchVesselInformation([FromQuery] string vesselName, [FromQuery] string vesselType = "")
        {
            try
            {
                var filter = $"category eq 'Vessel Information'";
                if (!string.IsNullOrEmpty(vesselType))
                {
                    filter += $" and vesselType eq '{vesselType}'";
                }

                var searchOptions = new SearchOptions
                {
                    Filter = filter,
                    Size = 50,
                    ScoringProfile = "maritime-boost",
                    OrderBy = { "priority desc", "lastUpdated desc" }
                };

                var searchResults = await _searchClient.SearchAsync<MaritimeSearchDocument>(vesselName, searchOptions);

                var vessels = searchResults.Value.GetResults().Select(result => new VesselInformation
                {
                    Name = result.Document.Title,
                    Type = result.Document.VesselType,
                    Description = result.Document.Content,
                    RouteInfo = result.Document.RouteInfo,
                    LastUpdated = result.Document.LastUpdated,
                    Features = result.Document.Tags
                }).ToList();

                return Ok(vessels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vessel information");
                return StatusCode(500, "Error searching vessel information");
            }
        }

        [HttpGet("route-guidance")]
        public async Task<IActionResult> SearchRouteGuidance([FromQuery] string route, [FromQuery] string season = "")
        {
            try
            {
                var filter = "category eq 'Route Information'";
                var searchQuery = route;

                if (!string.IsNullOrEmpty(season))
                {
                    searchQuery += $" {season}";
                }

                var searchOptions = new SearchOptions
                {
                    Filter = filter,
                    Size = 20,
                    ScoringProfile = "maritime-boost",
                    OrderBy = { "priority desc" }
                };

                var searchResults = await _searchClient.SearchAsync<MaritimeSearchDocument>(searchQuery, searchOptions);

                var routeGuidance = searchResults.Value.GetResults().Select(result => new RouteGuidance
                {
                    RouteName = result.Document.Title,
                    Description = result.Document.Content,
                    RouteDetails = result.Document.RouteInfo,
                    SeasonalInfo = ExtractSeasonalInfo(result.Document.Content),
                    ImportantNotes = result.Document.Tags,
                    LastUpdated = result.Document.LastUpdated
                }).ToList();

                return Ok(routeGuidance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching route guidance");
                return StatusCode(500, "Error searching route guidance");
            }
        }

        [HttpGet("safety-procedures")]
        public async Task<IActionResult> SearchSafetyProcedures([FromQuery] string query)
        {
            try
            {
                var searchOptions = new SearchOptions
                {
                    Filter = "category eq 'Safety Procedures'",
                    Size = 30,
                    ScoringProfile = "maritime-boost",
                    OrderBy = { "priority desc" }
                };

                var searchResults = await _searchClient.SearchAsync<MaritimeSearchDocument>(query, searchOptions);

                var procedures = searchResults.Value.GetResults().Select(result => new SafetyProcedure
                {
                    Title = result.Document.Title,
                    Content = result.Document.Content,
                    Priority = GetPriorityLevel(result.Document.Priority),
                    ApplicableVessels = result.Document.VesselType,
                    LastUpdated = result.Document.LastUpdated,
                    RelatedProcedures = result.Document.Tags
                }).ToList();

                return Ok(procedures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching safety procedures");
                return StatusCode(500, "Error searching safety procedures");
            }
        }

        [HttpGet("northern-lights-info")]
        public async Task<IActionResult> GetNorthernLightsInformation([FromQuery] string location = "")
        {
            try
            {
                var searchQuery = "northern lights aurora borealis";
                if (!string.IsNullOrEmpty(location))
                {
                    searchQuery += $" {location}";
                }

                var searchOptions = new SearchOptions
                {
                    Filter = "category eq 'Northern Lights' or category eq 'Route Information'",
                    Size = 15,
                    ScoringProfile = "maritime-boost"
                };

                var searchResults = await _searchClient.SearchAsync<MaritimeSearchDocument>(searchQuery, searchOptions);

                var northernLightsInfo = searchResults.Value.GetResults().Select(result => new NorthernLightsInfo
                {
                    Title = result.Document.Title,
                    Description = result.Document.Content,
                    Location = ExtractLocationInfo(result.Document.Content),
                    BestViewingTimes = ExtractViewingTimes(result.Document.Content),
                    ViewingTips = result.Document.Tags,
                    RouteSpecific = result.Document.RouteInfo,
                    LastUpdated = result.Document.LastUpdated
                }).ToList();

                return Ok(northernLightsInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching northern lights information");
                return StatusCode(500, "Error searching northern lights information");
            }
        }

        // Private helper methods
        private async Task PopulateInitialMaritimeKnowledge()
        {
            var maritimeDocuments = new List<MaritimeSearchDocument>
            {
                // Maritime Vessels Information
                new MaritimeSearchDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "MS Nordic Aurora - Hybrid Cruise Ferry",
                    Content = "MS Nordic Aurora is a modern hybrid vessel operating on the Bergen-Kirkenes route. Features advanced environmental technology, luxury accommodations, and excellent Northern Lights viewing facilities. Built in 2021 with focus on sustainability and passenger comfort.",
                    Category = "Vessel Information",
                    Source = "Maritime Fleet Database",
                    VesselType = "Hybrid Cruise Ferry",
                    RouteInfo = "Bergen - Kirkenes - Bergen (6-day roundtrip)",
                    Priority = 10,
                    Tags = new[] { "hybrid", "modern", "luxury", "eco-friendly", "northern lights", "2021" },
                    LastUpdated = DateTime.UtcNow
                },
                new MaritimeSearchDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "MS Arctic Explorer - Hybrid Coastal Vessel",
                    Content = "MS Arctic Explorer operates the beautiful Norwegian coastal route with state-of-the-art environmental systems. Features panoramic windows, premium dining options, and dedicated Northern Lights observation decks. Battery power enables silent operation in sensitive areas.",
                    Category = "Vessel Information",
                    Source = "Maritime Fleet Database",
                    VesselType = "Hybrid Cruise Ferry",
                    RouteInfo = "Bergen - Kirkenes - Bergen",
                    Priority = 10,
                    Tags = new[] { "hybrid", "silent operation", "panoramic views", "premium dining", "environmental" },
                    LastUpdated = DateTime.UtcNow
                },

                // Route Information
                new MaritimeSearchDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Bergen to Kirkenes Coastal Route",
                    Content = "The classic Norwegian coastal voyage covers 2,500 nautical miles in 6 days, visiting 34 ports along the way. Experience dramatic fjords, the Arctic Circle crossing, and excellent Northern Lights viewing opportunities during winter months. Route includes stops at Geiranger, Trondheim, Tromsø, and Honningsvåg.",
                    Category = "Route Information",
                    Source = "Navigation Department",
                    VesselType = "All Vessels",
                    RouteInfo = "34 ports, 6 days, 2500 nautical miles",
                    Priority = 9,
                    Tags = new[] { "coastal voyage", "fjords", "arctic circle", "34 ports", "winter", "summer" },
                    LastUpdated = DateTime.UtcNow
                },

                // Northern Lights Information
                new MaritimeSearchDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Northern Lights Viewing Guide",
                    Content = "Best Northern Lights viewing occurs from September to March when sailing north of the Arctic Circle. Optimal viewing times are between 6 PM and 2 AM. Recommended viewing locations include upper decks away from artificial lighting. Weather conditions and solar activity affect visibility.",
                    Category = "Northern Lights",
                    Source = "Guest Services",
                    VesselType = "All Vessels",
                    RouteInfo = "Arctic Circle and beyond",
                    Priority = 8,
                    Tags = new[] { "september", "march", "arctic circle", "evening", "upper deck", "weather dependent" },
                    LastUpdated = DateTime.UtcNow
                },

                // Safety Procedures
                new MaritimeSearchDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Emergency Evacuation Procedures",
                    Content = "In case of emergency, passengers must proceed to their designated muster stations immediately upon hearing the general alarm. Life jackets are located in all cabins and common areas. Follow crew instructions and remain calm. Emergency exits are clearly marked throughout the vessel.",
                    Category = "Safety Procedures",
                    Source = "Safety Department",
                    VesselType = "All Vessels",
                    RouteInfo = "All Routes",
                    Priority = 10,
                    Tags = new[] { "emergency", "muster station", "life jackets", "crew instructions", "evacuation" },
                    LastUpdated = DateTime.UtcNow
                },

                // Port Information
                new MaritimeSearchDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Tromsø Port Guide",
                    Content = "Tromsø is known as the Northern Lights capital and offers excellent opportunities for aurora viewing. Port facilities include passenger terminal with amenities, gift shops, and local transportation. Recommended excursions include cable car to Mount Storsteinen, Arctic Cathedral visit, and Northern Lights tours.",
                    Category = "Port Information",
                    Source = "Port Services",
                    VesselType = "All Vessels",
                    RouteInfo = "Northbound and Southbound",
                    Priority = 7,
                    Tags = new[] { "northern lights capital", "cable car", "arctic cathedral", "excursions", "storsteinen" },
                    LastUpdated = DateTime.UtcNow
                },

                // Environmental Information
                new MaritimeSearchDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Sustainable Maritime Operations",
                    Content = "Maritime operations utilize hybrid technology to minimize environmental impact. Battery operation in UNESCO World Heritage sites and sensitive areas. Advanced wastewater treatment, reduced emissions, and responsible waste management. Supporting marine conservation along the Norwegian coast.",
                    Category = "Environmental",
                    Source = "Environmental Department",
                    VesselType = "Hybrid Vessels",
                    RouteInfo = "UNESCO sites and protected areas",
                    Priority = 8,
                    Tags = new[] { "hybrid technology", "unesco", "battery operation", "emissions", "conservation" },
                    LastUpdated = DateTime.UtcNow
                }
            };

            var batch = IndexDocumentsBatch.Upload(maritimeDocuments);
            await _searchClient.IndexDocumentsAsync(batch);
        }

        private string TruncateContent(string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
                return content;

            return content.Substring(0, maxLength) + "...";
        }

        private string ExtractSeasonalInfo(string content)
        {
            var seasons = new[] { "winter", "summer", "spring", "autumn", "september", "march", "december" };
            var seasonalInfo = seasons.Where(season => 
                content.ToLower().Contains(season)).ToList();

            return seasonalInfo.Any() ? string.Join(", ", seasonalInfo) : "Year-round";
        }

        private string GetPriorityLevel(int priority)
        {
            return priority switch
            {
                >= 9 => "Critical",
                >= 7 => "High",
                >= 5 => "Medium",
                _ => "Low"
            };
        }

        private string ExtractLocationInfo(string content)
        {
            var locations = new[] { "tromsø", "bergen", "kirkenes", "geiranger", "trondheim", "honningsvåg", "arctic circle" };
            var foundLocations = locations.Where(location => 
                content.ToLower().Contains(location)).ToList();

            return foundLocations.Any() ? string.Join(", ", foundLocations) : "General";
        }

        private List<string> ExtractViewingTimes(string content)
        {
            var times = new List<string>();
            
            if (content.ToLower().Contains("evening") || content.ToLower().Contains("6 pm"))
                times.Add("Evening (6 PM onwards)");
            
            if (content.ToLower().Contains("night") || content.ToLower().Contains("midnight"))
                times.Add("Night time (10 PM - 2 AM)");
            
            if (content.ToLower().Contains("early morning"))
                times.Add("Early morning (before sunrise)");

            return times.Any() ? times : new List<string> { "Dark hours (6 PM - 6 AM)" };
        }
    }

    // Data models for maritime search
    public class MaritimeSearchDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string VesselType { get; set; } = string.Empty;
        public string RouteInfo { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public DateTime LastUpdated { get; set; }
    }

    public class MaritimeSearchResponse
    {
        public long TotalCount { get; set; }
        public List<MaritimeSearchResult> Results { get; set; } = new();
        public Dictionary<string, List<FacetResult>> Facets { get; set; } = new();
    }

    public class FacetResult
    {
        public string Value { get; set; } = string.Empty;
        public long Count { get; set; }
    }

    public class MaritimeSuggestion
    {
        public string Text { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class VesselInformation
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RouteInfo { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public string[] Features { get; set; } = Array.Empty<string>();
    }

    public class RouteGuidance
    {
        public string RouteName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RouteDetails { get; set; } = string.Empty;
        public string SeasonalInfo { get; set; } = string.Empty;
        public string[] ImportantNotes { get; set; } = Array.Empty<string>();
        public DateTime LastUpdated { get; set; }
    }

    public class SafetyProcedure
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ApplicableVessels { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public string[] RelatedProcedures { get; set; } = Array.Empty<string>();
    }

    public class NorthernLightsInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<string> BestViewingTimes { get; set; } = new();
        public string[] ViewingTips { get; set; } = Array.Empty<string>();
        public string RouteSpecific { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}