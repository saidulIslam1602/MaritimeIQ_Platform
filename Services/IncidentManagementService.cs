using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using MaritimeIQ.Platform.Models.Incident;
using System.Collections.Concurrent;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Comprehensive incident management service with real-world SRE practices
    /// </summary>
    public interface IIncidentManagementService
    {
        Task<Incident> CreateIncidentAsync(string title, string description, IncidentSeverity severity, IncidentCategory category, Dictionary<string, object>? customDetails = null);
        Task<Incident?> GetIncidentAsync(string incidentId);
        Task<List<Incident>> GetActiveIncidentsAsync();
        Task<List<Incident>> GetIncidentHistoryAsync(DateTime? from = null, DateTime? to = null, int limit = 100);
        Task<bool> AcknowledgeIncidentAsync(string incidentId, string acknowledgedBy);
        Task<bool> UpdateIncidentStatusAsync(string incidentId, IncidentStatus status, string updatedBy, string? message = null);
        Task<bool> ResolveIncidentAsync(string incidentId, string resolvedBy, string resolutionNote);
        Task<bool> AddIncidentUpdateAsync(string incidentId, string message, string updatedBy, Dictionary<string, object>? metadata = null);
        Task<IncidentMetrics> GetIncidentMetricsAsync(TimeSpan? period = null);
        Task<PostMortem> CreatePostMortemAsync(string incidentId, string createdBy);
        Task<bool> TriggerMaritimeEmergencyIncidentAsync(string vesselId, string emergencyType, Dictionary<string, object> details);
        Task<bool> TriggerEnvironmentalComplianceIncidentAsync(string vesselId, string violationType, double thresholdValue, double actualValue);
        Task<bool> TriggerSystemOutageIncidentAsync(string serviceName, string outageType, List<string> affectedServices);
    }

    public class IncidentManagementService : BaseMaritimeService, IIncidentManagementService
    {
        private readonly IPagerDutyService _pagerDutyService;
        private readonly TelemetryClient _telemetryClient;
        private readonly IOnCallService _onCallService;
        
        // In-memory storage for demo purposes - in production, use a database
        private readonly ConcurrentDictionary<string, Incident> _incidents = new();
        private readonly ConcurrentDictionary<string, PostMortem> _postMortems = new();

        public override string ServiceName => "Incident Management Service";

        public IncidentManagementService(
            IPagerDutyService pagerDutyService,
            TelemetryClient telemetryClient,
            IOnCallService onCallService,
            IConfiguration configuration,
            ILogger<IncidentManagementService> logger) : base(logger, configuration)
        {
            _pagerDutyService = pagerDutyService;
            _telemetryClient = telemetryClient;
            _onCallService = onCallService;
        }

        /// <summary>
        /// Create a new incident with automatic PagerDuty integration
        /// </summary>
        public async Task<Incident> CreateIncidentAsync(string title, string description, IncidentSeverity severity, IncidentCategory category, Dictionary<string, object>? customDetails = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var incident = new Incident
                {
                    Title = title,
                    Description = description,
                    Severity = severity,
                    Category = category,
                    CustomDetails = customDetails ?? new Dictionary<string, object>(),
                    CreatedAt = DateTime.UtcNow
                };

                // Add maritime-specific context
                if (category == IncidentCategory.VesselTracking || category == IncidentCategory.Environmental || category == IncidentCategory.Emergency)
                {
                    incident.CustomDetails["maritime_context"] = new MaritimeIncidentContext
                    {
                        AffectedVessels = customDetails?.ContainsKey("vessel_ids") == true ? 
                            (List<string>?)customDetails["vessel_ids"] : null,
                        IsEnvironmentalImpact = category == IncidentCategory.Environmental,
                        IsEmergencyResponse = category == IncidentCategory.Emergency,
                        Location = customDetails?.ContainsKey("location") == true ? 
                            customDetails["location"]?.ToString() : null
                    };
                }

                // Get current on-call engineer
                var onCallEngineer = await _onCallService.GetCurrentOnCallEngineerAsync();
                if (onCallEngineer != null)
                {
                    incident.AssignedTo = onCallEngineer.Name;
                }

                // Store incident
                _incidents[incident.Id] = incident;

                // Trigger PagerDuty incident for Critical and High severity
                if (severity <= IncidentSeverity.High)
                {
                    try
                    {
                        var pagerDutyDetails = new Dictionary<string, object>(incident.CustomDetails)
                        {
                            ["incident_id"] = incident.Id,
                            ["category"] = category.ToString(),
                            ["created_at"] = incident.CreatedAt,
                            ["assigned_to"] = incident.AssignedTo ?? "Unassigned"
                        };

                        var pagerDutyKey = await _pagerDutyService.TriggerIncidentAsync(
                            title, 
                            severity, 
                            pagerDutyDetails, 
                            incident.Id
                        );

                        incident.PagerDutyIncidentId = pagerDutyKey;
                        _logger.LogInformation("PagerDuty incident created: {PagerDutyKey} for incident {IncidentId}", pagerDutyKey, incident.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create PagerDuty incident for {IncidentId}", incident.Id);
                        // Continue without PagerDuty - incident is still created locally
                    }
                }

                // Track incident creation
                _telemetryClient.TrackEvent("IncidentCreated", new Dictionary<string, string>
                {
                    ["IncidentId"] = incident.Id,
                    ["Severity"] = severity.ToString(),
                    ["Category"] = category.ToString(),
                    ["Title"] = title,
                    ["AssignedTo"] = incident.AssignedTo ?? "Unassigned"
                });

                // Add initial update
                await AddIncidentUpdateAsync(incident.Id, "Incident created", "System", new Dictionary<string, object>
                {
                    ["severity"] = severity.ToString(),
                    ["category"] = category.ToString()
                });

                _logger.LogInformation("Incident created: {IncidentId} - {Title} (Severity: {Severity})", incident.Id, title, severity);
                return incident;
            });
        }

        /// <summary>
        /// Get incident by ID
        /// </summary>
        public async Task<Incident?> GetIncidentAsync(string incidentId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                return _incidents.TryGetValue(incidentId, out var incident) ? incident : null;
            }, nameof(GetIncidentAsync));
        }

        /// <summary>
        /// Get all active incidents
        /// </summary>
        public async Task<List<Incident>> GetActiveIncidentsAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                return _incidents.Values
                    .Where(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToList();
            }, nameof(GetActiveIncidentsAsync));
        }

        /// <summary>
        /// Get incident history with filtering
        /// </summary>
        public async Task<List<Incident>> GetIncidentHistoryAsync(DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                var query = _incidents.Values.AsQueryable();
                
                if (from.HasValue)
                    query = query.Where(i => i.CreatedAt >= from.Value);
                
                if (to.HasValue)
                    query = query.Where(i => i.CreatedAt <= to.Value);
                
                return query
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(limit)
                    .ToList();
            });
        }

        /// <summary>
        /// Acknowledge an incident
        /// </summary>
        public async Task<bool> AcknowledgeIncidentAsync(string incidentId, string acknowledgedBy)
        {
            return await ExecuteOperationAsync(async () =>
            {
                if (!_incidents.TryGetValue(incidentId, out var incident))
                    return false;

                incident.Status = IncidentStatus.Acknowledged;
                incident.AcknowledgedAt = DateTime.UtcNow;
                incident.AssignedTo = acknowledgedBy;

                // Acknowledge in PagerDuty if integrated
                if (!string.IsNullOrEmpty(incident.PagerDutyIncidentId))
                {
                    await _pagerDutyService.AcknowledgeIncidentAsync(incident.PagerDutyIncidentId, acknowledgedBy);
                }

                await AddIncidentUpdateAsync(incidentId, $"Incident acknowledged by {acknowledgedBy}", acknowledgedBy);

                _telemetryClient.TrackEvent("IncidentAcknowledged", new Dictionary<string, string>
                {
                    ["IncidentId"] = incidentId,
                    ["AcknowledgedBy"] = acknowledgedBy,
                    ["TimeToAcknowledge"] = incident.TimeToAcknowledge?.ToString() ?? "Unknown"
                });

                _logger.LogInformation("Incident acknowledged: {IncidentId} by {AcknowledgedBy}", incidentId, acknowledgedBy);
                return true;
            });
        }

        /// <summary>
        /// Update incident status
        /// </summary>
        public async Task<bool> UpdateIncidentStatusAsync(string incidentId, IncidentStatus status, string updatedBy, string? message = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                if (!_incidents.TryGetValue(incidentId, out var incident))
                    return false;

                var previousStatus = incident.Status;
                incident.Status = status;

                var updateMessage = message ?? $"Status changed from {previousStatus} to {status}";
                await AddIncidentUpdateAsync(incidentId, updateMessage, updatedBy, new Dictionary<string, object>
                {
                    ["previous_status"] = previousStatus.ToString(),
                    ["new_status"] = status.ToString()
                });

                _logger.LogInformation("Incident status updated: {IncidentId} from {PreviousStatus} to {NewStatus} by {UpdatedBy}", 
                    incidentId, previousStatus, status, updatedBy);
                
                return true;
            });
        }

        /// <summary>
        /// Resolve an incident
        /// </summary>
        public async Task<bool> ResolveIncidentAsync(string incidentId, string resolvedBy, string resolutionNote)
        {
            return await ExecuteOperationAsync(async () =>
            {
                if (!_incidents.TryGetValue(incidentId, out var incident))
                    return false;

                incident.Status = IncidentStatus.Resolved;
                incident.ResolvedAt = DateTime.UtcNow;

                // Resolve in PagerDuty if integrated
                if (!string.IsNullOrEmpty(incident.PagerDutyIncidentId))
                {
                    await _pagerDutyService.ResolveIncidentAsync(incident.PagerDutyIncidentId, resolvedBy, resolutionNote);
                }

                await AddIncidentUpdateAsync(incidentId, $"Incident resolved: {resolutionNote}", resolvedBy, new Dictionary<string, object>
                {
                    ["resolution_note"] = resolutionNote,
                    ["resolved_by"] = resolvedBy
                });

                _telemetryClient.TrackEvent("IncidentResolved", new Dictionary<string, string>
                {
                    ["IncidentId"] = incidentId,
                    ["ResolvedBy"] = resolvedBy,
                    ["TimeToResolve"] = incident.TimeToResolve?.ToString() ?? "Unknown",
                    ["Duration"] = incident.Duration?.ToString() ?? "Unknown"
                });

                _logger.LogInformation("Incident resolved: {IncidentId} by {ResolvedBy} - {ResolutionNote}", incidentId, resolvedBy, resolutionNote);
                return true;
            });
        }

        /// <summary>
        /// Add update to incident timeline
        /// </summary>
        public async Task<bool> AddIncidentUpdateAsync(string incidentId, string message, string updatedBy, Dictionary<string, object>? metadata = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                if (!_incidents.TryGetValue(incidentId, out var incident))
                    return false;

                var update = new IncidentUpdate
                {
                    IncidentId = incidentId,
                    Message = message,
                    UpdatedBy = updatedBy,
                    Metadata = metadata ?? new Dictionary<string, object>()
                };

                incident.Updates.Add(update);

                // Send update to PagerDuty if integrated
                if (!string.IsNullOrEmpty(incident.PagerDutyIncidentId))
                {
                    await _pagerDutyService.SendIncidentUpdateAsync(incident.PagerDutyIncidentId, message, metadata);
                }

                await Task.CompletedTask;
                return true;
            });
        }

        /// <summary>
        /// Get incident metrics for dashboards
        /// </summary>
        public async Task<IncidentMetrics> GetIncidentMetricsAsync(TimeSpan? period = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                var cutoffTime = DateTime.UtcNow.Subtract(period ?? TimeSpan.FromDays(30));
                var incidents = _incidents.Values.Where(i => i.CreatedAt >= cutoffTime).ToList();

                var metrics = new IncidentMetrics
                {
                    TotalIncidents = incidents.Count,
                    ActiveIncidents = incidents.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed),
                    CriticalIncidents = incidents.Count(i => i.Severity == IncidentSeverity.Critical),
                    HighSeverityIncidents = incidents.Count(i => i.Severity == IncidentSeverity.High),
                    IncidentsByCategory = incidents.GroupBy(i => i.Category).ToDictionary(g => g.Key, g => g.Count()),
                    IncidentsByService = incidents
                        .SelectMany(i => i.AffectedServices)
                        .GroupBy(s => s)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                // Calculate MTTA and MTTR
                var acknowledgedIncidents = incidents.Where(i => i.TimeToAcknowledge.HasValue).ToList();
                if (acknowledgedIncidents.Any())
                {
                    metrics.AverageMTTA = TimeSpan.FromTicks((long)acknowledgedIncidents.Average(i => i.TimeToAcknowledge!.Value.Ticks));
                }

                var resolvedIncidents = incidents.Where(i => i.TimeToResolve.HasValue).ToList();
                if (resolvedIncidents.Any())
                {
                    metrics.AverageMTTR = TimeSpan.FromTicks((long)resolvedIncidents.Average(i => i.TimeToResolve!.Value.Ticks));
                }

                // Calculate incident rate (incidents per day)
                var days = Math.Max(1, (period ?? TimeSpan.FromDays(30)).TotalDays);
                metrics.IncidentRate = incidents.Count / days;

                return metrics;
            });
        }

        /// <summary>
        /// Create post-mortem for resolved incident
        /// </summary>
        public async Task<PostMortem> CreatePostMortemAsync(string incidentId, string createdBy)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var incident = await GetIncidentAsync(incidentId);
                if (incident == null)
                    throw new ArgumentException($"Incident {incidentId} not found");

                var postMortem = new PostMortem
                {
                    IncidentId = incidentId,
                    Incident = incident,
                    CreatedBy = createdBy,
                    Summary = $"Post-mortem for incident: {incident.Title}"
                };

                _postMortems[postMortem.Id] = postMortem;

                _logger.LogInformation("Post-mortem created: {PostMortemId} for incident {IncidentId}", postMortem.Id, incidentId);
                return postMortem;
            });
        }

        /// <summary>
        /// Trigger maritime emergency incident (vessel in distress, man overboard, etc.)
        /// </summary>
        public async Task<bool> TriggerMaritimeEmergencyIncidentAsync(string vesselId, string emergencyType, Dictionary<string, object> details)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var customDetails = new Dictionary<string, object>(details)
                {
                    ["vessel_id"] = vesselId,
                    ["emergency_type"] = emergencyType,
                    ["alert_time"] = DateTime.UtcNow,
                    ["requires_immediate_response"] = true
                };

                var incident = await CreateIncidentAsync(
                    $"MARITIME EMERGENCY: {emergencyType} - Vessel {vesselId}",
                    $"Emergency situation detected for vessel {vesselId}. Type: {emergencyType}. Immediate response required.",
                    IncidentSeverity.Critical,
                    IncidentCategory.Emergency,
                    customDetails
                );

                Logger.LogCritical("Maritime emergency incident triggered: {IncidentId} - Vessel {VesselId}, Type: {EmergencyType}", 
                    incident.Id, vesselId, emergencyType);

                return true;
            });
        }

        /// <summary>
        /// Trigger environmental compliance incident
        /// </summary>
        public async Task<bool> TriggerEnvironmentalComplianceIncidentAsync(string vesselId, string violationType, double thresholdValue, double actualValue)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var severity = actualValue > thresholdValue * 1.5 ? IncidentSeverity.High : IncidentSeverity.Medium;
                
                var customDetails = new Dictionary<string, object>
                {
                    ["vessel_id"] = vesselId,
                    ["violation_type"] = violationType,
                    ["threshold_value"] = thresholdValue,
                    ["actual_value"] = actualValue,
                    ["excess_percentage"] = ((actualValue - thresholdValue) / thresholdValue) * 100,
                    ["regulatory_impact"] = true
                };

                var incident = await CreateIncidentAsync(
                    $"Environmental Compliance Violation - {violationType} - Vessel {vesselId}",
                    $"Vessel {vesselId} has exceeded {violationType} threshold. Limit: {thresholdValue}, Actual: {actualValue}",
                    severity,
                    IncidentCategory.Environmental,
                    customDetails
                );

                Logger.LogWarning("Environmental compliance incident triggered: {IncidentId} - Vessel {VesselId}, {ViolationType}: {ActualValue} > {ThresholdValue}", 
                    incident.Id, vesselId, violationType, actualValue, thresholdValue);

                return true;
            });
        }

        /// <summary>
        /// Trigger system outage incident
        /// </summary>
        public async Task<bool> TriggerSystemOutageIncidentAsync(string serviceName, string outageType, List<string> affectedServices)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var severity = outageType.ToLower().Contains("complete") ? IncidentSeverity.Critical : IncidentSeverity.High;
                
                var customDetails = new Dictionary<string, object>
                {
                    ["service_name"] = serviceName,
                    ["outage_type"] = outageType,
                    ["affected_services"] = affectedServices,
                    ["service_count"] = affectedServices.Count,
                    ["outage_start"] = DateTime.UtcNow
                };

                var incident = await CreateIncidentAsync(
                    $"System Outage: {serviceName} - {outageType}",
                    $"Service outage detected: {serviceName}. Type: {outageType}. Affected services: {string.Join(", ", affectedServices)}",
                    severity,
                    IncidentCategory.Infrastructure,
                    customDetails
                );

                incident.AffectedServices = affectedServices;

                _logger.LogError("System outage incident triggered: {IncidentId} - Service: {ServiceName}, Type: {OutageType}, Affected: {AffectedCount}", 
                    incident.Id, serviceName, outageType, affectedServices.Count);

                return true;
            });
        }
    }
}
