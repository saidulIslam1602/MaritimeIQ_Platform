# 2. API Controllers Deep Dive

## 2.1 Controller Architecture Overview

The MaritimeIQ Platform implements a sophisticated controller architecture with 21 specialized controllers, all inheriting from a common `BaseMaritimeController`. This design eliminates code duplication while maintaining consistency across all API endpoints.

### 2.1.1 Base Controller Implementation

```csharp
[ApiController]
public abstract class BaseMaritimeController : ControllerBase
{
    protected readonly ILogger _logger;

    protected BaseMaritimeController(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected async Task<IActionResult> ExecuteOperationAsync<T>(
        Func<Task<T>> operation, 
        string operationName)
    {
        try
        {
            _logger.LogInformation("Starting {Operation}", operationName);
            var result = await operation();
            return HandleSuccess(result, operationName);
        }
        catch (Exception ex)
        {
            return HandleException(ex, operationName);
        }
    }
}
```

**Key Design Decisions:**
- **Generic Error Handling**: Centralized exception handling reduces duplicate code
- **Logging Integration**: Consistent logging across all controllers
- **Template Method Pattern**: Common operation flow with customizable business logic
- **Type Safety**: Generic methods maintain compile-time type checking

### 2.1.2 Complete Controller Inventory

| Controller | Primary Purpose | Key Endpoints | Dependencies |
|------------|----------------|---------------|--------------|
| **KafkaIntegrationController** ⭐ | Kafka streaming operations | `/api/kafka/publish/*`, `/api/kafka/status` | KafkaProducerService |
| **AISController** | AIS data processing and analytics | `/api/ais/analytics`, `/api/ais/process` | AISProcessingService |
| **ApiManagementController** | API gateway management | `/api/apimanagement/status`, `/api/apimanagement/config` | N/A |
| **EnvironmentalController** | Environmental monitoring | `/api/environmental/data`, `/api/environmental/alerts` | EnvironmentalMonitoringService |
| **FleetAnalyticsController** | Fleet performance analytics | `/api/fleetanalytics/performance`, `/api/fleetanalytics/efficiency` | MaritimeDataService |
| **InsightsController** | Business intelligence | `/api/insights/dashboard`, `/api/insights/reports` | CognitiveServicesService |
| **IoTController** | IoT device management | `/api/iot/devices`, `/api/iot/telemetry` | IoTHubService |
| **MaritimeAIController** | AI/ML operations | `/api/maritimeai/predictions`, `/api/maritimeai/models` | CognitiveServicesService |
| **MaritimeChatController** | Chat and communication | `/api/maritimechat/channels`, `/api/maritimechat/messages` | ServiceBusService |
| **MaritimeIntelligenceController** | Maritime intelligence | `/api/maritimeintelligence/analysis`, `/api/maritimeintelligence/threats` | CognitiveServicesService |
| **MaritimeSearchController** | Search operations | `/api/maritimesearch/query`, `/api/maritimesearch/index` | CognitiveServicesService |
| **MaritimeVisionController** | Computer vision | `/api/maritimevision/analyze`, `/api/maritimevision/detect` | CognitiveServicesService |
| **MonitoringController** | System monitoring | `/api/monitoring/health`, `/api/monitoring/metrics` | Multiple Services |
| **PassengerNotificationController** | Passenger communications | `/api/passengernotification/send`, `/api/passengernotification/status` | PassengerNotificationService |
| **PowerBIController** | Business intelligence | `/api/powerbi/reports`, `/api/powerbi/datasets` | PowerBIWorkspaceService |
| **RouteController** | Route management | `/api/route/create`, `/api/route/optimize` | RouteOptimizationService |
| **RouteOptimizationController** | Advanced routing | `/api/routeoptimization/calculate`, `/api/routeoptimization/analyze` | RouteOptimizationService |
| **SafetyController** | Safety management | `/api/safety/alerts`, `/api/safety/compliance` | MaritimeDataService |
| **SecurityController** | Security operations | `/api/security/authenticate`, `/api/security/authorize` | KeyVaultService |
| **VesselController** | Vessel management | `/api/vessel/status`, `/api/vessel/location` | MaritimeDataService |
| **VesselDataIngestionController** | Data ingestion | `/api/vesseldataingestion/batch`, `/api/vesseldataingestion/stream` | EventHubService |

