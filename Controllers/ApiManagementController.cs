using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiManagementController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiManagementController> _logger;

        public ApiManagementController(
            IConfiguration configuration,
            ILogger<ApiManagementController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("api-definitions")]
        public IActionResult GetApiDefinitions()
        {
            var apiDefinitions = new ApiManagementDefinition
            {
                BaseUrl = _configuration["ApiManagement:BaseUrl"] ?? "https://havila-maritime-apim.azure-api.net",
                Version = "v1",
                Title = "Havila Kystruten Maritime Platform API",
                Description = "Comprehensive API for maritime operations, vessel tracking, and passenger services",
                ApiGroups = new List<ApiGroup>
                {
                    new ApiGroup
                    {
                        Name = "Vessel Operations",
                        Description = "APIs for vessel tracking, fleet management, and operational data",
                        Apis = new List<ApiDefinition>
                        {
                            new ApiDefinition
                            {
                                Name = "Vessel Tracking",
                                Path = "/api/vessel",
                                Methods = new[] { "GET", "POST", "PUT" },
                                Description = "Real-time vessel position tracking and fleet status",
                                RequiresSubscription = true,
                                RateLimit = "100 calls per minute",
                                Scopes = new[] { "vessel:read", "vessel:write" }
                            },
                            new ApiDefinition
                            {
                                Name = "Fleet Analytics",
                                Path = "/api/fleet-analytics",
                                Methods = new[] { "GET" },
                                Description = "Comprehensive fleet performance and analytics data",
                                RequiresSubscription = true,
                                RateLimit = "50 calls per minute",
                                Scopes = new[] { "analytics:read" }
                            },
                            new ApiDefinition
                            {
                                Name = "Route Optimization",
                                Path = "/api/route",
                                Methods = new[] { "GET", "POST" },
                                Description = "AI-powered route planning and optimization",
                                RequiresSubscription = true,
                                RateLimit = "20 calls per minute",
                                Scopes = new[] { "route:read", "route:optimize" }
                            }
                        }
                    },
                    new ApiGroup
                    {
                        Name = "Maritime Intelligence",
                        Description = "AI-powered document processing and intelligent analytics",
                        Apis = new List<ApiDefinition>
                        {
                            new ApiDefinition
                            {
                                Name = "Document Analysis",
                                Path = "/api/maritime-intelligence",
                                Methods = new[] { "POST" },
                                Description = "AI-powered maritime document processing and analysis",
                                RequiresSubscription = true,
                                RateLimit = "30 calls per minute",
                                Scopes = new[] { "intelligence:analyze" }
                            },
                            new ApiDefinition
                            {
                                Name = "Maritime Vision",
                                Path = "/api/maritime-vision",
                                Methods = new[] { "POST" },
                                Description = "Computer vision analysis for vessels and maritime conditions",
                                RequiresSubscription = true,
                                RateLimit = "25 calls per minute",
                                Scopes = new[] { "vision:analyze" }
                            },
                            new ApiDefinition
                            {
                                Name = "Maritime Search",
                                Path = "/api/maritime-search",
                                Methods = new[] { "GET", "POST" },
                                Description = "Intelligent search across maritime knowledge base",
                                RequiresSubscription = false,
                                RateLimit = "200 calls per minute",
                                Scopes = new[] { "search:read" }
                            }
                        }
                    },
                    new ApiGroup
                    {
                        Name = "Passenger Services",
                        Description = "APIs for passenger information, bookings, and service requests",
                        Apis = new List<ApiDefinition>
                        {
                            new ApiDefinition
                            {
                                Name = "Maritime Chat",
                                Path = "/api/maritime-chat",
                                Methods = new[] { "POST" },
                                Description = "AI-powered customer service chat for maritime inquiries",
                                RequiresSubscription = false,
                                RateLimit = "50 calls per minute",
                                Scopes = new[] { "chat:use" }
                            },
                            new ApiDefinition
                            {
                                Name = "Northern Lights Information",
                                Path = "/api/maritime-search/northern-lights-info",
                                Methods = new[] { "GET" },
                                Description = "Real-time Northern Lights viewing information and forecasts",
                                RequiresSubscription = false,
                                RateLimit = "100 calls per minute",
                                Scopes = new[] { "info:read" }
                            }
                        }
                    },
                    new ApiGroup
                    {
                        Name = "IoT and Monitoring",
                        Description = "APIs for IoT sensor data and real-time monitoring",
                        Apis = new List<ApiDefinition>
                        {
                            new ApiDefinition
                            {
                                Name = "IoT Data Ingestion",
                                Path = "/api/iot",
                                Methods = new[] { "POST" },
                                Description = "IoT sensor data collection and processing",
                                RequiresSubscription = true,
                                RateLimit = "1000 calls per minute",
                                Scopes = new[] { "iot:write" }
                            },
                            new ApiDefinition
                            {
                                Name = "Safety Monitoring",
                                Path = "/api/safety",
                                Methods = new[] { "GET", "POST" },
                                Description = "Safety equipment monitoring and emergency procedures",
                                RequiresSubscription = true,
                                RateLimit = "100 calls per minute",
                                Scopes = new[] { "safety:read", "safety:monitor" }
                            },
                            new ApiDefinition
                            {
                                Name = "Environmental Monitoring",
                                Path = "/api/vessel-data-ingestion/environmental",
                                Methods = new[] { "GET", "POST" },
                                Description = "Environmental compliance monitoring and reporting",
                                RequiresSubscription = true,
                                RateLimit = "200 calls per minute",
                                Scopes = new[] { "environmental:read", "environmental:monitor" }
                            }
                        }
                    }
                },
                SecuritySchemes = new Dictionary<string, SecurityScheme>
                {
                    ["ApiKey"] = new SecurityScheme
                    {
                        Type = "apiKey",
                        Name = "Ocp-Apim-Subscription-Key",
                        In = "header",
                        Description = "API Management subscription key"
                    },
                    ["OAuth2"] = new SecurityScheme
                    {
                        Type = "oauth2",
                        Description = "Azure AD OAuth2 authentication",
                        Flows = new OAuthFlows
                        {
                            AuthorizationCode = new OAuthFlow
                            {
                                AuthorizationUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize",
                                TokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token",
                                Scopes = new Dictionary<string, string>
                                {
                                    ["vessel:read"] = "Read vessel information",
                                    ["vessel:write"] = "Modify vessel data",
                                    ["analytics:read"] = "Access analytics data",
                                    ["route:read"] = "Read route information",
                                    ["route:optimize"] = "Perform route optimization",
                                    ["intelligence:analyze"] = "Use AI analysis services",
                                    ["vision:analyze"] = "Use computer vision services",
                                    ["search:read"] = "Search maritime knowledge base",
                                    ["chat:use"] = "Use maritime chat service",
                                    ["info:read"] = "Access general information",
                                    ["iot:write"] = "Submit IoT sensor data",
                                    ["safety:read"] = "Read safety information",
                                    ["safety:monitor"] = "Access safety monitoring",
                                    ["environmental:read"] = "Read environmental data",
                                    ["environmental:monitor"] = "Access environmental monitoring"
                                }
                            }
                        }
                    }
                },
                Policies = new List<ApiPolicy>
                {
                    new ApiPolicy
                    {
                        Name = "Rate Limiting",
                        Type = "rate-limit",
                        Configuration = new Dictionary<string, object>
                        {
                            ["calls"] = 1000,
                            ["renewal-period"] = 3600,
                            ["counter-key"] = "@(context.Subscription?.Key ?? context.Request.IpAddress)"
                        }
                    },
                    new ApiPolicy
                    {
                        Name = "IP Filtering",
                        Type = "ip-filter",
                        Configuration = new Dictionary<string, object>
                        {
                            ["action"] = "allow",
                            ["addresses"] = new[] { "0.0.0.0/0" } // Allow all IPs - configure as needed
                        }
                    },
                    new ApiPolicy
                    {
                        Name = "CORS",
                        Type = "cors",
                        Configuration = new Dictionary<string, object>
                        {
                            ["allowed-origins"] = new[] { "*" },
                            ["allowed-methods"] = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" },
                            ["allowed-headers"] = new[] { "*" }
                        }
                    },
                    new ApiPolicy
                    {
                        Name = "Request Validation",
                        Type = "validate-content",
                        Configuration = new Dictionary<string, object>
                        {
                            ["validate-as-json"] = true,
                            ["max-size"] = 10485760 // 10MB
                        }
                    },
                    new ApiPolicy
                    {
                        Name = "Response Transformation",
                        Type = "set-header",
                        Configuration = new Dictionary<string, object>
                        {
                            ["name"] = "X-Powered-By",
                            ["value"] = "Havila-Maritime-Platform",
                            ["action"] = "override"
                        }
                    }
                }
            };

            return Ok(apiDefinitions);
        }

        [HttpGet("subscription-tiers")]
        public IActionResult GetSubscriptionTiers()
        {
            var subscriptionTiers = new List<SubscriptionTier>
            {
                new SubscriptionTier
                {
                    Name = "Basic",
                    Description = "Basic access to maritime information and search",
                    PricePerMonth = 0,
                    Features = new[]
                    {
                        "Maritime Search API",
                        "Northern Lights Information",
                        "Basic Chat Support",
                        "Public Vessel Information"
                    },
                    RateLimits = new Dictionary<string, string>
                    {
                        ["search"] = "100 calls/hour",
                        ["chat"] = "20 calls/hour",
                        ["info"] = "50 calls/hour"
                    },
                    SlaLevel = "Best Effort"
                },
                new SubscriptionTier
                {
                    Name = "Professional",
                    Description = "Advanced maritime operations and analytics access",
                    PricePerMonth = 299,
                    Features = new[]
                    {
                        "All Basic features",
                        "Vessel Tracking API",
                        "Fleet Analytics",
                        "Route Information",
                        "Document Analysis (100/month)",
                        "Computer Vision Analysis (50/month)",
                        "Priority Support"
                    },
                    RateLimits = new Dictionary<string, string>
                    {
                        ["vessel"] = "500 calls/hour",
                        ["analytics"] = "200 calls/hour",
                        ["intelligence"] = "100 calls/hour",
                        ["vision"] = "50 calls/hour"
                    },
                    SlaLevel = "99.5% uptime"
                },
                new SubscriptionTier
                {
                    Name = "Enterprise",
                    Description = "Full maritime platform access with unlimited usage",
                    PricePerMonth = 1999,
                    Features = new[]
                    {
                        "All Professional features",
                        "Unlimited API calls",
                        "Route Optimization",
                        "IoT Data Ingestion",
                        "Custom AI Models",
                        "Real-time Monitoring",
                        "24/7 Premium Support",
                        "Custom Integrations",
                        "Dedicated Account Manager"
                    },
                    RateLimits = new Dictionary<string, string>
                    {
                        ["all"] = "Unlimited"
                    },
                    SlaLevel = "99.9% uptime with 4-hour response time"
                },
                new SubscriptionTier
                {
                    Name = "Partner",
                    Description = "Special pricing for maritime industry partners",
                    PricePerMonth = 999,
                    Features = new[]
                    {
                        "All Enterprise features",
                        "White-label options",
                        "Revenue sharing programs",
                        "Co-marketing opportunities",
                        "Priority feature requests",
                        "Beta access to new features"
                    },
                    RateLimits = new Dictionary<string, string>
                    {
                        ["all"] = "Unlimited with burst capacity"
                    },
                    SlaLevel = "99.95% uptime with 2-hour response time"
                }
            };

            return Ok(subscriptionTiers);
        }

        [HttpGet("health-status")]
        public IActionResult GetApiHealthStatus()
        {
            var healthStatus = new ApiHealthStatus
            {
                OverallStatus = "Healthy",
                LastChecked = DateTime.UtcNow,
                Services = new List<ServiceHealth>
                {
                    new ServiceHealth
                    {
                        ServiceName = "Vessel Tracking API",
                        Status = "Healthy",
                        ResponseTime = TimeSpan.FromMilliseconds(45),
                        LastChecked = DateTime.UtcNow.AddMinutes(-1),
                        Dependencies = new[] { "Azure SQL", "Redis Cache" }
                    },
                    new ServiceHealth
                    {
                        ServiceName = "Maritime Intelligence API",
                        Status = "Healthy",
                        ResponseTime = TimeSpan.FromMilliseconds(120),
                        LastChecked = DateTime.UtcNow.AddMinutes(-2),
                        Dependencies = new[] { "Form Recognizer", "Text Analytics", "Azure OpenAI" }
                    },
                    new ServiceHealth
                    {
                        ServiceName = "Maritime Vision API",
                        Status = "Healthy",
                        ResponseTime = TimeSpan.FromMilliseconds(200),
                        LastChecked = DateTime.UtcNow.AddMinutes(-1),
                        Dependencies = new[] { "Computer Vision API", "Blob Storage" }
                    },
                    new ServiceHealth
                    {
                        ServiceName = "Maritime Search API",
                        Status = "Healthy",
                        ResponseTime = TimeSpan.FromMilliseconds(30),
                        LastChecked = DateTime.UtcNow.AddMinutes(-1),
                        Dependencies = new[] { "Cognitive Search", "Search Index" }
                    },
                    new ServiceHealth
                    {
                        ServiceName = "Fleet Analytics API",
                        Status = "Healthy",
                        ResponseTime = TimeSpan.FromMilliseconds(85),
                        LastChecked = DateTime.UtcNow.AddMinutes(-3),
                        Dependencies = new[] { "Azure SQL", "Power BI", "Application Insights" }
                    },
                    new ServiceHealth
                    {
                        ServiceName = "Route Optimization API",
                        Status = "Warning",
                        ResponseTime = TimeSpan.FromMilliseconds(350),
                        LastChecked = DateTime.UtcNow.AddMinutes(-1),
                        Dependencies = new[] { "Weather API", "AI Route Optimizer", "Azure Functions" },
                        Issues = new[] { "Elevated response times due to complex route calculations" }
                    }
                },
                Metrics = new HealthMetrics
                {
                    TotalRequests24h = 145632,
                    SuccessRate = 99.7,
                    AverageResponseTime = TimeSpan.FromMilliseconds(95),
                    ErrorRate = 0.3,
                    TopErrors = new[]
                    {
                        "Rate limit exceeded (0.2%)",
                        "Authentication failed (0.1%)"
                    }
                }
            };

            return Ok(healthStatus);
        }

        [HttpGet("usage-analytics")]
        public IActionResult GetUsageAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var usageAnalytics = new UsageAnalytics
            {
                Period = new DatePeriod { StartDate = start, EndDate = end },
                TotalApiCalls = 2_847_592,
                UniqueUsers = 1_247,
                TopApis = new List<ApiUsage>
                {
                    new ApiUsage
                    {
                        ApiName = "Maritime Search",
                        Calls = 1_234_567,
                        Percentage = 43.3,
                        AverageResponseTime = TimeSpan.FromMilliseconds(25),
                        SuccessRate = 99.8
                    },
                    new ApiUsage
                    {
                        ApiName = "Vessel Tracking",
                        Calls = 876_543,
                        Percentage = 30.8,
                        AverageResponseTime = TimeSpan.FromMilliseconds(45),
                        SuccessRate = 99.9
                    },
                    new ApiUsage
                    {
                        ApiName = "Fleet Analytics",
                        Calls = 345_678,
                        Percentage = 12.1,
                        AverageResponseTime = TimeSpan.FromMilliseconds(85),
                        SuccessRate = 99.5
                    },
                    new ApiUsage
                    {
                        ApiName = "Maritime Intelligence",
                        Calls = 234_567,
                        Percentage = 8.2,
                        AverageResponseTime = TimeSpan.FromMilliseconds(150),
                        SuccessRate = 98.9
                    },
                    new ApiUsage
                    {
                        ApiName = "Maritime Vision",
                        Calls = 156_237,
                        Percentage = 5.5,
                        AverageResponseTime = TimeSpan.FromMilliseconds(220),
                        SuccessRate = 99.1
                    }
                },
                GeographicDistribution = new List<GeographicUsage>
                {
                    new GeographicUsage { Country = "Norway", Percentage = 35.2, Calls = 1_002_876 },
                    new GeographicUsage { Country = "Denmark", Percentage = 18.7, Calls = 532_539 },
                    new GeographicUsage { Country = "Sweden", Percentage = 15.3, Calls = 435_682 },
                    new GeographicUsage { Country = "Germany", Percentage = 10.8, Calls = 307_540 },
                    new GeographicUsage { Country = "United Kingdom", Percentage = 8.9, Calls = 253_436 },
                    new GeographicUsage { Country = "Netherlands", Percentage = 6.2, Calls = 176550 },
                    new GeographicUsage { Country = "Other", Percentage = 4.9, Calls = 139_469 }
                },
                SubscriptionTierUsage = new List<TierUsage>
                {
                    new TierUsage { TierName = "Basic", Users = 892, CallsPercentage = 25.4 },
                    new TierUsage { TierName = "Professional", Users = 287, CallsPercentage = 45.8 },
                    new TierUsage { TierName = "Enterprise", Users = 52, CallsPercentage = 23.1 },
                    new TierUsage { TierName = "Partner", Users = 16, CallsPercentage = 5.7 }
                }
            };

            return Ok(usageAnalytics);
        }

        [HttpPost("generate-api-key")]
        public IActionResult GenerateApiKey([FromBody] ApiKeyRequest request)
        {
            try
            {
                // Simulate API key generation
                var apiKey = GenerateSecureApiKey();
                
                var response = new ApiKeyResponse
                {
                    ApiKey = apiKey,
                    SubscriptionId = Guid.NewGuid().ToString(),
                    TierName = request.TierName,
                    ExpirationDate = DateTime.UtcNow.AddYears(1),
                    RateLimits = GetRateLimitsForTier(request.TierName),
                    Scopes = GetScopesForTier(request.TierName),
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                _logger.LogInformation($"Generated API key for tier: {request.TierName}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating API key");
                return StatusCode(500, "Error generating API key");
            }
        }

        [HttpGet("swagger-config")]
        public IActionResult GetSwaggerConfiguration()
        {
            var swaggerConfig = new SwaggerConfiguration
            {
                OpenApiVersion = "3.0.1",
                Info = new SwaggerInfo
                {
                    Title = "Havila Kystruten Maritime Platform API",
                    Version = "v1",
                    Description = @"
                        Comprehensive Maritime Platform API for Havila Kystruten's coastal operations.
                        
                        This API provides access to:
                        - Real-time vessel tracking and fleet management
                        - AI-powered maritime intelligence and document processing
                        - Computer vision analysis for vessels and environmental conditions
                        - Intelligent search across maritime knowledge base
                        - Route optimization and planning
                        - IoT sensor data ingestion and monitoring
                        - Northern Lights information and passenger services
                        
                        For support: api-support@havila.no
                    ",
                    Contact = new SwaggerContact
                    {
                        Name = "Havila Maritime API Support",
                        Email = "api-support@havila.no",
                        Url = "https://havila.no/support"
                    },
                    License = new SwaggerLicense
                    {
                        Name = "Havila API License",
                        Url = "https://havila.no/api-license"
                    }
                },
                Servers = new List<SwaggerServer>
                {
                    new SwaggerServer
                    {
                        Url = "https://havila-maritime-apim.azure-api.net/v1",
                        Description = "Production API Management Gateway"
                    },
                    new SwaggerServer
                    {
                        Url = "https://havila-maritime-apim-staging.azure-api.net/v1",
                        Description = "Staging Environment"
                    }
                },
                Components = new SwaggerComponents
                {
                    SecuritySchemes = new Dictionary<string, SwaggerSecurityScheme>
                    {
                        ["ApiKeyAuth"] = new SwaggerSecurityScheme
                        {
                            Type = "apiKey",
                            In = "header",
                            Name = "Ocp-Apim-Subscription-Key",
                            Description = "API Management subscription key required for authenticated endpoints"
                        },
                        ["BearerAuth"] = new SwaggerSecurityScheme
                        {
                            Type = "http",
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            Description = "Azure AD JWT token for OAuth2 authentication"
                        }
                    }
                },
                Tags = new List<SwaggerTag>
                {
                    new SwaggerTag { Name = "Vessel Operations", Description = "Vessel tracking and fleet management" },
                    new SwaggerTag { Name = "Maritime Intelligence", Description = "AI-powered document and data analysis" },
                    new SwaggerTag { Name = "Maritime Vision", Description = "Computer vision for maritime operations" },
                    new SwaggerTag { Name = "Maritime Search", Description = "Intelligent search and knowledge base" },
                    new SwaggerTag { Name = "Passenger Services", Description = "Customer service and information APIs" },
                    new SwaggerTag { Name = "IoT & Monitoring", Description = "Sensor data and real-time monitoring" }
                }
            };

            return Ok(swaggerConfig);
        }

        // Private helper methods
        private string GenerateSecureApiKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var keyLength = 32;
            
            return new string(Enumerable.Repeat(chars, keyLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private Dictionary<string, string> GetRateLimitsForTier(string tierName)
        {
            return tierName.ToLower() switch
            {
                "basic" => new Dictionary<string, string>
                {
                    ["search"] = "100 calls/hour",
                    ["chat"] = "20 calls/hour",
                    ["info"] = "50 calls/hour"
                },
                "professional" => new Dictionary<string, string>
                {
                    ["vessel"] = "500 calls/hour",
                    ["analytics"] = "200 calls/hour",
                    ["intelligence"] = "100 calls/hour",
                    ["vision"] = "50 calls/hour"
                },
                "enterprise" => new Dictionary<string, string>
                {
                    ["all"] = "Unlimited"
                },
                "partner" => new Dictionary<string, string>
                {
                    ["all"] = "Unlimited with burst capacity"
                },
                _ => new Dictionary<string, string>()
            };
        }

        private List<string> GetScopesForTier(string tierName)
        {
            return tierName.ToLower() switch
            {
                "basic" => new List<string> { "search:read", "info:read", "chat:use" },
                "professional" => new List<string> 
                { 
                    "search:read", "info:read", "chat:use", "vessel:read", 
                    "analytics:read", "intelligence:analyze", "vision:analyze" 
                },
                "enterprise" => new List<string> 
                { 
                    "vessel:read", "vessel:write", "analytics:read", "route:read", 
                    "route:optimize", "intelligence:analyze", "vision:analyze", 
                    "search:read", "chat:use", "info:read", "iot:write", 
                    "safety:read", "safety:monitor", "environmental:read", "environmental:monitor" 
                },
                "partner" => new List<string> 
                { 
                    "vessel:read", "vessel:write", "analytics:read", "route:read", 
                    "route:optimize", "intelligence:analyze", "vision:analyze", 
                    "search:read", "chat:use", "info:read", "iot:write", 
                    "safety:read", "safety:monitor", "environmental:read", "environmental:monitor",
                    "admin:manage", "partner:access" 
                },
                _ => new List<string>()
            };
        }
    }

    // Data models for API Management
    public class ApiManagementDefinition
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ApiGroup> ApiGroups { get; set; } = new();
        public Dictionary<string, SecurityScheme> SecuritySchemes { get; set; } = new();
        public List<ApiPolicy> Policies { get; set; } = new();
    }

    public class ApiGroup
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ApiDefinition> Apis { get; set; } = new();
    }

    public class ApiDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string[] Methods { get; set; } = Array.Empty<string>();
        public string Description { get; set; } = string.Empty;
        public bool RequiresSubscription { get; set; }
        public string RateLimit { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = Array.Empty<string>();
    }

    public class SecurityScheme
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string In { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public OAuthFlows? Flows { get; set; }
    }

    public class OAuthFlows
    {
        public OAuthFlow? AuthorizationCode { get; set; }
    }

    public class OAuthFlow
    {
        public string AuthorizationUrl { get; set; } = string.Empty;
        public string TokenUrl { get; set; } = string.Empty;
        public Dictionary<string, string> Scopes { get; set; } = new();
    }

    public class ApiPolicy
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class SubscriptionTier
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerMonth { get; set; }
        public string[] Features { get; set; } = Array.Empty<string>();
        public Dictionary<string, string> RateLimits { get; set; } = new();
        public string SlaLevel { get; set; } = string.Empty;
    }

    public class ApiHealthStatus
    {
        public string OverallStatus { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public List<ServiceHealth> Services { get; set; } = new();
        public HealthMetrics Metrics { get; set; } = new();
    }

    public class ServiceHealth
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
        public string[] Dependencies { get; set; } = Array.Empty<string>();
        public string[] Issues { get; set; } = Array.Empty<string>();
    }

    public class HealthMetrics
    {
        public long TotalRequests24h { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public double ErrorRate { get; set; }
        public string[] TopErrors { get; set; } = Array.Empty<string>();
    }

    public class UsageAnalytics
    {
        public DatePeriod Period { get; set; } = new();
        public long TotalApiCalls { get; set; }
        public int UniqueUsers { get; set; }
        public List<ApiUsage> TopApis { get; set; } = new();
        public List<GeographicUsage> GeographicDistribution { get; set; } = new();
        public List<TierUsage> SubscriptionTierUsage { get; set; } = new();
    }

    public class DatePeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ApiUsage
    {
        public string ApiName { get; set; } = string.Empty;
        public long Calls { get; set; }
        public double Percentage { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public double SuccessRate { get; set; }
    }

    public class GeographicUsage
    {
        public string Country { get; set; } = string.Empty;
        public double Percentage { get; set; }
        public long Calls { get; set; }
    }

    public class TierUsage
    {
        public string TierName { get; set; } = string.Empty;
        public int Users { get; set; }
        public double CallsPercentage { get; set; }
    }

    public class ApiKeyRequest
    {
        public string TierName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }

    public class ApiKeyResponse
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public Dictionary<string, string> RateLimits { get; set; } = new();
        public List<string> Scopes { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SwaggerConfiguration
    {
        public string OpenApiVersion { get; set; } = string.Empty;
        public SwaggerInfo Info { get; set; } = new();
        public List<SwaggerServer> Servers { get; set; } = new();
        public SwaggerComponents Components { get; set; } = new();
        public List<SwaggerTag> Tags { get; set; } = new();
    }

    public class SwaggerInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SwaggerContact Contact { get; set; } = new();
        public SwaggerLicense License { get; set; } = new();
    }

    public class SwaggerContact
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class SwaggerLicense
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class SwaggerServer
    {
        public string Url { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SwaggerComponents
    {
        public Dictionary<string, SwaggerSecurityScheme> SecuritySchemes { get; set; } = new();
    }

    public class SwaggerSecurityScheme
    {
        public string Type { get; set; } = string.Empty;
        public string In { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Scheme { get; set; } = string.Empty;
        public string BearerFormat { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SwaggerTag
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}