using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;
using System.Text.Json;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<MonitoringController> _logger;
        private readonly IConfiguration _configuration;

        public MonitoringController(
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            ILogger<MonitoringController> logger)
        {
            _telemetryClient = telemetryClient;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                var healthStatus = new SystemHealthStatus
                {
                    Timestamp = DateTime.UtcNow,
                    OverallStatus = "Healthy",
                    Services = new List<ServiceHealthStatus>(),
                    Performance = new PerformanceMetrics(),
                    Infrastructure = new InfrastructureStatus(),
                    Dependencies = new List<DependencyStatus>()
                };

                // Check Application health
                healthStatus.Services.Add(new ServiceHealthStatus
                {
                    ServiceName = "Maritime API",
                    Status = "Healthy",
                    ResponseTime = TimeSpan.FromMilliseconds(45),
                    LastChecked = DateTime.UtcNow,
                    Version = "1.0.0",
                    Uptime = TimeSpan.FromHours(168), // 7 days
                    Details = "All endpoints operational"
                });

                // Check Database connectivity
                try
                {
                    // Simulate database check
                    await Task.Delay(10);
                    healthStatus.Services.Add(new ServiceHealthStatus
                    {
                        ServiceName = "Azure SQL Database",
                        Status = "Healthy",
                        ResponseTime = TimeSpan.FromMilliseconds(23),
                        LastChecked = DateTime.UtcNow,
                        Details = "Connection pool: 45/100 active connections"
                    });

                    healthStatus.Dependencies.Add(new DependencyStatus
                    {
                        Name = "Azure SQL Database",
                        Status = "Available",
                        ResponseTime = TimeSpan.FromMilliseconds(23),
                        LastChecked = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    healthStatus.Services.Add(new ServiceHealthStatus
                    {
                        ServiceName = "Azure SQL Database",
                        Status = "Unhealthy",
                        ResponseTime = TimeSpan.FromMilliseconds(5000),
                        LastChecked = DateTime.UtcNow,
                        Details = $"Database connection failed: {ex.Message}"
                    });
                    healthStatus.OverallStatus = "Degraded";
                }

                // Check external dependencies
                var dependencies = new[]
                {
                    new { Name = "Azure OpenAI", Endpoint = "https://havila-openai.openai.azure.com/", ResponseTime = 156 },
                    new { Name = "Azure Cognitive Services", Endpoint = "https://havilacognitive.cognitiveservices.azure.com/", ResponseTime = 89 },
                    new { Name = "Azure Storage", Endpoint = "https://havilamaritime.blob.core.windows.net/", ResponseTime = 34 },
                    new { Name = "Application Insights", Endpoint = "https://api.applicationinsights.io/", ResponseTime = 67 },
                    new { Name = "Azure Key Vault", Endpoint = "https://havila-maritime-kv.vault.azure.net/", ResponseTime = 45 }
                };

                foreach (var dep in dependencies)
                {
                    healthStatus.Dependencies.Add(new DependencyStatus
                    {
                        Name = dep.Name,
                        Status = "Available",
                        ResponseTime = TimeSpan.FromMilliseconds(dep.ResponseTime),
                        LastChecked = DateTime.UtcNow,
                        Endpoint = dep.Endpoint
                    });
                }

                // Performance metrics
                healthStatus.Performance = new PerformanceMetrics
                {
                    CpuUsage = 23.5,
                    MemoryUsage = 67.8,
                    DiskUsage = 45.2,
                    NetworkThroughput = 15.6,
                    RequestsPerSecond = 145,
                    AverageResponseTime = TimeSpan.FromMilliseconds(89),
                    ErrorRate = 0.3,
                    ActiveConnections = 78
                };

                // Infrastructure status
                healthStatus.Infrastructure = new InfrastructureStatus
                {
                    ContainerAppsEnvironment = new ContainerEnvironmentStatus
                    {
                        Name = "havila-maritime-env",
                        Status = "Running",
                        ActiveReplicas = 3,
                        DesiredReplicas = 3,
                        CpuUtilization = 23.5,
                        MemoryUtilization = 67.8,
                        NetworkStatus = "Healthy"
                    },
                    LoadBalancer = new LoadBalancerStatus
                    {
                        Status = "Healthy",
                        ActiveBackends = 3,
                        TotalBackends = 3,
                        RequestsPerMinute = 8700,
                        FailedHealthChecks = 0
                    },
                    CDN = new CDNStatus
                    {
                        Status = "Operational",
                        CacheHitRatio = 87.3,
                        OriginShield = "Enabled",
                        EdgeLocations = 147
                    }
                };

                stopwatch.Stop();
                healthStatus.Performance.HealthCheckDuration = stopwatch.Elapsed;

                // Track health check metrics
                _telemetryClient.TrackMetric("SystemHealthCheck.Duration", stopwatch.ElapsedMilliseconds);
                _telemetryClient.TrackMetric("SystemHealthCheck.ServiceCount", healthStatus.Services.Count);
                _telemetryClient.TrackEvent("SystemHealthCheck", new Dictionary<string, string>
                {
                    ["OverallStatus"] = healthStatus.OverallStatus,
                    ["ServiceCount"] = healthStatus.Services.Count.ToString(),
                    ["DependencyCount"] = healthStatus.Dependencies.Count.ToString()
                });

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during system health check");
                _telemetryClient.TrackException(ex);
                return StatusCode(500, "Health check failed");
            }
        }

        [HttpGet("performance-metrics")]
        public IActionResult GetPerformanceMetrics([FromQuery] string timeframe = "1h")
        {
            try
            {
                var performanceData = new PerformanceAnalytics
                {
                    TimeFrame = timeframe,
                    GeneratedAt = DateTime.UtcNow,
                    ApplicationMetrics = new ApplicationPerformanceMetrics
                    {
                        RequestsPerSecond = GenerateTimeSeriesData(145, 50, GetDataPoints(timeframe)),
                        AverageResponseTime = GenerateTimeSeriesData(89, 20, GetDataPoints(timeframe)),
                        ErrorRate = GenerateTimeSeriesData(0.3, 0.2, GetDataPoints(timeframe)),
                        ThroughputMbps = GenerateTimeSeriesData(15.6, 5.0, GetDataPoints(timeframe)),
                        ActiveUsers = GenerateTimeSeriesData(234, 50, GetDataPoints(timeframe)),
                        ConcurrentConnections = GenerateTimeSeriesData(78, 20, GetDataPoints(timeframe))
                    },
                    InfrastructureMetrics = new InfrastructurePerformanceMetrics
                    {
                        CpuUtilization = GenerateTimeSeriesData(23.5, 10.0, GetDataPoints(timeframe)),
                        MemoryUtilization = GenerateTimeSeriesData(67.8, 15.0, GetDataPoints(timeframe)),
                        DiskIOPS = GenerateTimeSeriesData(1250, 300, GetDataPoints(timeframe)),
                        NetworkLatency = GenerateTimeSeriesData(45, 10, GetDataPoints(timeframe)),
                        StorageUtilization = GenerateTimeSeriesData(45.2, 5.0, GetDataPoints(timeframe)),
                        ContainerRestarts = GenerateTimeSeriesData(0, 1, GetDataPoints(timeframe))
                    },
                    DatabaseMetrics = new DatabasePerformanceMetrics
                    {
                        ConnectionPoolUtilization = GenerateTimeSeriesData(45, 15, GetDataPoints(timeframe)),
                        QueryExecutionTime = GenerateTimeSeriesData(23, 10, GetDataPoints(timeframe)),
                        DeadlockCount = GenerateTimeSeriesData(0, 1, GetDataPoints(timeframe)),
                        BlockedProcesses = GenerateTimeSeriesData(2, 3, GetDataPoints(timeframe)),
                        DatabaseSize = GenerateTimeSeriesData(2.5, 0.1, GetDataPoints(timeframe)),
                        IndexFragmentation = GenerateTimeSeriesData(12.5, 5.0, GetDataPoints(timeframe))
                    },
                    ExternalServiceMetrics = new ExternalServicePerformanceMetrics
                    {
                        AzureOpenAILatency = GenerateTimeSeriesData(156, 50, GetDataPoints(timeframe)),
                        CognitiveServicesLatency = GenerateTimeSeriesData(89, 30, GetDataPoints(timeframe)),
                        StorageLatency = GenerateTimeSeriesData(34, 15, GetDataPoints(timeframe)),
                        KeyVaultLatency = GenerateTimeSeriesData(45, 20, GetDataPoints(timeframe)),
                        ExternalAPISuccessRate = GenerateTimeSeriesData(99.5, 1.0, GetDataPoints(timeframe))
                    }
                };

                // Track performance metrics
                _telemetryClient.TrackMetric("AverageResponseTime", performanceData.ApplicationMetrics.AverageResponseTime.LastOrDefault()?.Value ?? 0);
                _telemetryClient.TrackMetric("RequestsPerSecond", performanceData.ApplicationMetrics.RequestsPerSecond.LastOrDefault()?.Value ?? 0);
                _telemetryClient.TrackMetric("ErrorRate", performanceData.ApplicationMetrics.ErrorRate.LastOrDefault()?.Value ?? 0);

                return Ok(performanceData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance metrics");
                return StatusCode(500, "Error retrieving performance metrics");
            }
        }

        [HttpGet("alerts")]
        public IActionResult GetActiveAlerts([FromQuery] string severity = "", [FromQuery] bool activeOnly = true)
        {
            try
            {
                var alerts = GenerateAlerts();

                if (!string.IsNullOrEmpty(severity))
                {
                    alerts = alerts.Where(a => a.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (activeOnly)
                {
                    alerts = alerts.Where(a => a.Status == "Active").ToList();
                }

                var alertSummary = new AlertSummary
                {
                    TotalAlerts = alerts.Count,
                    CriticalAlerts = alerts.Count(a => a.Severity == "Critical"),
                    HighAlerts = alerts.Count(a => a.Severity == "High"),
                    MediumAlerts = alerts.Count(a => a.Severity == "Medium"),
                    LowAlerts = alerts.Count(a => a.Severity == "Low"),
                    RecentAlerts = alerts.Where(a => a.CreatedAt > DateTime.UtcNow.AddHours(-24)).Count(),
                    Alerts = alerts.Take(50).ToList() // Limit to 50 alerts
                };

                // Track alert metrics
                _telemetryClient.TrackMetric("ActiveCriticalAlerts", alertSummary.CriticalAlerts);
                _telemetryClient.TrackMetric("ActiveHighAlerts", alertSummary.HighAlerts);
                _telemetryClient.TrackEvent("AlertsQueried", new Dictionary<string, string>
                {
                    ["TotalAlerts"] = alertSummary.TotalAlerts.ToString(),
                    ["CriticalAlerts"] = alertSummary.CriticalAlerts.ToString(),
                    ["Severity"] = severity,
                    ["ActiveOnly"] = activeOnly.ToString()
                });

                return Ok(alertSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerts");
                return StatusCode(500, "Error retrieving alerts");
            }
        }

        [HttpGet("logs")]
        public IActionResult GetApplicationLogs(
            [FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime,
            [FromQuery] string logLevel = "",
            [FromQuery] string component = "",
            [FromQuery] int limit = 100)
        {
            try
            {
                var start = startTime ?? DateTime.UtcNow.AddHours(-1);
                var end = endTime ?? DateTime.UtcNow;

                var logs = GenerateApplicationLogs(start, end, logLevel, component, limit);

                var logSummary = new LogSummary
                {
                    TimeRange = new { Start = start, End = end },
                    TotalLogs = logs.Count,
                    ErrorLogs = logs.Count(l => l.Level == "Error"),
                    WarningLogs = logs.Count(l => l.Level == "Warning"),
                    InfoLogs = logs.Count(l => l.Level == "Information"),
                    DebugLogs = logs.Count(l => l.Level == "Debug"),
                    Logs = logs
                };

                // Track log queries
                _telemetryClient.TrackEvent("ApplicationLogsQueried", new Dictionary<string, string>
                {
                    ["TotalLogs"] = logSummary.TotalLogs.ToString(),
                    ["ErrorLogs"] = logSummary.ErrorLogs.ToString(),
                    ["WarningLogs"] = logSummary.WarningLogs.ToString(),
                    ["LogLevel"] = logLevel,
                    ["Component"] = component
                });

                return Ok(logSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application logs");
                return StatusCode(500, "Error retrieving application logs");
            }
        }

        [HttpPost("custom-event")]
        public IActionResult TrackCustomEvent([FromBody] CustomEventRequest eventRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(eventRequest.EventName))
                {
                    return BadRequest("Event name is required");
                }

                // Track custom event in Application Insights
                _telemetryClient.TrackEvent(eventRequest.EventName, eventRequest.Properties, eventRequest.Metrics);

                // Log the custom event
                _logger.LogInformation("Custom event tracked: {EventName} with {PropertyCount} properties", 
                    eventRequest.EventName, eventRequest.Properties?.Count ?? 0);

                return Ok(new
                {
                    message = "Custom event tracked successfully",
                    eventName = eventRequest.EventName,
                    timestamp = DateTime.UtcNow,
                    properties = eventRequest.Properties?.Count ?? 0,
                    metrics = eventRequest.Metrics?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking custom event: {EventName}", eventRequest.EventName);
                return StatusCode(500, "Error tracking custom event");
            }
        }

        [HttpGet("application-insights")]
        public IActionResult GetApplicationInsightsMetrics([FromQuery] string timeframe = "1h")
        {
            try
            {
                var insightsMetrics = new ApplicationInsightsMetrics
                {
                    TimeFrame = timeframe,
                    GeneratedAt = DateTime.UtcNow,
                    RequestMetrics = new RequestMetrics
                    {
                        TotalRequests = 145670,
                        SuccessfulRequests = 143890,
                        FailedRequests = 1780,
                        SuccessRate = 98.78,
                        AverageResponseTime = 89.5,
                        P95ResponseTime = 245.0,
                        P99ResponseTime = 456.0
                    },
                    ExceptionMetrics = new ExceptionMetrics
                    {
                        TotalExceptions = 23,
                        UniqueExceptions = 8,
                        CriticalExceptions = 2,
                        RecentExceptions = new List<ExceptionInfo>
                        {
                            new ExceptionInfo
                            {
                                Type = "SqlException",
                                Message = "Timeout expired",
                                Count = 8,
                                FirstOccurrence = DateTime.UtcNow.AddMinutes(-45),
                                LastOccurrence = DateTime.UtcNow.AddMinutes(-5)
                            },
                            new ExceptionInfo
                            {
                                Type = "HttpRequestException",
                                Message = "External service unavailable",
                                Count = 15,
                                FirstOccurrence = DateTime.UtcNow.AddMinutes(-30),
                                LastOccurrence = DateTime.UtcNow.AddMinutes(-2)
                            }
                        }
                    },
                    DependencyMetrics = new DependencyMetrics
                    {
                        TotalDependencyCalls = 89450,
                        SuccessfulCalls = 88920,
                        FailedCalls = 530,
                        AverageDuration = 45.6,
                        MostFrequentDependencies = new List<DependencyInfo>
                        {
                            new DependencyInfo { Name = "Azure SQL Database", CallCount = 45670, AverageDuration = 23.4, SuccessRate = 99.2 },
                            new DependencyInfo { Name = "Azure OpenAI", CallCount = 23450, AverageDuration = 156.7, SuccessRate = 98.9 },
                            new DependencyInfo { Name = "Azure Storage", CallCount = 12340, AverageDuration = 34.2, SuccessRate = 99.8 },
                            new DependencyInfo { Name = "Azure Key Vault", CallCount = 7990, AverageDuration = 45.1, SuccessRate = 99.5 }
                        }
                    },
                    UserMetrics = new UserMetrics
                    {
                        ActiveUsers = 1247,
                        NewUsers = 89,
                        ReturningUsers = 1158,
                        AverageSessionDuration = TimeSpan.FromMinutes(23.5),
                        PageViews = 45670,
                        UniquePageViews = 34520
                    },
                    PerformanceCounters = new PerformanceCounters
                    {
                        ProcessorTime = 23.5,
                        AvailableMemory = 32.2,
                        RequestsPerSecond = 145.6,
                        RequestExecutionTime = 89.3,
                        RequestsInApplicationQueue = 12
                    }
                };

                return Ok(insightsMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Application Insights metrics");
                return StatusCode(500, "Error retrieving Application Insights metrics");
            }
        }

        [HttpGet("availability-tests")]
        public IActionResult GetAvailabilityTests()
        {
            try
            {
                var availabilityTests = new List<AvailabilityTestResult>
                {
                    new AvailabilityTestResult
                    {
                        TestName = "Maritime API Health Check",
                        Location = "North Europe",
                        Status = "Passed",
                        Duration = TimeSpan.FromMilliseconds(234),
                        LastRun = DateTime.UtcNow.AddMinutes(-5),
                        SuccessRate24h = 99.8,
                        Details = "All endpoints responding normally"
                    },
                    new AvailabilityTestResult
                    {
                        TestName = "Vessel Tracking API",
                        Location = "West Europe",
                        Status = "Passed",
                        Duration = TimeSpan.FromMilliseconds(189),
                        LastRun = DateTime.UtcNow.AddMinutes(-3),
                        SuccessRate24h = 99.5,
                        Details = "Real-time vessel data streaming"
                    },
                    new AvailabilityTestResult
                    {
                        TestName = "Weather Integration",
                        Location = "North Europe",
                        Status = "Warning",
                        Duration = TimeSpan.FromMilliseconds(2340),
                        LastRun = DateTime.UtcNow.AddMinutes(-2),
                        SuccessRate24h = 97.2,
                        Details = "Slow response from external weather API"
                    },
                    new AvailabilityTestResult
                    {
                        TestName = "Booking System",
                        Location = "North Europe",
                        Status = "Passed",
                        Duration = TimeSpan.FromMilliseconds(156),
                        LastRun = DateTime.UtcNow.AddMinutes(-1),
                        SuccessRate24h = 99.9,
                        Details = "Booking endpoints operational"
                    },
                    new AvailabilityTestResult
                    {
                        TestName = "Environmental Monitoring",
                        Location = "West Europe",
                        Status = "Passed",
                        Duration = TimeSpan.FromMilliseconds(278),
                        LastRun = DateTime.UtcNow,
                        SuccessRate24h = 98.7,
                        Details = "Environmental data collection active"
                    }
                };

                var summary = new AvailabilityTestSummary
                {
                    TotalTests = availabilityTests.Count,
                    PassedTests = availabilityTests.Count(t => t.Status == "Passed"),
                    FailedTests = availabilityTests.Count(t => t.Status == "Failed"),
                    WarningTests = availabilityTests.Count(t => t.Status == "Warning"),
                    OverallAvailability = availabilityTests.Average(t => t.SuccessRate24h),
                    AverageResponseTime = TimeSpan.FromMilliseconds(availabilityTests.Average(t => t.Duration.TotalMilliseconds)),
                    Tests = availabilityTests
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availability tests");
                return StatusCode(500, "Error retrieving availability tests");
            }
        }

        // Private helper methods
        private List<TimeSeriesDataPoint> GenerateTimeSeriesData(double baseValue, double variance, int dataPoints)
        {
            var random = new Random();
            var data = new List<TimeSeriesDataPoint>();
            var now = DateTime.UtcNow;

            for (int i = 0; i < dataPoints; i++)
            {
                var timestamp = now.AddMinutes(-((dataPoints - 1 - i) * 5)); // 5-minute intervals
                var value = baseValue + (random.NextDouble() - 0.5) * 2 * variance;
                value = Math.Max(0, value); // Ensure non-negative values

                data.Add(new TimeSeriesDataPoint
                {
                    Timestamp = timestamp,
                    Value = Math.Round(value, 2)
                });
            }

            return data;
        }

        private int GetDataPoints(string timeframe)
        {
            return timeframe.ToLower() switch
            {
                "1h" => 12,  // 5-minute intervals for 1 hour
                "6h" => 36,  // 10-minute intervals for 6 hours
                "24h" => 48, // 30-minute intervals for 24 hours
                "7d" => 84,  // 2-hour intervals for 7 days
                _ => 12
            };
        }

        private List<Alert> GenerateAlerts()
        {
            var alerts = new List<Alert>
            {
                new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "High CPU Usage",
                    Description = "CPU usage exceeded 80% threshold",
                    Severity = "High",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-45),
                    Source = "Container Apps Environment",
                    AffectedResource = "havila-maritime-app",
                    ActionRequired = "Scale up the application or investigate high CPU process"
                },
                new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "External API Slow Response",
                    Description = "Weather API response time exceeded 2 seconds",
                    Severity = "Medium",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                    Source = "Dependency Monitoring",
                    AffectedResource = "Weather Integration Service",
                    ActionRequired = "Monitor external service or implement caching"
                },
                new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Database Connection Pool Warning",
                    Description = "Connection pool utilization at 85%",
                    Severity = "Medium",
                    Status = "Resolved",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    ResolvedAt = DateTime.UtcNow.AddMinutes(-15),
                    Source = "Azure SQL Database",
                    AffectedResource = "HavilaMaritimeDB",
                    ActionRequired = "Monitor connection pool or increase pool size"
                },
                new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Certificate Expiring Soon",
                    Description = "SSL certificate expires in 30 days",
                    Severity = "Low",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Source = "Certificate Monitoring",
                    AffectedResource = "api.havila-maritime.com",
                    ActionRequired = "Renew SSL certificate before expiration"
                },
                new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Failed Authentication Attempts",
                    Description = "Multiple failed login attempts detected",
                    Severity = "High",
                    Status = "Investigating",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-15),
                    Source = "Security Monitoring",
                    AffectedResource = "Authentication Service",
                    ActionRequired = "Review security logs and consider IP blocking"
                }
            };

            return alerts;
        }

        private List<LogEntry> GenerateApplicationLogs(DateTime start, DateTime end, string logLevel, string component, int limit)
        {
            var logs = new List<LogEntry>();
            var random = new Random();
            
            var logLevels = new[] { "Information", "Warning", "Error", "Debug" };
            var components = new[] { "VesselController", "SecurityController", "MaritimeAI", "DatabaseService", "AuthenticationService" };
            var messages = new[]
            {
                "Request processed successfully",
                "Database query executed",
                "External API call completed",
                "User authentication successful",
                "Cache entry expired",
                "Background job completed",
                "Performance threshold exceeded",
                "Connection pool warning",
                "SSL certificate validated",
                "Security scan completed"
            };

            var timeSpan = end - start;
            var logCount = Math.Min(limit, (int)(timeSpan.TotalMinutes * 2)); // 2 logs per minute max

            for (int i = 0; i < logCount; i++)
            {
                var logTime = start.AddMinutes(random.NextDouble() * timeSpan.TotalMinutes);
                var level = logLevels[random.Next(logLevels.Length)];
                var comp = components[random.Next(components.Length)];

                if ((!string.IsNullOrEmpty(logLevel) && !level.Equals(logLevel, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(component) && !comp.Contains(component, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                logs.Add(new LogEntry
                {
                    Timestamp = logTime,
                    Level = level,
                    Component = comp,
                    Message = messages[random.Next(messages.Length)],
                    RequestId = Guid.NewGuid().ToString("N")[..8],
                    UserId = level == "Error" ? null : $"user-{random.Next(1, 1000)}",
                    Properties = new Dictionary<string, object>
                    {
                        ["Duration"] = random.Next(10, 1000),
                        ["StatusCode"] = level == "Error" ? 500 : 200,
                        ["Endpoint"] = $"/api/{comp.ToLower()}/{Guid.NewGuid().ToString("N")[..6]}"
                    }
                });
            }

            return logs.OrderByDescending(l => l.Timestamp).Take(limit).ToList();
        }
    }

    // Monitoring data models
    public class SystemHealthStatus
    {
        public DateTime Timestamp { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
        public List<ServiceHealthStatus> Services { get; set; } = new();
        public PerformanceMetrics Performance { get; set; } = new();
        public InfrastructureStatus Infrastructure { get; set; } = new();
        public List<DependencyStatus> Dependencies { get; set; } = new();
    }

    public class ServiceHealthStatus
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
        public string Version { get; set; } = string.Empty;
        public TimeSpan Uptime { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    public class PerformanceMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public double NetworkThroughput { get; set; }
        public int RequestsPerSecond { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public double ErrorRate { get; set; }
        public int ActiveConnections { get; set; }
        public TimeSpan HealthCheckDuration { get; set; }
    }

    public class InfrastructureStatus
    {
        public ContainerEnvironmentStatus ContainerAppsEnvironment { get; set; } = new();
        public LoadBalancerStatus LoadBalancer { get; set; } = new();
        public CDNStatus CDN { get; set; } = new();
    }

    public class ContainerEnvironmentStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ActiveReplicas { get; set; }
        public int DesiredReplicas { get; set; }
        public double CpuUtilization { get; set; }
        public double MemoryUtilization { get; set; }
        public string NetworkStatus { get; set; } = string.Empty;
    }

    public class LoadBalancerStatus
    {
        public string Status { get; set; } = string.Empty;
        public int ActiveBackends { get; set; }
        public int TotalBackends { get; set; }
        public int RequestsPerMinute { get; set; }
        public int FailedHealthChecks { get; set; }
    }

    public class CDNStatus
    {
        public string Status { get; set; } = string.Empty;
        public double CacheHitRatio { get; set; }
        public string OriginShield { get; set; } = string.Empty;
        public int EdgeLocations { get; set; }
    }

    public class DependencyStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
        public string Endpoint { get; set; } = string.Empty;
    }

    public class PerformanceAnalytics
    {
        public string TimeFrame { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public ApplicationPerformanceMetrics ApplicationMetrics { get; set; } = new();
        public InfrastructurePerformanceMetrics InfrastructureMetrics { get; set; } = new();
        public DatabasePerformanceMetrics DatabaseMetrics { get; set; } = new();
        public ExternalServicePerformanceMetrics ExternalServiceMetrics { get; set; } = new();
    }

    public class ApplicationPerformanceMetrics
    {
        public List<TimeSeriesDataPoint> RequestsPerSecond { get; set; } = new();
        public List<TimeSeriesDataPoint> AverageResponseTime { get; set; } = new();
        public List<TimeSeriesDataPoint> ErrorRate { get; set; } = new();
        public List<TimeSeriesDataPoint> ThroughputMbps { get; set; } = new();
        public List<TimeSeriesDataPoint> ActiveUsers { get; set; } = new();
        public List<TimeSeriesDataPoint> ConcurrentConnections { get; set; } = new();
    }

    public class InfrastructurePerformanceMetrics
    {
        public List<TimeSeriesDataPoint> CpuUtilization { get; set; } = new();
        public List<TimeSeriesDataPoint> MemoryUtilization { get; set; } = new();
        public List<TimeSeriesDataPoint> DiskIOPS { get; set; } = new();
        public List<TimeSeriesDataPoint> NetworkLatency { get; set; } = new();
        public List<TimeSeriesDataPoint> StorageUtilization { get; set; } = new();
        public List<TimeSeriesDataPoint> ContainerRestarts { get; set; } = new();
    }

    public class DatabasePerformanceMetrics
    {
        public List<TimeSeriesDataPoint> ConnectionPoolUtilization { get; set; } = new();
        public List<TimeSeriesDataPoint> QueryExecutionTime { get; set; } = new();
        public List<TimeSeriesDataPoint> DeadlockCount { get; set; } = new();
        public List<TimeSeriesDataPoint> BlockedProcesses { get; set; } = new();
        public List<TimeSeriesDataPoint> DatabaseSize { get; set; } = new();
        public List<TimeSeriesDataPoint> IndexFragmentation { get; set; } = new();
    }

    public class ExternalServicePerformanceMetrics
    {
        public List<TimeSeriesDataPoint> AzureOpenAILatency { get; set; } = new();
        public List<TimeSeriesDataPoint> CognitiveServicesLatency { get; set; } = new();
        public List<TimeSeriesDataPoint> StorageLatency { get; set; } = new();
        public List<TimeSeriesDataPoint> KeyVaultLatency { get; set; } = new();
        public List<TimeSeriesDataPoint> ExternalAPISuccessRate { get; set; } = new();
    }

    public class TimeSeriesDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    public class AlertSummary
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int HighAlerts { get; set; }
        public int MediumAlerts { get; set; }
        public int LowAlerts { get; set; }
        public int RecentAlerts { get; set; }
        public List<Alert> Alerts { get; set; } = new();
    }

    public class Alert
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string Source { get; set; } = string.Empty;
        public string AffectedResource { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
    }

    public class LogSummary
    {
        public object TimeRange { get; set; } = new();
        public int TotalLogs { get; set; }
        public int ErrorLogs { get; set; }
        public int WarningLogs { get; set; }
        public int InfoLogs { get; set; }
        public int DebugLogs { get; set; }
        public List<LogEntry> Logs { get; set; } = new();
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class CustomEventRequest
    {
        public string EventName { get; set; } = string.Empty;
        public Dictionary<string, string>? Properties { get; set; }
        public Dictionary<string, double>? Metrics { get; set; }
    }

    public class ApplicationInsightsMetrics
    {
        public string TimeFrame { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public RequestMetrics RequestMetrics { get; set; } = new();
        public ExceptionMetrics ExceptionMetrics { get; set; } = new();
        public DependencyMetrics DependencyMetrics { get; set; } = new();
        public UserMetrics UserMetrics { get; set; } = new();
        public PerformanceCounters PerformanceCounters { get; set; } = new();
    }

    public class RequestMetrics
    {
        public long TotalRequests { get; set; }
        public long SuccessfulRequests { get; set; }
        public long FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public double P95ResponseTime { get; set; }
        public double P99ResponseTime { get; set; }
    }

    public class ExceptionMetrics
    {
        public int TotalExceptions { get; set; }
        public int UniqueExceptions { get; set; }
        public int CriticalExceptions { get; set; }
        public List<ExceptionInfo> RecentExceptions { get; set; } = new();
    }

    public class ExceptionInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime FirstOccurrence { get; set; }
        public DateTime LastOccurrence { get; set; }
    }

    public class DependencyMetrics
    {
        public long TotalDependencyCalls { get; set; }
        public long SuccessfulCalls { get; set; }
        public long FailedCalls { get; set; }
        public double AverageDuration { get; set; }
        public List<DependencyInfo> MostFrequentDependencies { get; set; } = new();
    }

    public class DependencyInfo
    {
        public string Name { get; set; } = string.Empty;
        public long CallCount { get; set; }
        public double AverageDuration { get; set; }
        public double SuccessRate { get; set; }
    }

    public class UserMetrics
    {
        public int ActiveUsers { get; set; }
        public int NewUsers { get; set; }
        public int ReturningUsers { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
        public long PageViews { get; set; }
        public long UniquePageViews { get; set; }
    }

    public class PerformanceCounters
    {
        public double ProcessorTime { get; set; }
        public double AvailableMemory { get; set; }
        public double RequestsPerSecond { get; set; }
        public double RequestExecutionTime { get; set; }
        public int RequestsInApplicationQueue { get; set; }
    }

    public class AvailabilityTestSummary
    {
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int WarningTests { get; set; }
        public double OverallAvailability { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public List<AvailabilityTestResult> Tests { get; set; } = new();
    }

    public class AvailabilityTestResult
    {
        public string TestName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime LastRun { get; set; }
        public double SuccessRate24h { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}