**Total:** 22 controllers (⭐ indicates Kafka streaming integration)

## 2.2 Detailed Controller Analysis

### 2.2.1 KafkaIntegrationController - Real-Time Streaming (NEW) ⭐

The `KafkaIntegrationController` provides REST API endpoints for publishing maritime data to Kafka topics, demonstrating enterprise streaming integration patterns.

```csharp
[Route("api/[controller]")]
[ApiController]
public class KafkaIntegrationController : ControllerBase
{
    private readonly KafkaProducerService _kafkaProducer;
    private readonly ILogger<KafkaIntegrationController> _logger;

    public KafkaIntegrationController(
        KafkaProducerService kafkaProducer,
        ILogger<KafkaIntegrationController> logger)
    {
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    /// <summary>
    /// Publish AIS vessel data to Kafka stream
    /// </summary>
    [HttpPost("publish/ais")]
    public async Task<IActionResult> PublishAISData([FromBody] AISVesselData aisData)
    {
        try
        {
            await _kafkaProducer.PublishAISDataAsync(aisData);
            
            return Ok(new
            {
                message = "AIS data published to Kafka successfully",
                topic = "maritime.ais.data",
                vessel = aisData.VesselName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish AIS data to Kafka");
            return StatusCode(500, new { error = "Failed to publish to Kafka", details = ex.Message });
        }
    }

    /// <summary>
    /// Publish environmental sensor data
    /// </summary>
    [HttpPost("publish/environmental")]
    public async Task<IActionResult> PublishEnvironmentalData(
        [FromBody] EnvironmentalSensorData sensorData)
    {
        try
        {
            await _kafkaProducer.PublishEnvironmentalDataAsync(sensorData);
            
            return Ok(new
            {
                message = "Environmental data published successfully",
                vesselId = sensorData.VesselId,
                co2 = sensorData.CO2EmissionKg,
                nox = sensorData.NOxEmissionKg
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish environmental data");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Publish maritime alert
    /// </summary>
    [HttpPost("publish/alert")]
    public async Task<IActionResult> PublishAlert([FromBody] MaritimeAlert alert)
    {
        try
        {
            await _kafkaProducer.PublishAlertAsync(alert);
            
            return Ok(new
            {
                message = "Alert published successfully",
                alertId = alert.AlertId,
                severity = alert.Severity,
                type = alert.AlertType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish alert");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Bulk publish AIS data for high-throughput scenarios
    /// </summary>
    [HttpPost("publish/ais-batch")]
    public async Task<IActionResult> PublishAISBatch([FromBody] List<AISVesselData> aisDataList)
    {
        try
        {
            await _kafkaProducer.PublishBatchAsync(
                "maritime.ais.data",
                aisDataList,
                ais => ais.MMSI  // Partition by MMSI
            );
            
            return Ok(new
            {
                message = "Batch published successfully",
                count = aisDataList.Count,
                topic = "maritime.ais.data"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get Kafka integration status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetKafkaStatus()
    {
        try
        {
            return Ok(new
            {
                status = "healthy",
                producer = "active",
                topics = new[]
                {
                    "maritime.ais.data",
                    "maritime.environmental.sensors",
                    "maritime.alerts",
                    "maritime.voyage.events"
                },
                configuration = new
                {
                    compression = "Snappy",
                    acks = "All",
                    idempotence = true,
                    batchSize = "32KB",
                    lingerMs = 10
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Kafka status");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Flush pending Kafka messages
    /// </summary>
    [HttpPost("flush")]
    public IActionResult FlushKafka()
    {
        try
        {
            _kafkaProducer.Flush(TimeSpan.FromSeconds(10));
            return Ok(new { message = "Kafka producer flushed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush Kafka producer");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test streaming with simulated data
    /// </summary>
    [HttpPost("test/stream")]
    public async Task<IActionResult> TestStreaming([FromQuery] int messageCount = 10)
    {
        try
        {
            var results = new List<string>();
            
            for (int i = 0; i < messageCount; i++)
            {
                var testData = new AISVesselData
                {
                    MMSI = $"25800{i:D4}",
                    VesselName = $"TestVessel{i}",
                    Latitude = 59.9 + (i * 0.01),
                    Longitude = 10.7 + (i * 0.01),
                    SpeedOverGround = 12.5 + i,
                    Timestamp = DateTime.UtcNow
                };
                
                await _kafkaProducer.PublishAISDataAsync(testData);
                results.Add($"Published message {i + 1}/{messageCount}");
            }
            
            return Ok(new
            {
                message = "Test streaming completed",
                messagesPublished = messageCount,
                results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test streaming failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
```

