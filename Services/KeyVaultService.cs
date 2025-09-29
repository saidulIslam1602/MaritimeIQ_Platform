using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MaritimeIQ.Platform.Services
{
    public class KeyVaultConfiguration
    {
        public string VaultUri { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public bool UseManagedIdentity { get; set; } = true;
    }

    public interface IKeyVaultService
    {
        Task<string> GetSecretAsync(string secretName);
        Task<T> GetSecretAsync<T>(string secretName) where T : class;
        Task SetSecretAsync(string secretName, string secretValue);
        Task<bool> SecretExistsAsync(string secretName);
    }

    public class KeyVaultService : IKeyVaultService
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<KeyVaultService> _logger;

        public KeyVaultService(IOptions<KeyVaultConfiguration> options, ILogger<KeyVaultService> logger)
        {
            _logger = logger;
            var config = options.Value;

            if (string.IsNullOrWhiteSpace(config.VaultUri))
            {
                throw new ArgumentException("Key Vault URI is not configured. Set KeyVault:VaultUri in configuration or environment.");
            }

            var credential = CreateCredential(config);
            _secretClient = new SecretClient(new Uri(config.VaultUri), credential);
        }

        private static TokenCredential CreateCredential(KeyVaultConfiguration config)
        {
            if (!config.UseManagedIdentity &&
                !string.IsNullOrWhiteSpace(config.ClientId) &&
                !string.IsNullOrWhiteSpace(config.ClientSecret) &&
                !string.IsNullOrWhiteSpace(config.TenantId))
            {
                return new ClientSecretCredential(
                    config.TenantId,
                    config.ClientId,
                    config.ClientSecret);
            }

#pragma warning disable AZC0102 // Among the Microsoft-recommended credentials
            return new DefaultAzureCredential();
#pragma warning restore AZC0102
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                _logger.LogInformation("Retrieved secret {SecretName} from Key Vault", secretName);
                return secret.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret {SecretName} from Key Vault", secretName);
                throw;
            }
        }

        public async Task<T> GetSecretAsync<T>(string secretName) where T : class
        {
            try
            {
                var secretValue = await GetSecretAsync(secretName);
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(secretValue);
                return result!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing secret {SecretName} from Key Vault", secretName);
                throw;
            }
        }

        public async Task SetSecretAsync(string secretName, string secretValue)
        {
            try
            {
                await _secretClient.SetSecretAsync(secretName, secretValue);
                _logger.LogInformation("Set secret {SecretName} in Key Vault", secretName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting secret {SecretName} in Key Vault", secretName);
                throw;
            }
        }

        public async Task<bool> SecretExistsAsync(string secretName)
        {
            try
            {
                await _secretClient.GetSecretAsync(secretName);
                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if secret {SecretName} exists in Key Vault", secretName);
                throw;
            }
        }
    }

    // Extension methods for dependency injection and configuration
    public static class KeyVaultExtensions
    {
        public static IServiceCollection AddHavilaKeyVault(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KeyVaultConfiguration>(configuration.GetSection("KeyVault"));
            services.AddSingleton<IKeyVaultService, KeyVaultService>();

            return services;
        }

        public static IConfigurationBuilder AddHavilaKeyVault(this IConfigurationBuilder builder, KeyVaultConfiguration config)
        {
            var credential = BuildCredential(config);

            return builder.AddAzureKeyVault(
                new Uri(config.VaultUri),
                credential,
                new KeyVaultSecretManager());
        }

        private static TokenCredential BuildCredential(KeyVaultConfiguration config)
        {
            if (!config.UseManagedIdentity &&
                !string.IsNullOrWhiteSpace(config.ClientId) &&
                !string.IsNullOrWhiteSpace(config.ClientSecret) &&
                !string.IsNullOrWhiteSpace(config.TenantId))
            {
                return new ClientSecretCredential(
                    config.TenantId,
                    config.ClientId,
                    config.ClientSecret);
            }

#pragma warning disable AZC0102 // Among the Microsoft-recommended credentials
            return new DefaultAzureCredential();
#pragma warning restore AZC0102
        }
    }

    // Havila-specific secret names
    public static class HavilaSecrets
    {
        public const string SqlConnectionString = "HavilaSqlConnectionString";
        public const string StorageConnectionString = "HavilaStorageConnectionString";
        public const string ServiceBusConnectionString = "HavilaServiceBusConnectionString";
        public const string EventHubsConnectionString = "HavilaEventHubsConnectionString";
        public const string ApplicationInsightsConnectionString = "HavilaApplicationInsightsConnectionString";
        public const string OpenAIApiKey = "HavilaOpenAIApiKey";
        public const string PowerBIClientSecret = "HavilaPowerBIClientSecret";
        public const string ContainerRegistryPassword = "HavilaACRPassword";
    }
}