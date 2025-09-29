# 3. Services Architecture

## 3.1 Service Layer Overview

The Maritime Data Engineering Platform implements a sophisticated service-oriented architecture with 13 specialized services, all following a common pattern established by the `BaseMaritimeService`. This design promotes code reuse, consistent error handling, and maintainable business logic.

### 3.1.1 Base Service Implementation

```csharp
public abstract class BaseMaritimeService : IBaseMaritimeService
{
    protected readonly ILogger _logger;
    protected readonly IConfiguration? _configuration;

    public abstract string ServiceName { get; }

    protected BaseMaritimeService(ILogger logger, IConfiguration? configuration = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration;
    }

    public virtual async Task<bool> HealthCheckAsync()
    {
        try
        {
            _logger.LogInformation("Performing health check for {ServiceName}", ServiceName);
            await Task.Delay(10); // Simulate async operation
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for {ServiceName}", ServiceName);
            return false;
        }
    }

    protected async Task<T> ExecuteOperationAsync<T>(
        Func<Task<T>> operation, 
        string operationName)
    {
        try
        {
            LogInformation("Starting operation", operationName);
            var result = await operation();
            LogInformation("Operation completed successfully", operationName);
            return result;
        }
        catch (Exception ex)
        {
            LogError(ex, operationName);
            throw;
        }
    }
}
```

**Key Design Patterns:**
- **Template Method Pattern**: Common operation flow with customizable implementations
- **Strategy Pattern**: Different services implement domain-specific business logic
- **Dependency Injection**: Loose coupling and testability
- **Async/Await**: Non-blocking operations for scalability

### 3.1.2 Complete Service Inventory

| Service | Purpose | Dependencies | Key Methods |
|---------|---------|--------------|-------------|
| **AISProcessingService** | AIS data processing and analytics | Event Hubs, SQL Database | `ProcessAISDataAsync()`, `GetAISAnalyticsAsync()` |
| **EnvironmentalMonitoringService** | Weather and environmental data | External APIs, SQL Database | `GetCurrentEnvironmentalDataAsync()`, `GetForecastAsync()` |
| **RouteOptimizationService** | Route planning and optimization | AI Services, SQL Database | `CalculateOptimalRouteAsync()`, `AnalyzeRouteImpactAsync()` |
| **MaritimeDataService** | Core maritime data operations | SQL Database, Storage | `GetFleetDataAsync()`, `GetVesselPerformanceAsync()` |
| **PassengerNotificationService** | Passenger communication | Service Bus, SMS/Email APIs | `SendNotificationAsync()`, `GetNotificationStatusAsync()` |
| **PowerBIWorkspaceService** | Business intelligence integration | Power BI APIs, SQL Database | `RefreshDatasetAsync()`, `GetReportsAsync()` |
| **CognitiveServicesService** | AI/ML operations | Azure Cognitive Services | `AnalyzeImageAsync()`, `ProcessTextAsync()` |
| **EventHubService** | Real-time data streaming | Azure Event Hubs | `SendEventAsync()`, `ProcessEventsAsync()` |
| **ServiceBusService** | Message queuing | Azure Service Bus | `SendMessageAsync()`, `ReceiveMessagesAsync()` |
| **IoTHubService** | IoT device management | Azure IoT Hub | `SendTelemetryAsync()`, `InvokeDeviceMethodAsync()` |
| **KeyVaultService** | Secrets management | Azure Key Vault | `GetSecretAsync()`, `SetSecretAsync()` |

## 3.2 Detailed Service Analysis

### 3.2.1 AISProcessingService - Maritime Traffic Intelligence