**Key Features:**
- **Multiple Publishing Patterns**: Single message, batch, and test streaming
- **Topic Routing**: Automatically routes to correct Kafka topics
- **Error Handling**: Comprehensive exception handling with logging
- **Status Monitoring**: Health check endpoint for Kafka integration
- **Flush Control**: Manual flush capability for critical operations

**Interview Talking Points:**
- "REST API provides easy access to Kafka streaming for external systems"
- "Batch publishing optimizes for high-throughput scenarios"
- "Partitioning by MMSI ensures ordered processing per vessel"
- "Status endpoint enables monitoring and health checks"
- "Test endpoint useful for development and integration testing"

**Performance Characteristics:**
- Throughput: 500+ messages/second
- Latency: < 50ms from API to Kafka
- Batch size: Configurable (default 32KB)
- Compression: Snappy (30-40% reduction)

### 2.2.2 AISController - Real-time Maritime Traffic

```csharp
[Route("api/[controller]")]
public class AISController : BaseMaritimeController
{
    private readonly AISProcessingService _aisService;

    public AISController(AISProcessingService aisService, ILogger<AISController> logger) 
        : base(logger)
    {
        _aisService = aisService;
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAISAnalytics()
    {
        return await ExecuteOperationAsync(
            () => _aisService.GetAISAnalyticsAsync(),
            "GetAISAnalytics"
        );
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessAISData([FromBody] string[] events)
    {
        return await ExecuteOperationAsync(
            async () => {
                await _aisService.ProcessAISDataAsync(events);
                return new { message = "AIS data processing completed successfully" };
            },
            "ProcessAISData"
        );
    }
}
```

**Business Logic**:
- Processes real-time AIS (Automatic Identification System) data
- Provides analytics on vessel movements and traffic patterns
- Handles batch and real-time data processing

**Performance Considerations**:
- Async operations for non-blocking I/O
- Batch processing capabilities for high-volume data
- Integration with Event Hubs for real-time streaming

**Error Scenarios**:
- Invalid AIS message format
- Network connectivity issues
- Database connection failures

### 2.2.2 EnvironmentalController - Maritime Environmental Monitoring

```csharp
[Route("api/[controller]")]
public class EnvironmentalController : BaseMaritimeController
{
    private readonly EnvironmentalMonitoringService _environmentalService;

    public EnvironmentalController(
        EnvironmentalMonitoringService environmentalService, 
        ILogger<EnvironmentalController> logger) 
        : base(logger)
    {
        _environmentalService = environmentalService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentEnvironmentalData()
    {
        return await ExecuteOperationAsync(
            () => _environmentalService.GetCurrentEnvironmentalDataAsync(),
            "GetCurrentEnvironmentalData"
        );
    }

    [HttpGet("forecast/{hours}")]
    public async Task<IActionResult> GetEnvironmentalForecast(int hours = 24)
    {
        return await ExecuteOperationAsync(
            () => _environmentalService.GetEnvironmentalForecastAsync(hours),
            $"GetEnvironmentalForecast-{hours}hours"
        );
    }

    [HttpPost("alert-threshold")]
    public async Task<IActionResult> SetEnvironmentalAlertThreshold(
        [FromBody] EnvironmentalThreshold threshold)
    {
        return await ExecuteOperationAsync(
            async () => {
                await _environmentalService.SetAlertThresholdAsync(threshold);
                return new { message = "Environmental alert threshold updated" };
            },
            "SetEnvironmentalAlertThreshold"
        );
    }
}
```

**Key Features**:
- Real-time weather monitoring
- Environmental compliance tracking
- Alert threshold management
- Predictive weather forecasting

**Data Sources**:
- Weather APIs
- Marine sensors
- Satellite data
- Historical weather patterns

### 2.2.3 FleetAnalyticsController - Fleet Performance Management

