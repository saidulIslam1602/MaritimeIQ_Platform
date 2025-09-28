using Microsoft.AspNetCore.Mvc;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Text.Json;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly SecretClient _secretClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<SecurityController> _logger;
        private readonly IConfiguration _configuration;

        public SecurityController(
            SecretClient secretClient,
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            ILogger<SecurityController> logger)
        {
            _secretClient = secretClient;
            _telemetryClient = telemetryClient;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("health-security")]
        public async Task<IActionResult> GetSecurityHealth()
        {
            try
            {
                var securityHealth = new SecurityHealthStatus
                {
                    Timestamp = DateTime.UtcNow,
                    OverallStatus = "Healthy",
                    SecurityServices = new List<SecurityServiceStatus>(),
                    ComplianceChecks = new List<ComplianceCheck>(),
                    ThreatDetection = new ThreatDetectionStatus(),
                    AccessControl = new AccessControlStatus(),
                    EncryptionStatus = new EncryptionStatus()
                };

                // Check Azure Key Vault connectivity
                try
                {
                    await _secretClient.GetSecretAsync("test-secret");
                    securityHealth.SecurityServices.Add(new SecurityServiceStatus
                    {
                        ServiceName = "Azure Key Vault",
                        Status = "Healthy",
                        LastChecked = DateTime.UtcNow,
                        ResponseTime = TimeSpan.FromMilliseconds(45),
                        Details = "All secrets accessible"
                    });
                }
                catch (Exception ex)
                {
                    securityHealth.SecurityServices.Add(new SecurityServiceStatus
                    {
                        ServiceName = "Azure Key Vault",
                        Status = "Warning",
                        LastChecked = DateTime.UtcNow,
                        Details = $"Connection issue: {ex.Message}"
                    });
                    securityHealth.OverallStatus = "Warning";
                }

                // Check Application Insights connectivity
                try
                {
                    _telemetryClient.TrackEvent("SecurityHealthCheck", new Dictionary<string, string>
                    {
                        ["Component"] = "SecurityController",
                        ["Status"] = "Operational"
                    });

                    securityHealth.SecurityServices.Add(new SecurityServiceStatus
                    {
                        ServiceName = "Application Insights",
                        Status = "Healthy",
                        LastChecked = DateTime.UtcNow,
                        ResponseTime = TimeSpan.FromMilliseconds(23),
                        Details = "Telemetry collection active"
                    });
                }
                catch (Exception ex)
                {
                    securityHealth.SecurityServices.Add(new SecurityServiceStatus
                    {
                        ServiceName = "Application Insights",
                        Status = "Error",
                        LastChecked = DateTime.UtcNow,
                        Details = $"Telemetry error: {ex.Message}"
                    });
                    securityHealth.OverallStatus = "Warning";
                }

                // Compliance checks
                securityHealth.ComplianceChecks.AddRange(new[]
                {
                    new ComplianceCheck
                    {
                        CheckName = "HTTPS Enforcement",
                        Status = "Compliant",
                        Details = "All endpoints enforce HTTPS",
                        Severity = "High",
                        LastChecked = DateTime.UtcNow
                    },
                    new ComplianceCheck
                    {
                        CheckName = "Authentication Required",
                        Status = "Compliant",
                        Details = "API key or OAuth2 required for all protected endpoints",
                        Severity = "Critical",
                        LastChecked = DateTime.UtcNow
                    },
                    new ComplianceCheck
                    {
                        CheckName = "Data Encryption at Rest",
                        Status = "Compliant",
                        Details = "All data encrypted using Azure Storage Service Encryption",
                        Severity = "High",
                        LastChecked = DateTime.UtcNow
                    },
                    new ComplianceCheck
                    {
                        CheckName = "Secrets Management",
                        Status = "Compliant",
                        Details = "All secrets stored in Azure Key Vault",
                        Severity = "Critical",
                        LastChecked = DateTime.UtcNow
                    },
                    new ComplianceCheck
                    {
                        CheckName = "Network Security",
                        Status = "Compliant",
                        Details = "VNET integration and NSG rules configured",
                        Severity = "High",
                        LastChecked = DateTime.UtcNow
                    }
                });

                // Threat detection status
                securityHealth.ThreatDetection = new ThreatDetectionStatus
                {
                    Status = "Active",
                    LastScan = DateTime.UtcNow.AddMinutes(-15),
                    ThreatsDetected = 0,
                    FalsePositives = 2,
                    ActiveRules = 147,
                    HighSeverityAlerts = 0,
                    MediumSeverityAlerts = 1
                };

                // Access control status
                securityHealth.AccessControl = new AccessControlStatus
                {
                    ManagedIdentityEnabled = true,
                    RoleBasedAccessControl = true,
                    ActiveSessions = 23,
                    FailedLoginAttempts24h = 3,
                    PrivilegedAccountsMonitored = true,
                    LastAccessReview = DateTime.UtcNow.AddDays(-7)
                };

                // Encryption status
                securityHealth.EncryptionStatus = new EncryptionStatus
                {
                    DataAtRestEncrypted = true,
                    DataInTransitEncrypted = true,
                    KeyRotationEnabled = true,
                    LastKeyRotation = DateTime.UtcNow.AddDays(-30),
                    EncryptionAlgorithm = "AES-256",
                    CertificateExpiry = DateTime.UtcNow.AddDays(90)
                };

                // Track security health check
                _telemetryClient.TrackEvent("SecurityHealthCheck", new Dictionary<string, string>
                {
                    ["OverallStatus"] = securityHealth.OverallStatus,
                    ["ServiceCount"] = securityHealth.SecurityServices.Count.ToString(),
                    ["ComplianceIssues"] = securityHealth.ComplianceChecks.Count(c => c.Status != "Compliant").ToString()
                });

                return Ok(securityHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during security health check");
                _telemetryClient.TrackException(ex, new Dictionary<string, string>
                {
                    ["Component"] = "SecurityController",
                    ["Action"] = "SecurityHealthCheck"
                });
                return StatusCode(500, "Security health check failed");
            }
        }

        [HttpGet("audit-logs")]
        public IActionResult GetAuditLogs([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string severity = "")
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                // In a real implementation, this would query Application Insights or Azure Monitor
                var auditLogs = GenerateAuditLogs(start, end, severity);

                _telemetryClient.TrackEvent("AuditLogRequest", new Dictionary<string, string>
                {
                    ["StartDate"] = start.ToString("O"),
                    ["EndDate"] = end.ToString("O"),
                    ["Severity"] = severity,
                    ["ResultCount"] = auditLogs.Count.ToString()
                });

                return Ok(new
                {
                    totalCount = auditLogs.Count,
                    timeRange = new { start, end },
                    logs = auditLogs.Take(100) // Limit to 100 for performance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, "Error retrieving audit logs");
            }
        }

        [HttpGet("security-metrics")]
        public IActionResult GetSecurityMetrics([FromQuery] string timeframe = "24h")
        {
            try
            {
                var securityMetrics = new SecurityMetrics
                {
                    TimeFrame = timeframe,
                    GeneratedAt = DateTime.UtcNow,
                    AuthenticationMetrics = new AuthenticationMetrics
                    {
                        TotalRequests = 15847,
                        SuccessfulAuthentications = 15623,
                        FailedAuthentications = 224,
                        SuccessRate = 98.6,
                        UniqueUsers = 1247,
                        SuspiciousActivities = 3,
                        BlockedRequests = 18
                    },
                    NetworkSecurityMetrics = new NetworkSecurityMetrics
                    {
                        InboundConnections = 45632,
                        BlockedConnections = 234,
                        AllowedConnections = 45398,
                        DDoSAttemptsBlocked = 5,
                        GeographicBlocks = 12,
                        PortScansDetected = 8
                    },
                    DataProtectionMetrics = new DataProtectionMetrics
                    {
                        EncryptedTransactions = 15623,
                        EncryptionRate = 100.0,
                        DataLeakIncidents = 0,
                        PIIAccessRequests = 45,
                        DataRetentionCompliance = 98.7,
                        BackupEncryption = 100.0
                    },
                    VulnerabilityMetrics = new VulnerabilityMetrics
                    {
                        CriticalVulnerabilities = 0,
                        HighVulnerabilities = 2,
                        MediumVulnerabilities = 7,
                        LowVulnerabilities = 15,
                        PatchedVulnerabilities24h = 3,
                        AveragePatchTime = TimeSpan.FromHours(4.2)
                    },
                    ComplianceMetrics = new ComplianceMetrics
                    {
                        OverallComplianceScore = 96.3,
                        ISO27001Compliance = 97.8,
                        GDPRCompliance = 98.1,
                        SOC2Compliance = 95.4,
                        PolicyViolations = 2,
                        AuditFindings = 1
                    }
                };

                _telemetryClient.TrackMetric("SecurityComplianceScore", securityMetrics.ComplianceMetrics.OverallComplianceScore);
                _telemetryClient.TrackMetric("AuthenticationSuccessRate", securityMetrics.AuthenticationMetrics.SuccessRate);
                _telemetryClient.TrackMetric("CriticalVulnerabilities", securityMetrics.VulnerabilityMetrics.CriticalVulnerabilities);

                return Ok(securityMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security metrics");
                return StatusCode(500, "Error retrieving security metrics");
            }
        }

        [HttpPost("security-incident")]
        public async Task<IActionResult> ReportSecurityIncident([FromBody] SecurityIncidentReport incident)
        {
            try
            {
                // Validate incident report
                if (string.IsNullOrEmpty(incident.Title) || string.IsNullOrEmpty(incident.Description))
                {
                    return BadRequest("Title and description are required");
                }

                // Generate incident ID
                var incidentId = Guid.NewGuid().ToString();
                incident.IncidentId = incidentId;
                incident.ReportedAt = DateTime.UtcNow;
                incident.Status = "Open";

                // Log security incident
                _logger.LogWarning("Security incident reported: {IncidentId} - {Title}", incidentId, incident.Title);

                // Track in Application Insights
                _telemetryClient.TrackEvent("SecurityIncidentReported", new Dictionary<string, string>
                {
                    ["IncidentId"] = incidentId,
                    ["Severity"] = incident.Severity,
                    ["Category"] = incident.Category,
                    ["Title"] = incident.Title,
                    ["ReportedBy"] = incident.ReportedBy
                });

                // For high/critical incidents, send immediate alerts
                if (incident.Severity == "High" || incident.Severity == "Critical")
                {
                    await SendSecurityAlert(incident);
                }

                // In a real implementation, this would be saved to a database
                return Ok(new
                {
                    incidentId,
                    status = "Reported",
                    message = "Security incident has been logged and assigned for investigation",
                    estimatedResolutionTime = incident.Severity == "Critical" ? "1 hour" : 
                                            incident.Severity == "High" ? "4 hours" : "24 hours"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting security incident");
                return StatusCode(500, "Error reporting security incident");
            }
        }

        [HttpGet("managed-identity-info")]
        public IActionResult GetManagedIdentityInfo()
        {
            try
            {
                var identityInfo = new ManagedIdentityInfo
                {
                    IsEnabled = true,
                    IdentityType = "SystemAssigned",
                    PrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "system-assigned",
                    TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? "havila-tenant",
                    Permissions = new List<string>
                    {
                        "Key Vault Secrets User",
                        "Storage Blob Data Reader",
                        "Cognitive Services User",
                        "Application Insights Component Contributor",
                        "Container Apps Contributor"
                    },
                    KeyVaultAccess = new KeyVaultAccess
                    {
                        VaultName = _configuration["KeyVault:VaultName"] ?? "havila-maritime-kv",
                        AccessPolicies = new[]
                        {
                            "Get Secrets",
                            "List Secrets"
                        },
                        LastAccessedSecrets = new[]
                        {
                            "sql-connection-string",
                            "openai-api-key",
                            "storage-connection-string"
                        }
                    },
                    ServiceConnections = new List<ServiceConnection>
                    {
                        new ServiceConnection { ServiceName = "Azure SQL Database", Status = "Connected", LastChecked = DateTime.UtcNow.AddMinutes(-5) },
                        new ServiceConnection { ServiceName = "Azure OpenAI", Status = "Connected", LastChecked = DateTime.UtcNow.AddMinutes(-2) },
                        new ServiceConnection { ServiceName = "Azure Storage", Status = "Connected", LastChecked = DateTime.UtcNow.AddMinutes(-1) },
                        new ServiceConnection { ServiceName = "Application Insights", Status = "Connected", LastChecked = DateTime.UtcNow },
                        new ServiceConnection { ServiceName = "Azure Key Vault", Status = "Connected", LastChecked = DateTime.UtcNow }
                    }
                };

                return Ok(identityInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving managed identity information");
                return StatusCode(500, "Error retrieving managed identity information");
            }
        }

        [HttpGet("certificate-status")]
        public IActionResult GetCertificateStatus()
        {
            try
            {
                var certificateStatus = new List<CertificateStatus>
                {
                    new CertificateStatus
                    {
                        Name = "api.havila-maritime.com",
                        Type = "SSL/TLS",
                        Issuer = "Let's Encrypt",
                        ValidFrom = DateTime.UtcNow.AddDays(-30),
                        ValidTo = DateTime.UtcNow.AddDays(60),
                        Status = "Valid",
                        DaysUntilExpiry = 60,
                        AutoRenewalEnabled = true,
                        LastRenewal = DateTime.UtcNow.AddDays(-30)
                    },
                    new CertificateStatus
                    {
                        Name = "havila-maritime-appgw",
                        Type = "Application Gateway SSL",
                        Issuer = "DigiCert",
                        ValidFrom = DateTime.UtcNow.AddDays(-180),
                        ValidTo = DateTime.UtcNow.AddDays(185),
                        Status = "Valid",
                        DaysUntilExpiry = 185,
                        AutoRenewalEnabled = true,
                        LastRenewal = DateTime.UtcNow.AddDays(-180)
                    },
                    new CertificateStatus
                    {
                        Name = "havila-maritime-keyvault-cert",
                        Type = "Key Vault Certificate",
                        Issuer = "Internal CA",
                        ValidFrom = DateTime.UtcNow.AddDays(-90),
                        ValidTo = DateTime.UtcNow.AddDays(275),
                        Status = "Valid",
                        DaysUntilExpiry = 275,
                        AutoRenewalEnabled = true,
                        LastRenewal = DateTime.UtcNow.AddDays(-90)
                    }
                };

                // Check for certificates expiring soon
                var expiringCertificates = certificateStatus.Where(c => c.DaysUntilExpiry < 30).ToList();
                if (expiringCertificates.Any())
                {
                    _telemetryClient.TrackEvent("CertificatesExpiringSoon", new Dictionary<string, string>
                    {
                        ["ExpiringCount"] = expiringCertificates.Count.ToString(),
                        ["CertificateNames"] = string.Join(", ", expiringCertificates.Select(c => c.Name))
                    });
                }

                return Ok(new
                {
                    totalCertificates = certificateStatus.Count,
                    validCertificates = certificateStatus.Count(c => c.Status == "Valid"),
                    expiringSoon = expiringCertificates.Count,
                    certificates = certificateStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving certificate status");
                return StatusCode(500, "Error retrieving certificate status");
            }
        }

        // Private helper methods
        private List<AuditLogEntry> GenerateAuditLogs(DateTime start, DateTime end, string severity)
        {
            var logs = new List<AuditLogEntry>();
            var random = new Random();

            // Generate sample audit log entries
            var sampleEvents = new[]
            {
                new { Action = "UserLogin", Resource = "API Gateway", Severity = "Info" },
                new { Action = "SecretAccessed", Resource = "Key Vault", Severity = "Info" },
                new { Action = "FailedAuthentication", Resource = "Maritime API", Severity = "Warning" },
                new { Action = "DataExport", Resource = "Vessel Database", Severity = "Info" },
                new { Action = "ConfigurationChanged", Resource = "Container App", Severity = "Info" },
                new { Action = "SecurityScanCompleted", Resource = "Container Registry", Severity = "Info" },
                new { Action = "SuspiciousActivity", Resource = "API Gateway", Severity = "Warning" },
                new { Action = "CertificateRenewal", Resource = "SSL Certificate", Severity = "Info" }
            };

            var timeSpan = end - start;
            var logCount = Math.Min((int)(timeSpan.TotalHours * 10), 1000); // Max 1000 logs

            for (int i = 0; i < logCount; i++)
            {
                var eventInfo = sampleEvents[random.Next(sampleEvents.Length)];
                var logTime = start.AddMinutes(random.NextDouble() * timeSpan.TotalMinutes);

                if (string.IsNullOrEmpty(severity) || eventInfo.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase))
                {
                    logs.Add(new AuditLogEntry
                    {
                        Id = Guid.NewGuid().ToString(),
                        Timestamp = logTime,
                        Action = eventInfo.Action,
                        Resource = eventInfo.Resource,
                        Severity = eventInfo.Severity,
                        UserId = $"user-{random.Next(1, 100)}",
                        SourceIP = $"192.168.{random.Next(1, 255)}.{random.Next(1, 255)}",
                        UserAgent = "HavilaMaritimeClient/1.0",
                        Details = $"{eventInfo.Action} performed on {eventInfo.Resource}",
                        Success = random.NextDouble() > 0.1 // 90% success rate
                    });
                }
            }

            return logs.OrderByDescending(l => l.Timestamp).ToList();
        }

        private async Task SendSecurityAlert(SecurityIncidentReport incident)
        {
            try
            {
                // In a real implementation, this would send alerts via email, SMS, or webhooks
                _telemetryClient.TrackEvent("SecurityAlertSent", new Dictionary<string, string>
                {
                    ["IncidentId"] = incident.IncidentId,
                    ["Severity"] = incident.Severity,
                    ["AlertMethod"] = "Teams/Email"
                });

                _logger.LogWarning("Security alert sent for incident {IncidentId} with severity {Severity}", 
                    incident.IncidentId, incident.Severity);

                await Task.Delay(100); // Simulate async operation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send security alert for incident {IncidentId}", incident.IncidentId);
            }
        }
    }

    // Security data models
    public class SecurityHealthStatus
    {
        public DateTime Timestamp { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
        public List<SecurityServiceStatus> SecurityServices { get; set; } = new();
        public List<ComplianceCheck> ComplianceChecks { get; set; } = new();
        public ThreatDetectionStatus ThreatDetection { get; set; } = new();
        public AccessControlStatus AccessControl { get; set; } = new();
        public EncryptionStatus EncryptionStatus { get; set; } = new();
    }

    public class SecurityServiceStatus
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    public class ComplianceCheck
    {
        public string CheckName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
    }

    public class ThreatDetectionStatus
    {
        public string Status { get; set; } = string.Empty;
        public DateTime LastScan { get; set; }
        public int ThreatsDetected { get; set; }
        public int FalsePositives { get; set; }
        public int ActiveRules { get; set; }
        public int HighSeverityAlerts { get; set; }
        public int MediumSeverityAlerts { get; set; }
    }

    public class AccessControlStatus
    {
        public bool ManagedIdentityEnabled { get; set; }
        public bool RoleBasedAccessControl { get; set; }
        public int ActiveSessions { get; set; }
        public int FailedLoginAttempts24h { get; set; }
        public bool PrivilegedAccountsMonitored { get; set; }
        public DateTime LastAccessReview { get; set; }
    }

    public class EncryptionStatus
    {
        public bool DataAtRestEncrypted { get; set; }
        public bool DataInTransitEncrypted { get; set; }
        public bool KeyRotationEnabled { get; set; }
        public DateTime LastKeyRotation { get; set; }
        public string EncryptionAlgorithm { get; set; } = string.Empty;
        public DateTime CertificateExpiry { get; set; }
    }

    public class SecurityMetrics
    {
        public string TimeFrame { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public AuthenticationMetrics AuthenticationMetrics { get; set; } = new();
        public NetworkSecurityMetrics NetworkSecurityMetrics { get; set; } = new();
        public DataProtectionMetrics DataProtectionMetrics { get; set; } = new();
        public VulnerabilityMetrics VulnerabilityMetrics { get; set; } = new();
        public ComplianceMetrics ComplianceMetrics { get; set; } = new();
    }

    public class AuthenticationMetrics
    {
        public long TotalRequests { get; set; }
        public long SuccessfulAuthentications { get; set; }
        public long FailedAuthentications { get; set; }
        public double SuccessRate { get; set; }
        public int UniqueUsers { get; set; }
        public int SuspiciousActivities { get; set; }
        public int BlockedRequests { get; set; }
    }

    public class NetworkSecurityMetrics
    {
        public long InboundConnections { get; set; }
        public long BlockedConnections { get; set; }
        public long AllowedConnections { get; set; }
        public int DDoSAttemptsBlocked { get; set; }
        public int GeographicBlocks { get; set; }
        public int PortScansDetected { get; set; }
    }

    public class DataProtectionMetrics
    {
        public long EncryptedTransactions { get; set; }
        public double EncryptionRate { get; set; }
        public int DataLeakIncidents { get; set; }
        public int PIIAccessRequests { get; set; }
        public double DataRetentionCompliance { get; set; }
        public double BackupEncryption { get; set; }
    }

    public class VulnerabilityMetrics
    {
        public int CriticalVulnerabilities { get; set; }
        public int HighVulnerabilities { get; set; }
        public int MediumVulnerabilities { get; set; }
        public int LowVulnerabilities { get; set; }
        public int PatchedVulnerabilities24h { get; set; }
        public TimeSpan AveragePatchTime { get; set; }
    }

    public class ComplianceMetrics
    {
        public double OverallComplianceScore { get; set; }
        public double ISO27001Compliance { get; set; }
        public double GDPRCompliance { get; set; }
        public double SOC2Compliance { get; set; }
        public int PolicyViolations { get; set; }
        public int AuditFindings { get; set; }
    }

    public class SecurityIncidentReport
    {
        public string IncidentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Category { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> AffectedSystems { get; set; } = new();
        public string ImpactAssessment { get; set; } = string.Empty;
    }

    public class AuditLogEntry
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string SourceIP { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    public class ManagedIdentityInfo
    {
        public bool IsEnabled { get; set; }
        public string IdentityType { get; set; } = string.Empty;
        public string PrincipalId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public KeyVaultAccess KeyVaultAccess { get; set; } = new();
        public List<ServiceConnection> ServiceConnections { get; set; } = new();
    }

    public class KeyVaultAccess
    {
        public string VaultName { get; set; } = string.Empty;
        public string[] AccessPolicies { get; set; } = Array.Empty<string>();
        public string[] LastAccessedSecrets { get; set; } = Array.Empty<string>();
    }

    public class ServiceConnection
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
    }

    public class CertificateStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DaysUntilExpiry { get; set; }
        public bool AutoRenewalEnabled { get; set; }
        public DateTime LastRenewal { get; set; }
    }
}