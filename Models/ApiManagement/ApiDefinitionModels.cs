namespace MaritimeIQ.Platform.Models.ApiManagement
{
    /// <summary>
    /// Main API management definition and configuration
    /// </summary>
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

    /// <summary>
    /// API group containing related APIs
    /// </summary>
    public class ApiGroup
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ApiDefinition> Apis { get; set; } = new();
    }

    /// <summary>
    /// Individual API definition
    /// </summary>
    public class ApiDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string[] Methods { get; set; } = Array.Empty<string>();
        public string Description { get; set; } = string.Empty;
        public bool RequiresSubscription { get; set; }
        public string RateLimit { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = Array.Empty<string>();
        
        // Enhanced properties for comprehensive API management
        public string Version { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> AuthenticationMethods { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public string Documentation { get; set; } = string.Empty;
        public string OpenApiSpec { get; set; } = string.Empty;
    }

    /// <summary>
    /// Security scheme configuration
    /// </summary>
    public class SecurityScheme
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string In { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public OAuthFlows? Flows { get; set; }
    }

    /// <summary>
    /// OAuth flow configuration
    /// </summary>
    public class OAuthFlows
    {
        public OAuthFlow? AuthorizationCode { get; set; }
    }

    /// <summary>
    /// Individual OAuth flow definition
    /// </summary>
    public class OAuthFlow
    {
        public string AuthorizationUrl { get; set; } = string.Empty;
        public string TokenUrl { get; set; } = string.Empty;
        public Dictionary<string, string> Scopes { get; set; } = new();
    }

    /// <summary>
    /// API policy configuration
    /// </summary>
    public class ApiPolicy
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Configuration { get; set; } = new();
        
        // Enhanced properties for comprehensive policy management
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Priority { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public List<string> AppliedToApis { get; set; } = new List<string>();
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}