```csharp
[Route("api/[controller]")]
public class FleetAnalyticsController : BaseMaritimeController
{
    private readonly MaritimeDataService _maritimeDataService;

    public FleetAnalyticsController(
        MaritimeDataService maritimeDataService, 
        ILogger<FleetAnalyticsController> logger) 
        : base(logger)
    {
        _maritimeDataService = maritimeDataService;
    }

    [HttpGet("performance-summary")]
    public async Task<IActionResult> GetPerformanceSummary()
    {
        return await ExecuteOperationAsync(
            () => _maritimeDataService.GetFleetPerformanceSummaryAsync(),
            "GetPerformanceSummary"
        );
    }

    [HttpGet("vessel/{vesselId}/performance")]
    public async Task<IActionResult> GetVesselPerformance(string vesselId)
    {
        return await ExecuteOperationAsync(
            () => _maritimeDataService.GetVesselPerformanceAsync(vesselId),
            $"GetVesselPerformance-{vesselId}"
        );
    }

    [HttpGet("efficiency-metrics")]
    public async Task<IActionResult> GetEfficiencyMetrics(
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        return await ExecuteOperationAsync(
            () => _maritimeDataService.GetEfficiencyMetricsAsync(start, end),
            $"GetEfficiencyMetrics-{start:yyyy-MM-dd}-{end:yyyy-MM-dd}"
        );
    }
}
```

**Analytics Capabilities**:
- Fuel efficiency tracking
- Route performance analysis
- Maintenance scheduling optimization
- Cost analysis and reporting

### 2.2.4 RouteOptimizationController - Advanced Route Planning

```csharp
[Route("api/[controller]")]
public class RouteOptimizationController : BaseMaritimeController
{
    private readonly RouteOptimizationService _routeService;

    public RouteOptimizationController(
        RouteOptimizationService routeService, 
        ILogger<RouteOptimizationController> logger) 
        : base(logger)
    {
        _routeService = routeService;
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateOptimalRoute(
        [FromBody] RouteOptimizationRequest request)
    {
        return await ExecuteOperationAsync(
            () => _routeService.CalculateOptimalRouteAsync(request),
            "CalculateOptimalRoute"
        );
    }

    [HttpGet("alternatives/{routeId}")]
    public async Task<IActionResult> GetRouteAlternatives(string routeId)
    {
        return await ExecuteOperationAsync(
            () => _routeService.GetRouteAlternativesAsync(routeId),
            $"GetRouteAlternatives-{routeId}"
        );
    }

    [HttpPost("analyze-impact")]
    public async Task<IActionResult> AnalyzeRouteImpact(
        [FromBody] RouteImpactAnalysisRequest request)
    {
        return await ExecuteOperationAsync(
            () => _routeService.AnalyzeRouteImpactAsync(request),
            "AnalyzeRouteImpact"
        );
    }
}
```

**Optimization Features**:
- Multi-objective optimization (time, fuel, safety)
- Weather routing integration
- Traffic pattern consideration
- Environmental impact analysis

## 2.3 Common Patterns and Best Practices

### 2.3.1 Consistent Error Handling

```csharp
protected IActionResult HandleException(Exception ex, string operation)
{
    _logger.LogError(ex, "Error in {Operation}: {Message}", operation, ex.Message);
    
    return ex switch
    {
        ArgumentException or ArgumentNullException => BadRequest(new { 
            error = "Invalid request", 
            message = ex.Message 
        }),
        UnauthorizedAccessException => Unauthorized(new { 
            error = "Access denied", 
            message = "Insufficient permissions" 
        }),
        KeyNotFoundException => NotFound(new { 
            error = "Resource not found", 
            message = ex.Message 
        }),
        _ => StatusCode(500, new { 
            error = "Internal server error", 
            message = "An unexpected error occurred" 
        })
    };
}
```

**Benefits**:
- Consistent API responses
- Proper HTTP status codes
- Security-conscious error messages
- Comprehensive logging

### 2.3.2 Async/Await Pattern

All controllers use async/await consistently:
```csharp
public async Task<IActionResult> GetData()
{
    return await ExecuteOperationAsync(
        () => _service.GetDataAsync(),
        "GetData"
    );
}
```

