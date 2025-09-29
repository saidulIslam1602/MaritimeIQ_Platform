using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Services;
using MaritimeIQ.Platform.Models.Security;

namespace MaritimeIQ.Platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : BaseMaritimeController
    {
        private readonly ISecurityService _securityService;

        public SecurityController(
            ISecurityService securityService,
            ILogger<SecurityController> logger)
            : base(logger)
        {
            _securityService = securityService;
        }


        /// <summary>
        /// Get comprehensive security health status
        /// </summary>
        [HttpGet("health-security")]
        public async Task<IActionResult> GetSecurityHealth()
        {
            return await ExecuteOperationAsync(
                () => _securityService.GetSecurityDashboardAsync(),
                "GetSecurityHealth"
            );
        }

        /// <summary>
        /// Get security dashboard with metrics and recent events
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetSecurityDashboard()
        {
            return await ExecuteOperationAsync(
                () => _securityService.GetSecurityDashboardAsync(),
                "GetSecurityDashboard"
            );
        }

        /// <summary>
        /// Get audit logs for security analysis
        /// </summary>
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] string? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var userAuditLogs = await _securityService.GetAuditLogsAsync(userId, startDate, endDate);
                    return Ok(userAuditLogs);
                }
                else
                {
                    // Get general security events when no specific user is requested
                    var securityEvents = await _securityService.GetSecurityEventsAsync(startDate, endDate);
                    return Ok(securityEvents);
                }
            }, "GetAuditLogs");
        }

        /// <summary>
        /// Get comprehensive security metrics
        /// </summary>
        [HttpGet("security-metrics")]
        public async Task<IActionResult> GetSecurityMetrics([FromQuery] string timeframe = "24h")
        {
            return await ExecuteOperationAsync(async () =>
            {
                var metrics = await _securityService.GetSecurityMetricsAsync(timeframe);
                return Ok(metrics);
            }, "GetSecurityMetrics");
        }

        /// <summary>
        /// Get recent security events
        /// </summary>
        [HttpGet("events")]
        public async Task<IActionResult> GetSecurityEvents(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var events = await _securityService.GetSecurityEventsAsync(startDate, endDate);
                return Ok(events);
            }, "GetSecurityEvents");
        }

        /// <summary>
        /// Get suspicious activities detected by the system
        /// </summary>
        [HttpGet("suspicious-activities")]
        public async Task<IActionResult> GetSuspiciousActivities()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var activities = await _securityService.GetSuspiciousActivitiesAsync();
                return Ok(activities);
            }, "GetSuspiciousActivities");
        }

        /// <summary>
        /// Report a security incident
        /// </summary>
        [HttpPost("security-incident")]
        public async Task<IActionResult> ReportSecurityIncident([FromBody] SecurityIncidentRequest request)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                if (request == null)
                {
                    return BadRequest("Security incident request is required");
                }

                var securityEvent = new Models.Security.SecurityEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = request.IncidentType,
                    Description = request.Description,
                    Severity = request.Severity,
                    UserId = request.ReportedBy,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    Timestamp = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        ["location"] = request.Location ?? "",
                        ["additionalDetails"] = request.AdditionalDetails ?? "",
                        ["reportedBy"] = request.ReportedBy ?? "system"
                    }
                };

                // Convert to service SecurityEvent type
                var serviceEvent = new Services.SecurityEvent 
                { 
                    Type = securityEvent.Type,
                    Description = securityEvent.Description,
                    Severity = securityEvent.Severity,
                    Timestamp = securityEvent.Timestamp
                };
                await _securityService.LogSecurityEventAsync(serviceEvent);

                return Ok(new { 
                    Message = "Security incident reported successfully", 
                    IncidentId = securityEvent.Id,
                    ReportedAt = securityEvent.Timestamp 
                });
            }, "ReportSecurityIncident");
        }

        /// <summary>
        /// Get login attempts for analysis
        /// </summary>
        [HttpGet("login-attempts")]
        public async Task<IActionResult> GetLoginAttempts(
            [FromQuery] string? userId = null,
            [FromQuery] DateTime? startDate = null)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("UserId is required for login attempts query");
                }

                var loginAttempts = await _securityService.GetLoginAttemptsAsync(userId, startDate);
                return Ok(loginAttempts);
            }, "GetLoginAttempts");
        }

        /// <summary>
        /// Get blocked IP addresses
        /// </summary>
        [HttpGet("blocked-ips")]
        public async Task<IActionResult> GetBlockedIpAddresses()
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                var blockedIps = await _securityService.GetBlockedIpAddressesAsync();
                return Ok(blockedIps);
            }, "GetBlockedIps");
        }

        /// <summary>
        /// Block an IP address
        /// </summary>
        [HttpPost("block-ip")]
        public async Task<IActionResult> BlockIpAddress([FromBody] BlockIpRequest request)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                if (request == null || string.IsNullOrEmpty(request.IpAddress))
                {
                    return BadRequest("IP address and reason are required");
                }

                var success = await _securityService.BlockIpAddressAsync(request.IpAddress, request.Reason);
                
                if (!success)
                {
                    return BadRequest($"Failed to block IP address {request.IpAddress}");
                }

                return Ok(new { 
                    Message = $"IP address {request.IpAddress} blocked successfully", 
                    BlockedAt = DateTime.UtcNow 
                });
            }, "BlockIpAddress");
        }

        /// <summary>
        /// Unblock an IP address
        /// </summary>
        [HttpPost("unblock-ip")]
        public async Task<IActionResult> UnblockIpAddress([FromBody] UnblockIpRequest request)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                if (request == null || string.IsNullOrEmpty(request.IpAddress))
                {
                    return BadRequest("IP address is required");
                }

                var success = await _securityService.UnblockIpAddressAsync(request.IpAddress);
                
                if (!success)
                {
                    return BadRequest($"Failed to unblock IP address {request.IpAddress}");
                }

                return Ok(new { 
                    Message = $"IP address {request.IpAddress} unblocked successfully", 
                    UnblockedAt = DateTime.UtcNow 
                });
            }, "UnblockIpAddress");
        }

        /// <summary>
        /// Analyze threat level for an IP address
        /// </summary>
        [HttpPost("analyze-threat")]
        public async Task<IActionResult> AnalyzeThreat([FromBody] ThreatAnalysisRequest request)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                if (request == null || string.IsNullOrEmpty(request.IpAddress))
                {
                    return BadRequest("IP address is required for threat analysis");
                }

                var analysis = await _securityService.AnalyzeThreatAsync(request.IpAddress, request.UserAgent ?? "");
                return Ok(analysis);
            }, "AnalyzeThreat");
        }

        /// <summary>
        /// Check rate limit status for a user/endpoint
        /// </summary>
        [HttpGet("rate-limit/{identifier}")]
        public async Task<IActionResult> CheckRateLimit(string identifier, [FromQuery] string endpoint)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                if (string.IsNullOrEmpty(endpoint))
                {
                    return BadRequest("Endpoint parameter is required");
                }

                var rateLimitStatus = await _securityService.CheckRateLimitAsync(identifier, endpoint);
                return Ok(rateLimitStatus);
            }, "CheckRateLimit");
        }

        /// <summary>
        /// Get active user sessions
        /// </summary>
        [HttpGet("sessions/{userId}")]
        public async Task<IActionResult> GetUserSessions(string userId)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                var sessions = await _securityService.GetActiveSessionsAsync(userId);
                return Ok(sessions);
            }, "GetUserSessions");
        }

        /// <summary>
        /// Invalidate a user session
        /// </summary>
        [HttpPost("sessions/{sessionId}/invalidate")]
        public async Task<IActionResult> InvalidateSession(string sessionId)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                var success = await _securityService.InvalidateSessionAsync(sessionId);
                
                if (!success)
                {
                    return BadRequest($"Failed to invalidate session {sessionId}");
                }

                return Ok(new { 
                    Message = $"Session {sessionId} invalidated successfully", 
                    InvalidatedAt = DateTime.UtcNow 
                });
            }, "InvalidateSession");
        }

        /// <summary>
        /// Invalidate all sessions for a user
        /// </summary>
        [HttpPost("users/{userId}/invalidate-sessions")]
        public async Task<IActionResult> InvalidateAllUserSessions(string userId)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                var success = await _securityService.InvalidateAllUserSessionsAsync(userId);
                
                if (!success)
                {
                    return BadRequest($"Failed to invalidate sessions for user {userId}");
                }

                return Ok(new { 
                    Message = $"All sessions for user {userId} invalidated successfully", 
                    InvalidatedAt = DateTime.UtcNow 
                });
            }, "InvalidateAllUserSessions");
        }

        /// <summary>
        /// Generate a compliance report
        /// </summary>
        [HttpGet("compliance-report")]
        public async Task<IActionResult> GenerateComplianceReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start >= end)
                {
                    return BadRequest("Start date must be before end date");
                }

                var report = await _securityService.GenerateComplianceReportAsync(start, end);
                return Ok(report);
            }, "GenerateComplianceReport");
        }

        /// <summary>
        /// Generate a security report
        /// </summary>
        [HttpGet("security-report")]
        public async Task<IActionResult> GenerateSecurityReport(
            [FromQuery] string reportType = "summary",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                if (start >= end)
                {
                    return BadRequest("Start date must be before end date");
                }

                var report = await _securityService.GenerateSecurityReportAsync(reportType, start, end);
                return Ok(report);
            }, "GenerateSecurityReport");
        }

        /// <summary>
        /// Get password policy
        /// </summary>
        [HttpGet("password-policy")]
        public async Task<IActionResult> GetPasswordPolicy()
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                var policy = await _securityService.GetPasswordPolicyAsync();
                return Ok(policy);
            }, "GetPasswordPolicy");
        }

        /// <summary>
        /// Validate password strength
        /// </summary>
        [HttpPost("validate-password")]
        public async Task<IActionResult> ValidatePassword([FromBody] PasswordValidationRequest request)
        {
            return await ExecuteOperationAsync<IActionResult>(async () =>
            {
                if (request == null || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Password is required for validation");
                }

                var result = await _securityService.ValidatePasswordStrengthAsync(request.Password);
                return Ok(result);
            }, "ValidatePassword");
        }
    }

}