```csharp
public class AISProcessingService : BaseMaritimeService
{
    public override string ServiceName => "AISProcessing";
    
    private readonly EventHubService _eventHubService;
    private readonly MaritimeDataService _dataService;
    
    public AISProcessingService(
        EventHubService eventHubService,
        MaritimeDataService dataService,
        ILogger<AISProcessingService> logger,
        IConfiguration configuration) 
        : base(logger, configuration)
    {
        _eventHubService = eventHubService;
        _dataService = dataService;
    }

    public async Task ProcessAISDataAsync(string[] aisMessages)
    {
        return await ExecuteOperationAsync(async () =>
        {
            var processedData = new List<AISRecord>();
            
            foreach (var message in aisMessages)
            {
                try
                {
                    var aisRecord = ParseAISMessage(message);
                    if (aisRecord != null)
                    {
                        processedData.Add(aisRecord);
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Failed to parse AIS message: {message}", "ProcessAISData");
                }
            }

            // Batch insert to database
            await _dataService.BulkInsertAISDataAsync(processedData);
            
            // Send to real-time processing via Event Hub
            await _eventHubService.SendEventAsync("ais-processed", processedData);
            
            return processedData;
        }, "ProcessAISDataAsync");
    }

    public async Task<AISAnalytics> GetAISAnalyticsAsync()
    {
        return await ExecuteOperationAsync(async () =>
        {
            var analytics = new AISAnalytics();
            
            // Get vessel count and positions
            analytics.ActiveVessels = await _dataService.GetActiveVesselCountAsync();
            analytics.VesselPositions = await _dataService.GetLatestVesselPositionsAsync();
            
            // Calculate traffic density
            analytics.TrafficDensity = CalculateTrafficDensity(analytics.VesselPositions);
            
            // Get performance metrics
            analytics.ProcessingMetrics = await GetProcessingMetricsAsync();
            
            return analytics;
        }, "GetAISAnalyticsAsync");
    }

    private AISRecord? ParseAISMessage(string message)
    {
        // AIS message parsing logic
        // Handles different AIS message types (1-27)
        // Extracts vessel information, position, speed, course, etc.
        
        if (string.IsNullOrWhiteSpace(message) || !message.StartsWith("!AIVDM"))
        {
            return null;
        }

        try
        {
            var parts = message.Split(',');
            if (parts.Length < 6) return null;

            var payload = parts[5];
            var decodedData = DecodeAISPayload(payload);
            
            return new AISRecord
            {
                MMSI = ExtractMMSI(decodedData),
                Latitude = ExtractLatitude(decodedData),
                Longitude = ExtractLongitude(decodedData),
                Speed = ExtractSpeed(decodedData),
                Course = ExtractCourse(decodedData),
                Timestamp = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }
}
```

**Key Capabilities:**
- Real-time AIS message parsing and validation
- Batch processing for high-volume data
- Analytics generation for maritime traffic
- Integration with Event Hubs for streaming

**Performance Optimizations:**
- Bulk database operations
- Asynchronous processing
- Error isolation per message
- Caching for frequently accessed data

### 3.2.2 EnvironmentalMonitoringService - Weather Intelligence

```csharp
public class EnvironmentalMonitoringService : BaseMaritimeService
{
    public override string ServiceName => "EnvironmentalMonitoring";
    
    private readonly HttpClient _httpClient;
    private readonly MaritimeDataService _dataService;
    private readonly string _weatherApiKey;
    
    public EnvironmentalMonitoringService(
        HttpClient httpClient,
        MaritimeDataService dataService,
        ILogger<EnvironmentalMonitoringService> logger,
        IConfiguration configuration) 
        : base(logger, configuration)
    {
        _httpClient = httpClient;
        _dataService = dataService;
        _weatherApiKey = configuration["WeatherAPI:Key"] ?? 
            throw new ArgumentException("Weather API key not configured");
    }

    public async Task<EnvironmentalData> GetCurrentEnvironmentalDataAsync()
    {
        return await ExecuteOperationAsync(async () =>
        {
            var fleetPositions = await _dataService.GetActiveFleetPositionsAsync();
            var environmentalData = new EnvironmentalData();
            
            foreach (var position in fleetPositions)
            {
                var weatherData = await GetWeatherDataForPositionAsync(
                    position.Latitude, position.Longitude);
                
                environmentalData.WeatherReports.Add(new WeatherReport
                {
                    VesselId = position.VesselId,
                    Position = position,
                    Temperature = weatherData.Temperature,
                    WindSpeed = weatherData.WindSpeed,
                    WindDirection = weatherData.WindDirection,
                    WaveHeight = weatherData.WaveHeight,
                    Visibility = weatherData.Visibility,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            // Check for severe weather alerts
            environmentalData.Alerts = await CheckWeatherAlertsAsync(environmentalData.WeatherReports);
            
            return environmentalData;
        }, "GetCurrentEnvironmentalDataAsync");
    }

    public async Task<EnvironmentalForecast> GetEnvironmentalForecastAsync(int hours)
    {
        return await ExecuteOperationAsync(async () =>
        {
            var forecast = new EnvironmentalForecast();
            var routes = await _dataService.GetActiveRoutesAsync();
            
            foreach (var route in routes)
            {
                var routeForecast = await GenerateRouteForecastAsync(route, hours);
                forecast.RouteForecast.Add(routeForecast);
            }
            
            return forecast;
        }, $"GetEnvironmentalForecastAsync-{hours}h");
    }

    private async Task<WeatherData> GetWeatherDataForPositionAsync(double lat, double lon)
    {
        var url = $"https://api.weatherapi.com/v1/current.json?key={_weatherApiKey}&q={lat},{lon}&aqi=no";
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var weatherResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json);
        
        return new WeatherData
        {
            Temperature = weatherResponse.Current.TempC,
            WindSpeed = weatherResponse.Current.WindKph,
            WindDirection = weatherResponse.Current.WindDegree,
            WaveHeight = CalculateWaveHeight(weatherResponse.Current.WindKph),
            Visibility = weatherResponse.Current.VisKm
        };
    }
}
```

