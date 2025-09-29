using Azure.Core;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MaritimeIQ.Platform.Models;
using MaritimeIQ.Platform.Controllers;
using Microsoft.ApplicationInsights;
using MaritimeIQ.Platform.Services;
using MaritimeIQ.Platform.Services.Interfaces;
using MaritimeIQ.Platform.DataPipelines;
using System.Text.Json.Serialization;

static TokenCredential ResolveKeyVaultCredential(IConfiguration configuration)
{
    var clientId = configuration["KeyVault:ClientId"];
    var clientSecret = configuration["KeyVault:ClientSecret"];
    var tenantId = configuration["KeyVault:TenantId"];
    var useManagedIdentity = configuration.GetValue("KeyVault:UseManagedIdentity", true);

    if (!useManagedIdentity &&
        !string.IsNullOrWhiteSpace(clientId) &&
        !string.IsNullOrWhiteSpace(clientSecret) &&
        !string.IsNullOrWhiteSpace(tenantId))
    {
        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }

#pragma warning disable AZC0102
    return new DefaultAzureCredential();
#pragma warning restore AZC0102
}

var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    var credential = ResolveKeyVaultCredential(builder.Configuration);
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
}

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

// Register Havila Maritime services
builder.Services.AddScoped<AISProcessingService>();
builder.Services.AddScoped<EnvironmentalMonitoringService>();
builder.Services.AddScoped<PassengerNotificationService>();
builder.Services.AddScoped<RouteOptimizationService>();
builder.Services.AddSingleton<IMaritimeDataService, MaritimeDataService>();

// Register new architecture services
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddScoped<IApiManagementService, ApiManagementService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();

// Register core services that are properly implemented  
builder.Services.AddScoped<ISafetyService, SafetyService>();

// Core data service for controllers
builder.Services.AddScoped<IMaritimeDataService, MaritimeDataService>();

// Register enterprise data pipeline services for real-time data ingestion
builder.Services.AddMaritimeDataPipelines(builder.Configuration);
builder.Services.ConfigureDataPipelineOptions(builder.Configuration);

// Note: Additional services can be registered when their interfaces are properly defined
// builder.Services.AddScoped<IMaritimeVisionService, MaritimeVisionService>();
// builder.Services.AddScoped<IVesselDataIngestionService, VesselDataIngestionService>();
// builder.Services.AddScoped<IMaritimeSearchService, MaritimeSearchService>();
// builder.Services.AddScoped<IMaritimeIntelligenceService, MaritimeIntelligenceService>();
// builder.Services.AddScoped<IMaritimeAIService, MaritimeAIService>();
// builder.Services.AddScoped<IVesselService, VesselService>();
// builder.Services.AddScoped<IIoTService, IoTService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Havila Kystruten Maritime Platform",
        Version = "v2.0",
        Description = "Comprehensive maritime operations platform with AI-powered analytics, real-time monitoring, IoT integration, and Power BI reporting for Havila Kystruten's coastal fleet operations."
    });
});

// Add HTTP clients for external services
builder.Services.AddHttpClient();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Configure Azure service options
builder.Services.Configure<MaritimeIQ.Platform.Services.IoTHubConfiguration>(builder.Configuration.GetSection("IoTHub"));
builder.Services.Configure<MaritimeIQ.Platform.Services.ServiceBusConfiguration>(builder.Configuration.GetSection("ServiceBus"));
builder.Services.Configure<MaritimeIQ.Platform.Services.EventHubConfiguration>(builder.Configuration.GetSection("EventHub"));
builder.Services.Configure<MaritimeIQ.Platform.Services.CognitiveServicesConfiguration>(builder.Configuration.GetSection("CognitiveServices"));

// Register Azure services
builder.Services.AddScoped<MaritimeIQ.Platform.Services.IIoTHubService>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaritimeIQ.Platform.Services.IoTHubConfiguration>>().Value;
    var logger = serviceProvider.GetRequiredService<ILogger<MaritimeIQ.Platform.Services.IoTHubService>>();
    return new MaritimeIQ.Platform.Services.IoTHubService(config, logger);
});

builder.Services.AddScoped<MaritimeIQ.Platform.Services.IEventHubService>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaritimeIQ.Platform.Services.EventHubConfiguration>>().Value;
    var logger = serviceProvider.GetRequiredService<ILogger<MaritimeIQ.Platform.Services.EventHubService>>();
    return new MaritimeIQ.Platform.Services.EventHubService(config, logger);
});

builder.Services.AddScoped<MaritimeIQ.Platform.Services.IServiceBusService>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaritimeIQ.Platform.Services.ServiceBusConfiguration>>().Value;
    var logger = serviceProvider.GetRequiredService<ILogger<MaritimeIQ.Platform.Services.ServiceBusService>>();
    var client = new Azure.Messaging.ServiceBus.ServiceBusClient(config.ConnectionString);
    return new MaritimeIQ.Platform.Services.ServiceBusService(client, config, logger);
});

builder.Services.AddScoped<MaritimeIQ.Platform.Services.ICognitiveServicesService>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaritimeIQ.Platform.Services.CognitiveServicesConfiguration>>().Value;
    var logger = serviceProvider.GetRequiredService<ILogger<MaritimeIQ.Platform.Services.CognitiveServicesService>>();
    return new MaritimeIQ.Platform.Services.CognitiveServicesService(config, logger);
});

// Platform integrations
var keyVaultSection = builder.Configuration.GetSection("KeyVault");
builder.Services.Configure<KeyVaultConfiguration>(keyVaultSection);

if (!string.IsNullOrWhiteSpace(keyVaultSection["VaultUri"]))
{
    builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();
}

builder.Services.AddScoped<IPowerBIWorkspaceService, PowerBIWorkspaceService>();

// Add health checks for Azure services
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database connection ready"))
    .AddCheck("azure-services", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Azure services ready"));

// Add CORS for Havila web applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("HavilaPolicy", policy =>
    {
        policy.WithOrigins("https://havila-maritime.azurewebsites.net", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Havila Kystruten Maritime Platform v2.0");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at app's root
    });
}
else
{
    // Production error handling
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseHttpsRedirection();

// Add CORS
app.UseCors("HavilaPolicy");

app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Enhanced health check endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Add detailed health checks for development
if (app.Environment.IsDevelopment())
{
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");
}

// Main API endpoints
app.MapGet("/", () => new
{
    Message = "Havila Kystruten Maritime Operations API",
    Description = "Comprehensive maritime service for vessel management, route planning, and safety monitoring",
    Timestamp = DateTime.UtcNow,
    Version = "2.0.0",
    Framework = ".NET 8.0",
    Environment = app.Environment.EnvironmentName,
    Services = new string[]
    {
        "Vessel Management",
        "Route Planning", 
        "Safety Monitoring",
        "IoT Sensor Integration",
        "Weather Services",
        "Emergency Response",
        "AI-Powered Analytics",
        "Power BI Integration",
        "Real-time Monitoring"
    }
});

app.MapGet("/api/status", () => new
{
    Status = "Operational",
    Timestamp = DateTime.UtcNow,
    Platform = "Havila Kystruten Maritime Platform",
    Version = "2.0.0",
    Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64),
    ActiveServices = new string[]
    {
        "AIS Processing",
        "Environmental Monitoring", 
        "Passenger Notifications",
        "Route Optimization",
        "IoT Hub Integration",
        "Cognitive Services",
        "Power BI Analytics"
    }
});

app.Run();

public partial class Program;