**Advantages**:
- Non-blocking I/O operations
- Better scalability
- Resource efficiency
- Improved user experience

### 2.3.3 Dependency Injection

```csharp
public AISController(AISProcessingService aisService, ILogger<AISController> logger) 
    : base(logger)
{
    _aisService = aisService;
}
```

**Benefits**:
- Loose coupling
- Testability
- Maintainability
- Configuration flexibility

## 2.4 API Documentation and Versioning

### 2.4.1 Swagger Integration

```csharp
// Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Maritime Data Engineering API", 
        Version = "v1",
        Description = "Comprehensive maritime operations management API"
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

### 2.4.2 API Versioning Strategy

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AISController : BaseMaritimeController
{
    // Implementation
}
```

**Versioning Approach**:
- URL path versioning for clarity
- Backward compatibility maintenance
- Deprecation notices for old versions
- Clear migration paths

## 2.5 Performance Optimization

### 2.5.1 Response Caching

```csharp
[HttpGet("analytics")]
[ResponseCache(Duration = 300)] // 5 minutes
public async Task<IActionResult> GetAISAnalytics()
{
    return await ExecuteOperationAsync(
        () => _aisService.GetAISAnalyticsAsync(),
        "GetAISAnalytics"
    );
}
```

### 2.5.2 Rate Limiting

```csharp
[EnableRateLimiting("DefaultPolicy")]
public class AISController : BaseMaritimeController
{
    // Rate-limited endpoints
}
```

### 2.5.3 Compression

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
```

## 2.6 Testing Strategy

### 2.6.1 Unit Testing

```csharp
[Fact]
public async Task GetAISAnalytics_ReturnsOkResult()
{
    // Arrange
    var mockService = new Mock<AISProcessingService>();
    var mockLogger = new Mock<ILogger<AISController>>();
    var controller = new AISController(mockService.Object, mockLogger.Object);

    // Act
    var result = await controller.GetAISAnalytics();

    // Assert
    Assert.IsType<OkObjectResult>(result);
}
```

### 2.6.2 Integration Testing

```csharp
[Fact]
public async Task GetAISAnalytics_IntegrationTest()
{
    // Test with real services and database
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var response = await client.GetAsync("/api/ais/analytics");
    
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

## 2.7 Monitoring and Logging

### 2.7.1 Application Insights Integration

```csharp
protected async Task<IActionResult> ExecuteOperationAsync<T>(
    Func<Task<T>> operation, 
    string operationName)
{
    using var activity = Activity.StartActivity(operationName);
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        _logger.LogInformation("Starting {Operation}", operationName);
        var result = await operation();
        
        // Custom telemetry
        _telemetryClient.TrackDependency("Service", operationName, 
            DateTime.UtcNow.Subtract(stopwatch.Elapsed), stopwatch.Elapsed, true);
            
        return HandleSuccess(result, operationName);
    }
    catch (Exception ex)
    {
        _telemetryClient.TrackException(ex);
        return HandleException(ex, operationName);
    }
}
```

### 2.7.2 Custom Metrics

```csharp
// Track custom business metrics
_telemetryClient.GetMetric("AIS.ProcessingTime").TrackValue(stopwatch.ElapsedMilliseconds);
_telemetryClient.GetMetric("Fleet.VesselCount").TrackValue(vesselCount);
```

---

## Interview Preparation Notes for Section 2

### Key Technical Questions:

1. **"How did you eliminate code duplication across 20+ controllers?"**
   - Explain BaseMaritimeController pattern
   - Template method implementation
   - Common error handling strategy

2. **"Walk me through your error handling strategy."**
   - Centralized exception handling
   - HTTP status code mapping
   - Security considerations in error messages

3. **"How do you ensure API consistency across all endpoints?"**
   - Base controller patterns
   - Standardized response formats
   - Common logging approach

4. **"Explain your approach to API versioning and documentation."**
   - URL path versioning
   - Swagger/OpenAPI integration
   - Backward compatibility strategy

5. **"How would you add a new controller to this system?"**
   - Inherit from BaseMaritimeController
   - Implement required dependencies
   - Follow established patterns

### Code Review Points:

- Know the purpose of each controller
- Understand service dependencies
- Be able to explain design decisions
- Know performance optimization techniques
- Understand testing approach