**Environmental Monitoring Features:**
- Real-time weather data integration
- Multi-source weather APIs
- Severe weather alerting
- Route-specific forecasting
- Environmental compliance tracking

### 3.2.3 RouteOptimizationService - Intelligent Route Planning

```csharp
public class RouteOptimizationService : BaseMaritimeService
{
    public override string ServiceName => "RouteOptimization";
    
    private readonly CognitiveServicesService _aiService;
    private readonly EnvironmentalMonitoringService _environmentalService;
    private readonly MaritimeDataService _dataService;
    
    public RouteOptimizationService(
        CognitiveServicesService aiService,
        EnvironmentalMonitoringService environmentalService,
        MaritimeDataService dataService,
        ILogger<RouteOptimizationService> logger,
        IConfiguration configuration) 
        : base(logger, configuration)
    {
        _aiService = aiService;
        _environmentalService = environmentalService;
        _dataService = dataService;
    }

    public async Task<OptimalRoute> CalculateOptimalRouteAsync(RouteOptimizationRequest request)
    {
        return await ExecuteOperationAsync(async () =>
        {
            // Multi-objective optimization considering:
            // 1. Distance and time
            // 2. Fuel consumption
            // 3. Weather conditions
            // 4. Traffic patterns
            // 5. Safety factors
            
            var routes = await GenerateRouteAlternativesAsync(request);
            var scoredRoutes = new List<ScoredRoute>();
            
            foreach (var route in routes)
            {
                var score = await CalculateRouteScoreAsync(route, request.OptimizationCriteria);
                scoredRoutes.Add(new ScoredRoute { Route = route, Score = score });
            }
            
            var optimalRoute = scoredRoutes.OrderByDescending(r => r.Score).First().Route;
            
            // Enhance with real-time data
            optimalRoute = await EnhanceRouteWithRealTimeDataAsync(optimalRoute);
            
            return optimalRoute;
        }, "CalculateOptimalRouteAsync");
    }

    public async Task<RouteImpactAnalysis> AnalyzeRouteImpactAsync(RouteImpactAnalysisRequest request)
    {
        return await ExecuteOperationAsync(async () =>
        {
            var analysis = new RouteImpactAnalysis();
            
            // Environmental impact
            analysis.EnvironmentalImpact = await CalculateEnvironmentalImpactAsync(request.Route);
            
            // Economic impact
            analysis.EconomicImpact = await CalculateEconomicImpactAsync(request.Route);
            
            // Safety impact
            analysis.SafetyImpact = await CalculateSafetyImpactAsync(request.Route);
            
            // Time impact
            analysis.TimeImpact = await CalculateTimeImpactAsync(request.Route);
            
            return analysis;
        }, "AnalyzeRouteImpactAsync");
    }

    private async Task<double> CalculateRouteScoreAsync(Route route, OptimizationCriteria criteria)
    {
        var score = 0.0;
        
        // Distance efficiency (shorter is better)
        score += (1.0 / route.TotalDistance) * criteria.DistanceWeight;
        
        // Time efficiency (faster is better)
        score += (1.0 / route.EstimatedTime.TotalHours) * criteria.TimeWeight;
        
        // Fuel efficiency (less consumption is better)
        var fuelConsumption = await EstimateFuelConsumptionAsync(route);
        score += (1.0 / fuelConsumption) * criteria.FuelWeight;
        
        // Weather favorability (better weather is better)
        var weatherScore = await CalculateWeatherScoreAsync(route);
        score += weatherScore * criteria.WeatherWeight;
        
        // Safety score (safer routes are better)
        var safetyScore = await CalculateSafetyScoreAsync(route);
        score += safetyScore * criteria.SafetyWeight;
        
        return score;
    }
}
```

