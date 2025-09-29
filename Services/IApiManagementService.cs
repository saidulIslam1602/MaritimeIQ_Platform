using MaritimeIQ.Platform.Models.ApiManagement;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// API Management service interface for managing API definitions, subscriptions, and policies
    /// </summary>
    public interface IApiManagementService
    {
        // API Definition Management
        Task<IEnumerable<ApiDefinition>> GetApiDefinitionsAsync();
        Task<ApiDefinition> GetApiDefinitionByIdAsync(string apiId);
        Task<ApiDefinition> CreateApiDefinitionAsync(ApiDefinition apiDefinition);
        Task<ApiDefinition> UpdateApiDefinitionAsync(string apiId, ApiDefinition apiDefinition);
        Task<bool> DeleteApiDefinitionAsync(string apiId);

        // Policy Management
        Task<IEnumerable<ApiPolicy>> GetApiPoliciesAsync(string apiId);
        Task<ApiPolicy> GetApiPolicyByIdAsync(string apiId, string policyId);
        Task<ApiPolicy> CreateApiPolicyAsync(string apiId, ApiPolicy policy);
        Task<ApiPolicy> UpdateApiPolicyAsync(string apiId, string policyId, ApiPolicy policy);
        Task<bool> DeleteApiPolicyAsync(string apiId, string policyId);

        // Security Policy Management
        Task<IEnumerable<SecurityPolicy>> GetSecurityPoliciesAsync(string apiId);
        Task<SecurityPolicy> CreateSecurityPolicyAsync(string apiId, SecurityPolicy securityPolicy);
        Task<bool> UpdateSecurityPolicyAsync(string apiId, string policyId, SecurityPolicy securityPolicy);

        // Rate Limiting Management
        Task<IEnumerable<RateLimitPolicy>> GetRateLimitPoliciesAsync(string apiId);
        Task<RateLimitPolicy> CreateRateLimitPolicyAsync(string apiId, RateLimitPolicy rateLimitPolicy);
        Task<bool> UpdateRateLimitPolicyAsync(string apiId, string policyId, RateLimitPolicy rateLimitPolicy);

        // Subscription Management
        Task<IEnumerable<ApiSubscription>> GetSubscriptionsAsync();
        Task<IEnumerable<ApiSubscription>> GetSubscriptionsByApiAsync(string apiId);
        Task<ApiSubscription> GetSubscriptionByIdAsync(string subscriptionId);
        Task<ApiSubscription> CreateSubscriptionAsync(ApiSubscription subscription);
        Task<ApiSubscription> UpdateSubscriptionAsync(string subscriptionId, ApiSubscription subscription);
        Task<bool> DeleteSubscriptionAsync(string subscriptionId);

        // Subscription Key Management
        Task<SubscriptionKey> GenerateSubscriptionKeyAsync(string subscriptionId);
        Task<SubscriptionKey> RegenerateSubscriptionKeyAsync(string subscriptionId, string keyType);
        Task<bool> RevokeSubscriptionKeyAsync(string subscriptionId, string keyType);

        // Analytics and Usage
        Task<ApiUsageMetrics> GetApiUsageMetricsAsync(string apiId, string timeframe);
        Task<SubscriptionUsageMetrics> GetSubscriptionUsageMetricsAsync(string subscriptionId, string timeframe);
        Task<ApiAnalytics> GetApiAnalyticsAsync(string apiId, DateTime startDate, DateTime endDate);

        // Product Management
        Task<IEnumerable<ApiProduct>> GetProductsAsync();
        Task<ApiProduct> GetProductByIdAsync(string productId);
        Task<ApiProduct> CreateProductAsync(ApiProduct product);
        Task<ApiProduct> UpdateProductAsync(string productId, ApiProduct product);
        Task<bool> DeleteProductAsync(string productId);

        // User and Developer Management
        Task<IEnumerable<DeveloperAccount>> GetDevelopersAsync();
        Task<DeveloperAccount> GetDeveloperByIdAsync(string developerId);
        Task<DeveloperAccount> CreateDeveloperAccountAsync(DeveloperAccount developer);
        Task<DeveloperAccount> UpdateDeveloperAccountAsync(string developerId, DeveloperAccount developer);
        Task<bool> DeleteDeveloperAccountAsync(string developerId);

        // Validation and Testing
        Task<ApiValidationResult> ValidateApiDefinitionAsync(ApiDefinition apiDefinition);
        Task<ApiTestResult> TestApiEndpointAsync(string apiId, string endpoint, ApiTestRequest testRequest);

        // Documentation and Schema
        Task<ApiDocumentation> GetApiDocumentationAsync(string apiId);
        Task<ApiDocumentation> UpdateApiDocumentationAsync(string apiId, ApiDocumentation documentation);
        Task<OpenApiSchema> GetOpenApiSchemaAsync(string apiId);
        Task<OpenApiSchema> UpdateOpenApiSchemaAsync(string apiId, OpenApiSchema schema);

        // Dashboard and Overview
        Task<ApiManagementDashboard> GetDashboardDataAsync();
        Task<ApiOverview> GetApiOverviewAsync(string apiId);
    }
}