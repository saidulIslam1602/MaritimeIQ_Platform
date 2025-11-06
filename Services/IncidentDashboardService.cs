using Microsoft.ApplicationInsights;
using MaritimeIQ.Platform.Models.Incident;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Service for incident management dashboard and real-time monitoring
    /// Provides comprehensive SRE metrics and incident tracking capabilities
    /// </summary>
    public interface IIncidentDashboardService
    {
        Task<IncidentDashboard> GetDashboardDataAsync();
        Task<List<IncidentTrend>> GetIncidentTrendsAsync(int days = 30);
        Task<SREMetrics> GetSREMetricsAsync(TimeSpan? period = null);
        Task<List<OnCallStatus>> GetOnCallStatusAsync();
        Task<List<RecentIncident>> GetRecentIncidentsAsync(int limit = 10);
        Task<AlertingSummary> GetAlertingSummaryAsync();
        Task<SystemReliabilityMetrics> GetReliabilityMetricsAsync();
        Task<List<PostMortemSummary>> GetPostMortemSummaryAsync(int limit = 5);
    }

    public class IncidentDashboardService : BaseMaritimeService, IIncidentDashboardService
    {
        private readonly IIncidentManagementService _incidentService;
        private readonly IOnCallService _onCallService;
        private readonly IMonitoringService _monitoringService;
        private readonly TelemetryClient _telemetryClient;

        public override string ServiceName => "Incident Dashboard Service";

        public IncidentDashboardService(
            IIncidentManagementService incidentService,
            IOnCallService onCallService,
            IMonitoringService monitoringService,
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            ILogger<IncidentDashboardService> logger) : base(logger, configuration)
        {
            _incidentService = incidentService;
            _onCallService = onCallService;
            _monitoringService = monitoringService;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Get comprehensive dashboard data
        /// </summary>
        public async Task<IncidentDashboard> GetDashboardDataAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var dashboard = new IncidentDashboard
                {
                    GeneratedAt = DateTime.UtcNow,
                    Metrics = await _incidentService.GetIncidentMetricsAsync(),
                    OnCallTeam = await _onCallService.GetOnCallTeamAsync(),
                    RecentIncidents = await GetRecentIncidentsAsync(5),
                    SystemHealth = await _monitoringService.GetSystemHealthAsync(),
                    SREMetrics = await GetSREMetricsAsync(),
                    AlertingSummary = await GetAlertingSummaryAsync()
                };

                return dashboard;
            });
        }

        /// <summary>
        /// Get incident trends over time
        /// </summary>
        public async Task<List<IncidentTrend>> GetIncidentTrendsAsync(int days = 30)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days);
                var incidents = await _incidentService.GetIncidentHistoryAsync(startDate, endDate);

                var trends = new List<IncidentTrend>();
                
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dayIncidents = incidents.Where(i => i.CreatedAt.Date == date).ToList();
                    
                    trends.Add(new IncidentTrend
                    {
                        Date = date,
                        TotalIncidents = dayIncidents.Count,
                        CriticalIncidents = dayIncidents.Count(i => i.Severity == IncidentSeverity.Critical),
                        HighSeverityIncidents = dayIncidents.Count(i => i.Severity == IncidentSeverity.High),
                        ResolvedIncidents = dayIncidents.Count(i => i.Status == IncidentStatus.Resolved),
                        AverageMTTR = dayIncidents.Where(i => i.TimeToResolve.HasValue).Any() 
                            ? TimeSpan.FromTicks((long)dayIncidents.Where(i => i.TimeToResolve.HasValue).Average(i => i.TimeToResolve!.Value.Ticks))
                            : TimeSpan.Zero
                    });
                }

                return trends;
            });
        }

        /// <summary>
        /// Get comprehensive SRE metrics
        /// </summary>
        public async Task<SREMetrics> GetSREMetricsAsync(TimeSpan? period = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var metrics = await _incidentService.GetIncidentMetricsAsync(period);
                var systemHealth = await _monitoringService.GetSystemHealthAsync();
                
                return new SREMetrics
                {
                    Period = period ?? TimeSpan.FromDays(30),
                    GeneratedAt = DateTime.UtcNow,
                    
                    // Incident metrics
                    TotalIncidents = metrics.TotalIncidents,
                    ActiveIncidents = metrics.ActiveIncidents,
                    MTTA = metrics.AverageMTTA,
                    MTTR = metrics.AverageMTTR,
                    IncidentRate = metrics.IncidentRate,
                    
                    // System reliability
                    SystemUptime = CalculateSystemUptime(systemHealth),
                    ErrorBudgetRemaining = CalculateErrorBudget(metrics),
                    SLOCompliance = CalculateSLOCompliance(systemHealth),
                    
                    // Performance indicators
                    AvailabilityPercentage = CalculateAvailability(systemHealth),
                    PerformanceScore = CalculatePerformanceScore(systemHealth),
                    ReliabilityScore = CalculateReliabilityScore(metrics, systemHealth)
                };
            });
        }

        /// <summary>
        /// Get current on-call status
        /// </summary>
        public async Task<List<OnCallStatus>> GetOnCallStatusAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var onCallTeam = await _onCallService.GetOnCallTeamAsync();
                var activeIncidents = await _incidentService.GetActiveIncidentsAsync();
                
                return onCallTeam.Select(engineer => new OnCallStatus
                {
                    Engineer = engineer,
                    Role = GetEngineerRole(engineer),
                    IsActive = true,
                    AssignedIncidents = activeIncidents.Where(i => i.AssignedTo == engineer.Name).Count(),
                    LastActivity = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(1, 60)), // Simulated
                    ResponseTime = TimeSpan.FromMinutes(Random.Shared.Next(1, 15)) // Simulated
                }).ToList();
            });
        }

        /// <summary>
        /// Get recent incidents summary
        /// </summary>
        public async Task<List<RecentIncident>> GetRecentIncidentsAsync(int limit = 10)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var incidents = await _incidentService.GetIncidentHistoryAsync(limit: limit);
                
                return incidents.Select(i => new RecentIncident
                {
                    Id = i.Id,
                    Title = i.Title,
                    Severity = i.Severity,
                    Status = i.Status,
                    Category = i.Category,
                    CreatedAt = i.CreatedAt,
                    AssignedTo = i.AssignedTo,
                    Duration = i.Duration,
                    AffectedServices = i.AffectedServices
                }).ToList();
            });
        }

        /// <summary>
        /// Get alerting system summary
        /// </summary>
        public async Task<AlertingSummary> GetAlertingSummaryAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var metrics = await _incidentService.GetIncidentMetricsAsync(TimeSpan.FromHours(24));
                
                return new AlertingSummary
                {
                    TotalAlertsLast24h = metrics.TotalIncidents,
                    CriticalAlerts = metrics.CriticalIncidents,
                    HighSeverityAlerts = metrics.HighSeverityIncidents,
                    AlertsAcknowledged = metrics.TotalIncidents - metrics.ActiveIncidents,
                    AlertsResolved = metrics.TotalIncidents - metrics.ActiveIncidents,
                    AverageResponseTime = metrics.AverageMTTA,
                    AlertingSources = new Dictionary<string, int>
                    {
                        ["Application Insights"] = Random.Shared.Next(5, 15),
                        ["System Health"] = Random.Shared.Next(2, 8),
                        ["Vessel Tracking"] = Random.Shared.Next(1, 5),
                        ["Environmental"] = Random.Shared.Next(0, 3),
                        ["Security"] = Random.Shared.Next(0, 2)
                    }
                };
            });
        }

        /// <summary>
        /// Get system reliability metrics
        /// </summary>
        public async Task<SystemReliabilityMetrics> GetReliabilityMetricsAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var systemHealth = await _monitoringService.GetSystemHealthAsync();
                var metrics = await _incidentService.GetIncidentMetricsAsync(TimeSpan.FromDays(30));
                
                return new SystemReliabilityMetrics
                {
                    Uptime = CalculateSystemUptime(systemHealth),
                    AvailabilityPercentage = CalculateAvailability(systemHealth),
                    ErrorRate = CalculateErrorRate(systemHealth),
                    LatencyP95 = CalculateLatencyP95(systemHealth),
                    LatencyP99 = CalculateLatencyP99(systemHealth),
                    ThroughputRPS = CalculateThroughput(systemHealth),
                    IncidentImpactHours = CalculateIncidentImpact(metrics),
                    SLOBreach = CalculateSLOBreaches(systemHealth),
                    ReliabilityScore = CalculateReliabilityScore(metrics, systemHealth)
                };
            });
        }

        /// <summary>
        /// Get post-mortem summaries
        /// </summary>
        public async Task<List<PostMortemSummary>> GetPostMortemSummaryAsync(int limit = 5)
        {
            return await ExecuteOperationAsync(async () =>
            {
                // This would typically query a post-mortem database
                // For now, return simulated data based on recent critical incidents
                var recentCriticalIncidents = await _incidentService.GetIncidentHistoryAsync(
                    DateTime.UtcNow.AddDays(-30), 
                    DateTime.UtcNow, 
                    limit * 2
                );

                var criticalIncidents = recentCriticalIncidents
                    .Where(i => i.Severity == IncidentSeverity.Critical && i.Status == IncidentStatus.Resolved)
                    .Take(limit)
                    .ToList();

                return criticalIncidents.Select(i => new PostMortemSummary
                {
                    IncidentId = i.Id,
                    Title = i.Title,
                    Severity = i.Severity,
                    Duration = i.Duration ?? TimeSpan.Zero,
                    RootCause = GenerateSimulatedRootCause(i.Category),
                    ActionItemsCount = Random.Shared.Next(2, 8),
                    IsPublished = Random.Shared.NextDouble() > 0.3, // 70% published
                    CreatedAt = i.ResolvedAt?.AddHours(Random.Shared.Next(1, 48)) ?? DateTime.UtcNow
                }).ToList();
            });
        }

        // Helper methods for calculations
        private TimeSpan CalculateSystemUptime(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            // Simulated uptime calculation - in production, this would be based on actual metrics
            return TimeSpan.FromDays(Random.Shared.Next(25, 30));
        }

        private double CalculateErrorBudget(IncidentMetrics metrics)
        {
            // Error budget calculation based on SLO (99.9% = 43.2 minutes/month)
            var totalMinutesInMonth = TimeSpan.FromDays(30).TotalMinutes;
            var errorBudgetMinutes = totalMinutesInMonth * 0.001; // 0.1% error budget
            var usedMinutes = metrics.TotalIncidents * 10; // Assume 10 minutes impact per incident
            return Math.Max(0, (errorBudgetMinutes - usedMinutes) / errorBudgetMinutes * 100);
        }

        private double CalculateSLOCompliance(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            // SLO compliance calculation
            return healthStatus.OverallStatus == "Healthy" ? 99.95 : 
                   healthStatus.OverallStatus == "Degraded" ? 99.5 : 98.0;
        }

        private double CalculateAvailability(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            return healthStatus.OverallStatus == "Healthy" ? 99.98 : 
                   healthStatus.OverallStatus == "Degraded" ? 99.2 : 97.5;
        }

        private double CalculatePerformanceScore(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            return healthStatus.OverallStatus == "Healthy" ? 95.0 : 
                   healthStatus.OverallStatus == "Degraded" ? 75.0 : 45.0;
        }

        private double CalculateReliabilityScore(IncidentMetrics incidentMetrics, Models.Monitoring.SystemHealthStatus healthStatus)
        {
            var baseScore = 100.0;
            var incidentPenalty = incidentMetrics.CriticalIncidents * 10 + incidentMetrics.HighSeverityIncidents * 5;
            var healthPenalty = healthStatus.OverallStatus != "Healthy" ? 15 : 0;
            return Math.Max(0, baseScore - incidentPenalty - healthPenalty);
        }

        private double CalculateErrorRate(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            return healthStatus.OverallStatus == "Healthy" ? 0.05 : 
                   healthStatus.OverallStatus == "Degraded" ? 2.5 : 8.0;
        }

        private TimeSpan CalculateLatencyP95(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            return healthStatus.OverallStatus == "Healthy" ? TimeSpan.FromMilliseconds(150) : 
                   healthStatus.OverallStatus == "Degraded" ? TimeSpan.FromMilliseconds(350) : 
                   TimeSpan.FromMilliseconds(800);
        }

        private TimeSpan CalculateLatencyP99(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            return healthStatus.OverallStatus == "Healthy" ? TimeSpan.FromMilliseconds(300) : 
                   healthStatus.OverallStatus == "Degraded" ? TimeSpan.FromMilliseconds(600) : 
                   TimeSpan.FromMilliseconds(1200);
        }

        private double CalculateThroughput(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            return healthStatus.OverallStatus == "Healthy" ? 450.0 : 
                   healthStatus.OverallStatus == "Degraded" ? 280.0 : 120.0;
        }

        private double CalculateIncidentImpact(IncidentMetrics metrics)
        {
            return metrics.CriticalIncidents * 2.5 + metrics.HighSeverityIncidents * 1.0;
        }

        private int CalculateSLOBreaches(Models.Monitoring.SystemHealthStatus healthStatus)
        {
            return healthStatus.OverallStatus == "Healthy" ? 0 : 
                   healthStatus.OverallStatus == "Degraded" ? 1 : 3;
        }

        private string GetEngineerRole(OnCallEngineer engineer)
        {
            if (engineer.Skills.Contains("Manager")) return "Manager";
            if (engineer.Skills.Contains("VP")) return "VP";
            if (engineer.Skills.Contains("SRE")) return "Primary";
            return "Secondary";
        }

        private string GenerateSimulatedRootCause(IncidentCategory category)
        {
            return category switch
            {
                IncidentCategory.Infrastructure => "Database connection pool exhaustion due to increased load",
                IncidentCategory.Performance => "Memory leak in vessel tracking service causing degraded performance",
                IncidentCategory.VesselTracking => "AIS data provider API rate limiting during peak hours",
                IncidentCategory.Environmental => "Sensor calibration drift causing false positive emissions alerts",
                IncidentCategory.Security => "Expired SSL certificate causing authentication failures",
                IncidentCategory.DataQuality => "Data pipeline timeout due to large batch processing",
                _ => "Configuration change deployed without proper validation"
            };
        }
    }

    // Dashboard data models
    public class IncidentDashboard
    {
        public DateTime GeneratedAt { get; set; }
        public IncidentMetrics Metrics { get; set; } = new();
        public List<OnCallEngineer> OnCallTeam { get; set; } = new();
        public List<RecentIncident> RecentIncidents { get; set; } = new();
        public Models.Monitoring.SystemHealthStatus SystemHealth { get; set; } = new();
        public SREMetrics SREMetrics { get; set; } = new();
        public AlertingSummary AlertingSummary { get; set; } = new();
    }

    public class IncidentTrend
    {
        public DateTime Date { get; set; }
        public int TotalIncidents { get; set; }
        public int CriticalIncidents { get; set; }
        public int HighSeverityIncidents { get; set; }
        public int ResolvedIncidents { get; set; }
        public TimeSpan AverageMTTR { get; set; }
    }

    public class SREMetrics
    {
        public TimeSpan Period { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int TotalIncidents { get; set; }
        public int ActiveIncidents { get; set; }
        public TimeSpan MTTA { get; set; }
        public TimeSpan MTTR { get; set; }
        public double IncidentRate { get; set; }
        public TimeSpan SystemUptime { get; set; }
        public double ErrorBudgetRemaining { get; set; }
        public double SLOCompliance { get; set; }
        public double AvailabilityPercentage { get; set; }
        public double PerformanceScore { get; set; }
        public double ReliabilityScore { get; set; }
    }

    public class OnCallStatus
    {
        public OnCallEngineer Engineer { get; set; } = new();
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int AssignedIncidents { get; set; }
        public DateTime LastActivity { get; set; }
        public TimeSpan ResponseTime { get; set; }
    }

    public class RecentIncident
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public IncidentSeverity Severity { get; set; }
        public IncidentStatus Status { get; set; }
        public IncidentCategory Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AssignedTo { get; set; }
        public TimeSpan? Duration { get; set; }
        public List<string> AffectedServices { get; set; } = new();
    }

    public class AlertingSummary
    {
        public int TotalAlertsLast24h { get; set; }
        public int CriticalAlerts { get; set; }
        public int HighSeverityAlerts { get; set; }
        public int AlertsAcknowledged { get; set; }
        public int AlertsResolved { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public Dictionary<string, int> AlertingSources { get; set; } = new();
    }

    public class SystemReliabilityMetrics
    {
        public TimeSpan Uptime { get; set; }
        public double AvailabilityPercentage { get; set; }
        public double ErrorRate { get; set; }
        public TimeSpan LatencyP95 { get; set; }
        public TimeSpan LatencyP99 { get; set; }
        public double ThroughputRPS { get; set; }
        public double IncidentImpactHours { get; set; }
        public int SLOBreach { get; set; }
        public double ReliabilityScore { get; set; }
    }

    public class PostMortemSummary
    {
        public string IncidentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public IncidentSeverity Severity { get; set; }
        public TimeSpan Duration { get; set; }
        public string RootCause { get; set; } = string.Empty;
        public int ActionItemsCount { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