**Route Optimization Features:**
- Multi-objective optimization algorithms
- Real-time weather integration
- Traffic pattern analysis
- Fuel consumption prediction
- Safety risk assessment
- Environmental impact calculation

### 3.2.4 EventHubService - Real-time Data Streaming

```csharp
public class EventHubService : BaseMaritimeService
{
    public override string ServiceName => "EventHub";
    
    private readonly EventHubProducerClient _producerClient;
    private readonly Dictionary<string, EventProcessorClient> _processorClients;
    
    public EventHubService(
        IConfiguration configuration,
        ILogger<EventHubService> logger) 
        : base(logger, configuration)
    {
        var connectionString = configuration.GetConnectionString("EventHub") ??
            throw new ArgumentException("Event Hub connection string not configured");
            
        _producerClient = new EventHubProducerClient(connectionString, "maritime-data");
        _processorClients = new Dictionary<string, EventProcessorClient>();
    }

    public async Task SendEventAsync<T>(string eventType, T data)
    {
        await ExecuteOperationAsync(async () =>
        {
            var eventData = new EventData(JsonSerializer.SerializeToUtf8Bytes(data));
            eventData.Properties["EventType"] = eventType;
            eventData.Properties["Timestamp"] = DateTime.UtcNow.ToString("O");
            
            await _producerClient.SendAsync(new[] { eventData });
            
            LogInformation($"Sent event of type {eventType}", "SendEventAsync");
            return true;
        }, "SendEventAsync");
    }

    public async Task SendBatchEventsAsync<T>(string eventType, IEnumerable<T> data)
    {
        await ExecuteOperationAsync(async () =>
        {
            using var eventBatch = await _producerClient.CreateBatchAsync();
            
            foreach (var item in data)
            {
                var eventData = new EventData(JsonSerializer.SerializeToUtf8Bytes(item));
                eventData.Properties["EventType"] = eventType;
                eventData.Properties["Timestamp"] = DateTime.UtcNow.ToString("O");
                
                if (!eventBatch.TryAdd(eventData))
                {
                    // Send current batch and create new one
                    await _producerClient.SendAsync(eventBatch);
                    using var newBatch = await _producerClient.CreateBatchAsync();
                    newBatch.TryAdd(eventData);
                }
            }
            
            if (eventBatch.Count > 0)
            {
                await _producerClient.SendAsync(eventBatch);
            }
            
            LogInformation($"Sent batch of {eventBatch.Count} events", "SendBatchEventsAsync");
            return true;
        }, "SendBatchEventsAsync");
    }
}
```

**Event Hub Features:**
- High-throughput data streaming
- Batch processing capabilities
- Partition-based scaling
- Event ordering guarantees
- Dead letter queue handling

## 3.3 Service Integration Patterns

### 3.3.1 Dependency Injection Configuration

```csharp
// Program.cs - Service Registration
public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Base services
    services.AddScoped<IBaseMaritimeService, BaseMaritimeService>();
    
    // Domain services
    services.AddScoped<AISProcessingService>();
    services.AddScoped<EnvironmentalMonitoringService>();
    services.AddScoped<RouteOptimizationService>();
    services.AddScoped<MaritimeDataService>();
    
    // Infrastructure services
    services.AddScoped<EventHubService>();
    services.AddScoped<ServiceBusService>();
    services.AddScoped<KeyVaultService>();
    
    // External API services
    services.AddHttpClient<EnvironmentalMonitoringService>();
    services.AddScoped<CognitiveServicesService>();
    
    // Health checks
    services.AddHealthChecks()
        .AddCheck<AISProcessingService>("ais-processing")
        .AddCheck<EnvironmentalMonitoringService>("environmental-monitoring")
        .AddCheck<RouteOptimizationService>("route-optimization");
}
```

### 3.3.2 Service Communication Patterns

#### A. Event-Driven Communication
```csharp
// Service A publishes event
await _eventHubService.SendEventAsync("vessel-position-updated", vesselPosition);

// Service B processes event
public async Task ProcessVesselPositionUpdate(VesselPosition position)
{
    // Business logic here
}
```

