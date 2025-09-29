using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Services;
using MaritimeIQ.Platform.Models.ApiManagement;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// API Management controller for managing API definitions, subscriptions, policies, and analytics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ApiManagementController : ControllerBase
    {
        private readonly IApiManagementService _apiManagementService;
        private readonly ILogger<ApiManagementController> _logger;

        public ApiManagementController(
            IApiManagementService apiManagementService,
            ILogger<ApiManagementController> logger)
        {
            _apiManagementService = apiManagementService;
            _logger = logger;
        }

        #region API Definitions

        /// <summary>
        /// Get all API definitions
        /// </summary>
        [HttpGet("definitions")]
        public async Task<IActionResult> GetApiDefinitions()
        {
            try
            {
                var definitions = await _apiManagementService.GetApiDefinitionsAsync();
                return Ok(definitions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API definitions");
                return StatusCode(500, new { error = "Failed to retrieve API definitions", details = ex.Message });
            }
        }

        /// <summary>
        /// Get API definition by ID
        /// </summary>
        [HttpGet("definitions/{apiId}")]
        public async Task<IActionResult> GetApiDefinition(string apiId)
        {
            try
            {
                var definition = await _apiManagementService.GetApiDefinitionByIdAsync(apiId);
                return Ok(definition);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API definition for ID: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to retrieve API definition", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new API definition
        /// </summary>
        [HttpPost("definitions")]
        public async Task<IActionResult> CreateApiDefinition([FromBody] ApiDefinition apiDefinition)
        {
            try
            {
                if (apiDefinition == null)
                {
                    return BadRequest(new { error = "API definition is required" });
                }

                var createdDefinition = await _apiManagementService.CreateApiDefinitionAsync(apiDefinition);
                return CreatedAtAction(nameof(GetApiDefinition), new { apiId = createdDefinition.Id }, createdDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API definition: {ApiName}", apiDefinition?.Name);
                return StatusCode(500, new { error = "Failed to create API definition", details = ex.Message });
            }
        }

        /// <summary>
        /// Update API definition
        /// </summary>
        [HttpPut("definitions/{apiId}")]
        public async Task<IActionResult> UpdateApiDefinition(string apiId, [FromBody] ApiDefinition apiDefinition)
        {
            try
            {
                if (apiDefinition == null)
                {
                    return BadRequest(new { error = "API definition is required" });
                }

                var updatedDefinition = await _apiManagementService.UpdateApiDefinitionAsync(apiId, apiDefinition);
                return Ok(updatedDefinition);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating API definition: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to update API definition", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete API definition
        /// </summary>
        [HttpDelete("definitions/{apiId}")]
        public async Task<IActionResult> DeleteApiDefinition(string apiId)
        {
            try
            {
                var success = await _apiManagementService.DeleteApiDefinitionAsync(apiId);
                if (success)
                {
                    return NoContent();
                }
                return NotFound(new { error = "API definition not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting API definition: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to delete API definition", details = ex.Message });
            }
        }

        #endregion

        #region API Policies

        /// <summary>
        /// Get policies for an API
        /// </summary>
        [HttpGet("definitions/{apiId}/policies")]
        public async Task<IActionResult> GetApiPolicies(string apiId)
        {
            try
            {
                var policies = await _apiManagementService.GetApiPoliciesAsync(apiId);
                return Ok(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policies for API: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to retrieve API policies", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new API policy
        /// </summary>
        [HttpPost("definitions/{apiId}/policies")]
        public async Task<IActionResult> CreateApiPolicy(string apiId, [FromBody] ApiPolicy policy)
        {
            try
            {
                if (policy == null)
                {
                    return BadRequest(new { error = "Policy is required" });
                }

                var createdPolicy = await _apiManagementService.CreateApiPolicyAsync(apiId, policy);
                return CreatedAtAction(nameof(GetApiPolicies), new { apiId }, createdPolicy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy for API: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to create API policy", details = ex.Message });
            }
        }

        /// <summary>
        /// Update API policy
        /// </summary>
        [HttpPut("definitions/{apiId}/policies/{policyId}")]
        public async Task<IActionResult> UpdateApiPolicy(string apiId, string policyId, [FromBody] ApiPolicy policy)
        {
            try
            {
                if (policy == null)
                {
                    return BadRequest(new { error = "Policy is required" });
                }

                var updatedPolicy = await _apiManagementService.UpdateApiPolicyAsync(apiId, policyId, policy);
                return Ok(updatedPolicy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy {PolicyId} for API: {ApiId}", policyId, apiId);
                return StatusCode(500, new { error = "Failed to update API policy", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete API policy
        /// </summary>
        [HttpDelete("definitions/{apiId}/policies/{policyId}")]
        public async Task<IActionResult> DeleteApiPolicy(string apiId, string policyId)
        {
            try
            {
                var success = await _apiManagementService.DeleteApiPolicyAsync(apiId, policyId);
                if (success)
                {
                    return NoContent();
                }
                return NotFound(new { error = "Policy not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy {PolicyId} for API: {ApiId}", policyId, apiId);
                return StatusCode(500, new { error = "Failed to delete API policy", details = ex.Message });
            }
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// Get all subscriptions
        /// </summary>
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions()
        {
            try
            {
                var subscriptions = await _apiManagementService.GetSubscriptionsAsync();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions");
                return StatusCode(500, new { error = "Failed to retrieve subscriptions", details = ex.Message });
            }
        }

        /// <summary>
        /// Get subscriptions by API
        /// </summary>
        [HttpGet("definitions/{apiId}/subscriptions")]
        public async Task<IActionResult> GetSubscriptionsByApi(string apiId)
        {
            try
            {
                var subscriptions = await _apiManagementService.GetSubscriptionsByApiAsync(apiId);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions for API: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to retrieve API subscriptions", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new subscription
        /// </summary>
        [HttpPost("subscriptions")]
        public async Task<IActionResult> CreateSubscription([FromBody] ApiSubscription subscription)
        {
            try
            {
                if (subscription == null)
                {
                    return BadRequest(new { error = "Subscription is required" });
                }

                var createdSubscription = await _apiManagementService.CreateSubscriptionAsync(subscription);
                return CreatedAtAction(nameof(GetSubscriptions), createdSubscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription: {SubscriptionName}", subscription?.Name);
                return StatusCode(500, new { error = "Failed to create subscription", details = ex.Message });
            }
        }

        /// <summary>
        /// Generate new subscription key
        /// </summary>
        [HttpPost("subscriptions/{subscriptionId}/keys")]
        public async Task<IActionResult> GenerateSubscriptionKey(string subscriptionId)
        {
            try
            {
                var key = await _apiManagementService.GenerateSubscriptionKeyAsync(subscriptionId);
                return Ok(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating key for subscription: {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { error = "Failed to generate subscription key", details = ex.Message });
            }
        }

        /// <summary>
        /// Regenerate subscription key
        /// </summary>
        [HttpPost("subscriptions/{subscriptionId}/keys/{keyType}/regenerate")]
        public async Task<IActionResult> RegenerateSubscriptionKey(string subscriptionId, string keyType)
        {
            try
            {
                var key = await _apiManagementService.RegenerateSubscriptionKeyAsync(subscriptionId, keyType);
                return Ok(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating {KeyType} key for subscription: {SubscriptionId}", keyType, subscriptionId);
                return StatusCode(500, new { error = "Failed to regenerate subscription key", details = ex.Message });
            }
        }

        #endregion

        #region Analytics and Usage

        /// <summary>
        /// Get API usage metrics
        /// </summary>
        [HttpGet("definitions/{apiId}/usage")]
        public async Task<IActionResult> GetApiUsage(string apiId, [FromQuery] string timeframe = "24h")
        {
            try
            {
                var usage = await _apiManagementService.GetApiUsageMetricsAsync(apiId, timeframe);
                return Ok(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage for API: {ApiId}, timeframe: {Timeframe}", apiId, timeframe);
                return StatusCode(500, new { error = "Failed to retrieve API usage", details = ex.Message });
            }
        }

        /// <summary>
        /// Get subscription usage metrics
        /// </summary>
        [HttpGet("subscriptions/{subscriptionId}/usage")]
        public async Task<IActionResult> GetSubscriptionUsage(string subscriptionId, [FromQuery] string timeframe = "24h")
        {
            try
            {
                var usage = await _apiManagementService.GetSubscriptionUsageMetricsAsync(subscriptionId, timeframe);
                return Ok(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage for subscription: {SubscriptionId}, timeframe: {Timeframe}", subscriptionId, timeframe);
                return StatusCode(500, new { error = "Failed to retrieve subscription usage", details = ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive API analytics
        /// </summary>
        [HttpGet("definitions/{apiId}/analytics")]
        public async Task<IActionResult> GetApiAnalytics(string apiId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                var analytics = await _apiManagementService.GetApiAnalyticsAsync(apiId, start, end);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics for API: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to retrieve API analytics", details = ex.Message });
            }
        }

        #endregion

        #region Products

        /// <summary>
        /// Get all API products
        /// </summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _apiManagementService.GetProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API products");
                return StatusCode(500, new { error = "Failed to retrieve API products", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new API product
        /// </summary>
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] ApiProduct product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest(new { error = "Product is required" });
                }

                var createdProduct = await _apiManagementService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetProducts), createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API product: {ProductName}", product?.Name);
                return StatusCode(500, new { error = "Failed to create API product", details = ex.Message });
            }
        }

        #endregion

        #region Developers

        /// <summary>
        /// Get all developer accounts
        /// </summary>
        [HttpGet("developers")]
        public async Task<IActionResult> GetDevelopers()
        {
            try
            {
                var developers = await _apiManagementService.GetDevelopersAsync();
                return Ok(developers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving developer accounts");
                return StatusCode(500, new { error = "Failed to retrieve developer accounts", details = ex.Message });
            }
        }

        /// <summary>
        /// Create new developer account
        /// </summary>
        [HttpPost("developers")]
        public async Task<IActionResult> CreateDeveloper([FromBody] DeveloperAccount developer)
        {
            try
            {
                if (developer == null)
                {
                    return BadRequest(new { error = "Developer account is required" });
                }

                var createdDeveloper = await _apiManagementService.CreateDeveloperAccountAsync(developer);
                return CreatedAtAction(nameof(GetDevelopers), createdDeveloper);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating developer account: {Email}", developer?.Email);
                return StatusCode(500, new { error = "Failed to create developer account", details = ex.Message });
            }
        }

        #endregion

        #region Validation and Testing

        /// <summary>
        /// Validate API definition
        /// </summary>
        [HttpPost("definitions/validate")]
        public async Task<IActionResult> ValidateApiDefinition([FromBody] ApiDefinition apiDefinition)
        {
            try
            {
                if (apiDefinition == null)
                {
                    return BadRequest(new { error = "API definition is required" });
                }

                var validationResult = await _apiManagementService.ValidateApiDefinitionAsync(apiDefinition);
                return Ok(validationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API definition");
                return StatusCode(500, new { error = "Failed to validate API definition", details = ex.Message });
            }
        }

        /// <summary>
        /// Test API endpoint
        /// </summary>
        [HttpPost("definitions/{apiId}/test")]
        public async Task<IActionResult> TestApiEndpoint(string apiId, [FromBody] ApiTestRequest testRequest)
        {
            try
            {
                if (testRequest == null || string.IsNullOrEmpty(testRequest.Endpoint))
                {
                    return BadRequest(new { error = "Test request with endpoint is required" });
                }

                var testResult = await _apiManagementService.TestApiEndpointAsync(apiId, testRequest.Endpoint, testRequest);
                return Ok(testResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing API endpoint for API: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to test API endpoint", details = ex.Message });
            }
        }

        #endregion

        #region Documentation

        /// <summary>
        /// Get API documentation
        /// </summary>
        [HttpGet("definitions/{apiId}/documentation")]
        public async Task<IActionResult> GetApiDocumentation(string apiId)
        {
            try
            {
                var documentation = await _apiManagementService.GetApiDocumentationAsync(apiId);
                return Ok(documentation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documentation for API: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to retrieve API documentation", details = ex.Message });
            }
        }

        /// <summary>
        /// Get OpenAPI schema
        /// </summary>
        [HttpGet("definitions/{apiId}/schema")]
        public async Task<IActionResult> GetOpenApiSchema(string apiId)
        {
            try
            {
                var schema = await _apiManagementService.GetOpenApiSchemaAsync(apiId);
                return Ok(schema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OpenAPI schema for API: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to retrieve OpenAPI schema", details = ex.Message });
            }
        }

        #endregion

        #region Dashboard

        /// <summary>
        /// Get API management dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var dashboardData = await _apiManagementService.GetDashboardDataAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API management dashboard");
                return StatusCode(500, new { error = "Failed to retrieve dashboard data", details = ex.Message });
            }
        }

        /// <summary>
        /// Get API overview
        /// </summary>
        [HttpGet("definitions/{apiId}/overview")]
        public async Task<IActionResult> GetApiOverview(string apiId)
        {
            try
            {
                var overview = await _apiManagementService.GetApiOverviewAsync(apiId);
                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overview for API: {ApiId}", apiId);
                return StatusCode(500, new { error = "Failed to retrieve API overview", details = ex.Message });
            }
        }

        #endregion
    }

}