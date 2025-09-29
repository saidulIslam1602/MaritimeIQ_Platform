namespace MaritimeIQ.Platform.Models.ApiManagement
{
    /// <summary>
    /// Subscription tier definition with pricing and features
    /// </summary>
    public class SubscriptionTier
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerMonth { get; set; }
        public string[] Features { get; set; } = Array.Empty<string>();
        public Dictionary<string, string> RateLimits { get; set; } = new();
        public string SlaLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// API key request from user
    /// </summary>
    public class ApiKeyRequest
    {
        public string TierName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }

    /// <summary>
    /// API key response with credentials and configuration
    /// </summary>
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

    /// <summary>
    /// API usage analytics
    /// </summary>
    public class UsageAnalytics
    {
        public DatePeriod Period { get; set; } = new();
        public long TotalApiCalls { get; set; }
        public int UniqueUsers { get; set; }
        public List<ApiUsage> TopApis { get; set; } = new();
        public List<GeographicUsage> GeographicDistribution { get; set; } = new();
        public List<TierUsage> SubscriptionTierUsage { get; set; } = new();
    }

    /// <summary>
    /// Date period for analytics
    /// </summary>
    public class DatePeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Usage statistics for individual API
    /// </summary>
    public class ApiUsage
    {
        public string ApiName { get; set; } = string.Empty;
        public long Calls { get; set; }
        public double Percentage { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// Geographic distribution of API usage
    /// </summary>
    public class GeographicUsage
    {
        public string Country { get; set; } = string.Empty;
        public double Percentage { get; set; }
        public long Calls { get; set; }
    }

    /// <summary>
    /// Usage by subscription tier
    /// </summary>
    public class TierUsage
    {
        public string TierName { get; set; } = string.Empty;
        public int Users { get; set; }
        public double CallsPercentage { get; set; }
    }

    // Additional API Management Models
    public class ApiSubscription
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string> ApiIds { get; set; } = new List<string>();
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string SecondaryKey { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
    }

    public class ApiProduct
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public decimal PricePerMonth { get; set; }
        public List<string> ApiIds { get; set; } = new List<string>();
        public List<string> Features { get; set; } = new List<string>();
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DeveloperAccount
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> ApiKeys { get; set; } = new List<string>();
        public DateTime? UpdatedAt { get; set; }
    }

    public class SubscriptionKey
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string KeyType { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}