#### B. Request-Response Pattern
```csharp
// Service A calls Service B directly
var weatherData = await _environmentalService.GetCurrentEnvironmentalDataAsync();
var optimizedRoute = await _routeService.CalculateOptimalRouteAsync(request);
```

#### C. Saga Pattern for Complex Workflows
```csharp
public class RouteOptimizationSaga
{
    public async Task ExecuteAsync(RouteOptimizationRequest request)
    {
        // Step 1: Get weather data
        var weather = await _environmentalService.GetForecastAsync(24);
        
        // Step 2: Calculate routes
        var routes = await _routeService.GenerateAlternativesAsync(request);
        
        // Step 3: Analyze each route
        var analysis = await _routeService.AnalyzeRoutesAsync(routes, weather);
        
        // Step 4: Select optimal route
        var optimal = await _routeService.SelectOptimalRouteAsync(analysis);
        
        // Step 5: Notify stakeholders
        await _notificationService.NotifyRouteOptimizationCompleteAsync(optimal);
    }
}
```

## 3.4 Error Handling and Resilience

### 3.4.1 Circuit Breaker Pattern

```csharp
public class ResilientService : BaseMaritimeService
{
    private readonly CircuitBreakerPolicy _circuitBreaker;
    
    public ResilientService(ILogger<ResilientService> logger) : base(logger)
    {
        _circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) => 
                {
                    LogWarning($"Circuit breaker opened for {duration}", "CircuitBreaker");
                },
                onReset: () => 
                {
                    LogInformation("Circuit breaker reset", "CircuitBreaker");
                });
    }
    
    public async Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> operation)
    {
        return await _circuitBreaker.ExecuteAsync(operation);
    }
}
```

### 3.4.2 Retry Policies

```csharp
public async Task<T> ExecuteWithRetryAsync<T>(
    Func<Task<T>> operation,
    int maxRetries = 3,
    TimeSpan delay = default)
{
    if (delay == default) delay = TimeSpan.FromSeconds(1);
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxRetries && IsRetryableException(ex))
        {
            LogWarning($"Operation failed on attempt {attempt}, retrying in {delay}", "Retry");
            await Task.Delay(delay);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 1.5); // Exponential backoff
        }
    }
    
    // Final attempt without catch
    return await operation();
}

private bool IsRetryableException(Exception ex)
{
    return ex is HttpRequestException or TimeoutException or TaskCanceledException;
}
```

## 3.5 Performance Optimization

### 3.5.1 Caching Strategy

```csharp
public class CachedMaritimeDataService : BaseMaritimeService
{
    private readonly IMemoryCache _cache;
    private readonly MaritimeDataService _dataService;
    
    public async Task<FleetData> GetFleetDataAsync(string fleetId)
    {
        var cacheKey = $"fleet-data-{fleetId}";
        
        if (_cache.TryGetValue(cacheKey, out FleetData cachedData))
        {
            return cachedData;
        }
        
        var data = await _dataService.GetFleetDataAsync(fleetId);
        
        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
        
        return data;
    }
}
```

### 3.5.2 Batch Processing

```csharp
public async Task ProcessBatchOperationsAsync<T>(
    IEnumerable<T> items,
    Func<IEnumerable<T>, Task> batchProcessor,
    int batchSize = 100)
{
    var batches = items.Chunk(batchSize);
    
    await Parallel.ForEachAsync(batches, new ParallelOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    }, async (batch, ct) =>
    {
        await batchProcessor(batch);
    });
}
```

---

## Interview Preparation Notes for Section 3

### Key Service Architecture Questions:

1. **"How did you eliminate code duplication across your services?"**
   - BaseMaritimeService pattern
   - Template method implementation
   - Common logging and error handling

2. **"Explain your service communication patterns."**
   - Event-driven with Event Hubs
   - Direct service calls for synchronous operations
   - Saga pattern for complex workflows

3. **"How do you handle failures in your service layer?"**
   - Circuit breaker pattern
   - Retry policies with exponential backoff
   - Graceful degradation strategies

4. **"Walk me through your route optimization algorithm."**
   - Multi-objective optimization
   - Real-time data integration
   - Scoring and ranking system

5. **"How do you ensure service scalability?"**
   - Async operations throughout
   - Batch processing capabilities
   - Caching strategies
   - Resource pooling

### Technical Deep-Dive Points:

- Know each service's specific business logic
- Understand inter-service dependencies
- Be able to explain optimization decisions
- Know the data flow between services
- Understand error handling strategies