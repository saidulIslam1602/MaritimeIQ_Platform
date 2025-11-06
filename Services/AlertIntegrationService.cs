using Microsoft.ApplicationInsights;
using MaritimeIQ.Platform.Models.Incident;
using MaritimeIQ.Platform.Models.Monitoring;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Service that integrates existing monitoring alerts with the new incident management system
    /// Converts Application Insights alerts and system monitoring into real PagerDuty incidents
    /// </summary>
    public interface IAlertIntegrationService
    {
        Task ProcessApplicationInsightsAlert(string alertName, string alertDescription, Dictionary<string, object> alertData);
        Task ProcessSystemHealthAlert(SystemHealthStatus healthStatus);
        Task ProcessVesselTrackingAlert(string vesselId, string alertType, Dictionary<string, object> vesselData);
        Task ProcessEnvironmentalAlert(string vesselId, string emissionType, double threshold, double actual);
        Task ProcessPerformanceAlert(string serviceName, string metricName, double threshold, double actual);
        Task ProcessSecurityAlert(string alertType, string source, Dictionary<string, object> securityData);
        Task<bool> TestAlertIntegrationAsync();
    }

    public class AlertIntegrationService : BaseMaritimeService, IAlertIntegrationService
    {
        private readonly IIncidentManagementService _incidentService;
        private readonly TelemetryClient _telemetryClient;

        public override string ServiceName => "Alert Integration Service";

        public AlertIntegrationService(
            IIncidentManagementService incidentService,
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            ILogger<AlertIntegrationService> logger) : base(logger, configuration)
        {
            _incidentService = incidentService;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Process Application Insights alerts and convert to incidents
        /// </summary>
        public async Task ProcessApplicationInsightsAlert(string alertName, string alertDescription, Dictionary<string, object> alertData)
        {
            await ExecuteOperationAsync(async () =>
            {
                _logger.LogInformation("Processing Application Insights alert: {AlertName}", alertName);

                // Determine severity based on alert name and data
                var severity = DetermineAlertSeverity(alertName, alertData);
                var category = DetermineAlertCategory(alertName, alertData);

                // Enrich alert data with context
                var customDetails = new Dictionary<string, object>(alertData)
                {
                    ["alert_source"] = "Application Insights",
                    ["alert_name"] = alertName,
                    ["processed_at"] = DateTime.UtcNow,
                    ["platform"] = "MaritimeIQ"
                };

                // Create incident
                var incident = await _incidentService.CreateIncidentAsync(
                    $"Application Alert: {alertName}",
                    alertDescription,
                    severity,
                    category,
                    customDetails
                );

                _telemetryClient.TrackEvent("ApplicationInsightsAlertProcessed", new Dictionary<string, string>
                {
                    ["AlertName"] = alertName,
                    ["IncidentId"] = incident.Id,
                    ["Severity"] = severity.ToString(),
                    ["Category"] = category.ToString()
                });

                _logger.LogInformation("Application Insights alert converted to incident: {IncidentId}", incident.Id);
            }, nameof(ProcessApplicationInsightsAlert));
        }

        /// <summary>
        /// Process system health alerts
        /// </summary>
        public async Task ProcessSystemHealthAlert(SystemHealthStatus healthStatus)
        {
            await ExecuteOperationAsync(async () =>
            {
                // Only create incidents for unhealthy systems
                if (healthStatus.OverallStatus == "Healthy")
                    return;

                _logger.LogWarning("Processing system health alert: {Status}", healthStatus.OverallStatus);

                var severity = healthStatus.OverallStatus switch
                {
                    "Critical" => IncidentSeverity.Critical,
                    "Degraded" => IncidentSeverity.High,
                    "Warning" => IncidentSeverity.Medium,
                    _ => IncidentSeverity.Low
                };

                var unhealthyServices = healthStatus.Services
                    .Where(s => s.Status != "Healthy")
                    .Select(s => s.ServiceName)
                    .ToList();

                var customDetails = new Dictionary<string, object>
                {
                    ["alert_source"] = "System Health Monitor",
                    ["overall_status"] = healthStatus.OverallStatus,
                    ["unhealthy_services"] = unhealthyServices,
                    ["total_services"] = healthStatus.Services.Count,
                    ["timestamp"] = healthStatus.Timestamp,
                    ["performance_metrics"] = healthStatus.Performance,
                    ["infrastructure_status"] = healthStatus.Infrastructure
                };

                var incident = await _incidentService.CreateIncidentAsync(
                    $"System Health Alert: {healthStatus.OverallStatus}",
                    $"System health degraded. Unhealthy services: {string.Join(", ", unhealthyServices)}",
                    severity,
                    IncidentCategory.Infrastructure,
                    customDetails
                );

                _logger.LogWarning("System health alert converted to incident: {IncidentId}", incident.Id);
            }, nameof(ProcessSystemHealthAlert));
        }

        /// <summary>
        /// Process vessel tracking alerts (AIS failures, position updates missing, etc.)
        /// </summary>
        public async Task ProcessVesselTrackingAlert(string vesselId, string alertType, Dictionary<string, object> vesselData)
        {
            await ExecuteOperationAsync(async () =>
            {
                _logger.LogWarning("Processing vessel tracking alert: {VesselId} - {AlertType}", vesselId, alertType);

                var severity = alertType.ToLower() switch
                {
                    "position_lost" => IncidentSeverity.Critical,
                    "ais_failure" => IncidentSeverity.High,
                    "delayed_update" => IncidentSeverity.Medium,
                    "communication_loss" => IncidentSeverity.High,
                    _ => IncidentSeverity.Medium
                };

                var customDetails = new Dictionary<string, object>(vesselData)
                {
                    ["alert_source"] = "Vessel Tracking System",
                    ["vessel_id"] = vesselId,
                    ["alert_type"] = alertType,
                    ["requires_immediate_attention"] = severity <= IncidentSeverity.High,
                    ["maritime_emergency"] = alertType.ToLower().Contains("lost") || alertType.ToLower().Contains("emergency")
                };

                // Check if this is an emergency situation
                if (alertType.ToLower().Contains("emergency") || alertType.ToLower().Contains("distress"))
                {
                    await _incidentService.TriggerMaritimeEmergencyIncidentAsync(vesselId, alertType, customDetails);
                }
                else
                {
                    var incident = await _incidentService.CreateIncidentAsync(
                        $"Vessel Tracking Alert: {alertType} - {vesselId}",
                        $"Vessel tracking issue detected for {vesselId}: {alertType}",
                        severity,
                        IncidentCategory.VesselTracking,
                        customDetails
                    );

                    _logger.LogWarning("Vessel tracking alert converted to incident: {IncidentId}", incident.Id);
                }
            });
        }

        /// <summary>
        /// Process environmental compliance alerts
        /// </summary>
        public async Task ProcessEnvironmentalAlert(string vesselId, string emissionType, double threshold, double actual)
        {
            await ExecuteOperationAsync(async () =>
            {
                _logger.LogWarning("Processing environmental alert: {VesselId} - {EmissionType}: {Actual} > {Threshold}", 
                    vesselId, emissionType, actual, threshold);

                // Use the dedicated environmental compliance incident trigger
                await _incidentService.TriggerEnvironmentalComplianceIncidentAsync(vesselId, emissionType, threshold, actual);

                _telemetryClient.TrackEvent("EnvironmentalAlertProcessed", new Dictionary<string, string>
                {
                    ["VesselId"] = vesselId,
                    ["EmissionType"] = emissionType,
                    ["ThresholdExceeded"] = ((actual - threshold) / threshold * 100).ToString("F1") + "%"
                });
            });
        }

        /// <summary>
        /// Process performance alerts (high latency, error rates, etc.)
        /// </summary>
        public async Task ProcessPerformanceAlert(string serviceName, string metricName, double threshold, double actual)
        {
            await ExecuteOperationAsync(async () =>
            {
                _logger.LogWarning("Processing performance alert: {ServiceName} - {MetricName}: {Actual} vs {Threshold}", 
                    serviceName, metricName, actual, threshold);

                var severity = DeterminePerformanceSeverity(metricName, threshold, actual);
                
                var customDetails = new Dictionary<string, object>
                {
                    ["alert_source"] = "Performance Monitor",
                    ["service_name"] = serviceName,
                    ["metric_name"] = metricName,
                    ["threshold_value"] = threshold,
                    ["actual_value"] = actual,
                    ["deviation_percentage"] = Math.Abs((actual - threshold) / threshold * 100),
                    ["performance_impact"] = severity <= IncidentSeverity.High
                };

                var incident = await _incidentService.CreateIncidentAsync(
                    $"Performance Alert: {serviceName} - {metricName}",
                    $"Performance threshold exceeded for {serviceName}. {metricName}: {actual} (threshold: {threshold})",
                    severity,
                    IncidentCategory.Performance,
                    customDetails
                );

                _logger.LogWarning("Performance alert converted to incident: {IncidentId}", incident.Id);
            });
        }

        /// <summary>
        /// Process security alerts
        /// </summary>
        public async Task ProcessSecurityAlert(string alertType, string source, Dictionary<string, object> securityData)
        {
            await ExecuteOperationAsync(async () =>
            {
                _logger.LogError("Processing security alert: {AlertType} from {Source}", alertType, source);

                var severity = alertType.ToLower() switch
                {
                    "breach" => IncidentSeverity.Critical,
                    "intrusion" => IncidentSeverity.Critical,
                    "unauthorized_access" => IncidentSeverity.High,
                    "suspicious_activity" => IncidentSeverity.Medium,
                    "failed_authentication" => IncidentSeverity.Low,
                    _ => IncidentSeverity.Medium
                };

                var customDetails = new Dictionary<string, object>(securityData)
                {
                    ["alert_source"] = "Security Monitor",
                    ["alert_type"] = alertType,
                    ["source_system"] = source,
                    ["requires_immediate_response"] = severity <= IncidentSeverity.High,
                    ["security_incident"] = true,
                    ["compliance_impact"] = true
                };

                var incident = await _incidentService.CreateIncidentAsync(
                    $"SECURITY ALERT: {alertType}",
                    $"Security incident detected: {alertType} from {source}. Immediate investigation required.",
                    severity,
                    IncidentCategory.Security,
                    customDetails
                );

                _telemetryClient.TrackEvent("SecurityAlertProcessed", new Dictionary<string, string>
                {
                    ["AlertType"] = alertType,
                    ["Source"] = source,
                    ["IncidentId"] = incident.Id,
                    ["Severity"] = severity.ToString()
                });

                _logger.LogError("Security alert converted to incident: {IncidentId}", incident.Id);
            });
        }

        /// <summary>
        /// Test the alert integration system
        /// </summary>
        public async Task<bool> TestAlertIntegrationAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                _logger.LogInformation("Testing alert integration system...");

                try
                {
                    // Test Application Insights alert
                    await ProcessApplicationInsightsAlert(
                        "Test High Error Rate",
                        "Test alert for integration verification",
                        new Dictionary<string, object>
                        {
                            ["error_rate"] = 15.5,
                            ["threshold"] = 5.0,
                            ["test"] = true
                        }
                    );

                    // Test vessel tracking alert
                    await ProcessVesselTrackingAlert(
                        "TEST-VESSEL-001",
                        "delayed_update",
                        new Dictionary<string, object>
                        {
                            ["last_update"] = DateTime.UtcNow.AddMinutes(-15),
                            ["expected_interval"] = 5,
                            ["test"] = true
                        }
                    );

                    // Test performance alert
                    await ProcessPerformanceAlert(
                        "MaritimeAPI",
                        "response_time",
                        200.0,
                        850.0
                    );

                    _logger.LogInformation("Alert integration test completed successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Alert integration test failed");
                    return false;
                }
            });
        }

        /// <summary>
        /// Determine incident severity based on alert characteristics
        /// </summary>
        private IncidentSeverity DetermineAlertSeverity(string alertName, Dictionary<string, object> alertData)
        {
            var alertNameLower = alertName.ToLower();

            // Critical alerts
            if (alertNameLower.Contains("critical") || 
                alertNameLower.Contains("outage") || 
                alertNameLower.Contains("down") ||
                alertNameLower.Contains("emergency"))
            {
                return IncidentSeverity.Critical;
            }

            // High severity alerts
            if (alertNameLower.Contains("high") || 
                alertNameLower.Contains("error") || 
                alertNameLower.Contains("failure") ||
                alertNameLower.Contains("timeout"))
            {
                return IncidentSeverity.High;
            }

            // Medium severity alerts
            if (alertNameLower.Contains("warning") || 
                alertNameLower.Contains("degraded") || 
                alertNameLower.Contains("slow"))
            {
                return IncidentSeverity.Medium;
            }

            // Check alert data for severity indicators
            if (alertData.ContainsKey("severity"))
            {
                var severityValue = alertData["severity"]?.ToString()?.ToLower();
                return severityValue switch
                {
                    "critical" => IncidentSeverity.Critical,
                    "high" => IncidentSeverity.High,
                    "medium" => IncidentSeverity.Medium,
                    "low" => IncidentSeverity.Low,
                    _ => IncidentSeverity.Medium
                };
            }

            return IncidentSeverity.Medium; // Default
        }

        /// <summary>
        /// Determine incident category based on alert characteristics
        /// </summary>
        private IncidentCategory DetermineAlertCategory(string alertName, Dictionary<string, object> alertData)
        {
            var alertNameLower = alertName.ToLower();

            if (alertNameLower.Contains("vessel") || alertNameLower.Contains("ais") || alertNameLower.Contains("tracking"))
                return IncidentCategory.VesselTracking;

            if (alertNameLower.Contains("environmental") || alertNameLower.Contains("emission") || alertNameLower.Contains("co2"))
                return IncidentCategory.Environmental;

            if (alertNameLower.Contains("security") || alertNameLower.Contains("breach") || alertNameLower.Contains("unauthorized"))
                return IncidentCategory.Security;

            if (alertNameLower.Contains("performance") || alertNameLower.Contains("latency") || alertNameLower.Contains("response"))
                return IncidentCategory.Performance;

            if (alertNameLower.Contains("data") || alertNameLower.Contains("quality") || alertNameLower.Contains("pipeline"))
                return IncidentCategory.DataQuality;

            if (alertNameLower.Contains("emergency") || alertNameLower.Contains("distress") || alertNameLower.Contains("mayday"))
                return IncidentCategory.Emergency;

            return IncidentCategory.Infrastructure; // Default
        }

        /// <summary>
        /// Determine performance alert severity based on deviation from threshold
        /// </summary>
        private IncidentSeverity DeterminePerformanceSeverity(string metricName, double threshold, double actual)
        {
            var deviationPercentage = Math.Abs((actual - threshold) / threshold * 100);

            return deviationPercentage switch
            {
                >= 200 => IncidentSeverity.Critical,  // 200%+ deviation
                >= 100 => IncidentSeverity.High,      // 100%+ deviation
                >= 50 => IncidentSeverity.Medium,     // 50%+ deviation
                _ => IncidentSeverity.Low             // < 50% deviation
            };
        }
    }
}
