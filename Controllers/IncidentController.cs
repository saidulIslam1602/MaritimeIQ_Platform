using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;
using MaritimeIQ.Platform.Models.Incident;
using MaritimeIQ.Platform.Services;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// Incident management REST API controller
    /// Provides comprehensive incident management capabilities for SRE operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class IncidentController : BaseMaritimeController
    {
        private readonly IIncidentManagementService _incidentService;
        private readonly IOnCallService _onCallService;
        private readonly IPagerDutyService _pagerDutyService;
        private readonly TelemetryClient _telemetryClient;

        public IncidentController(
            IIncidentManagementService incidentService,
            IOnCallService onCallService,
            IPagerDutyService pagerDutyService,
            TelemetryClient telemetryClient,
            ILogger<IncidentController> logger) : base(logger)
        {
            _incidentService = incidentService;
            _onCallService = onCallService;
            _pagerDutyService = pagerDutyService;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Create a new incident
        /// </summary>
        /// <param name="request">Incident creation request</param>
        /// <returns>Created incident</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Incident), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentRequest request)
        {
            try
            {
                Logger.LogInformation("Creating incident: {Title} (Severity: {Severity})", request.Title, request.Severity);

                var incident = await _incidentService.CreateIncidentAsync(
                    request.Title,
                    request.Description,
                    request.Severity,
                    request.Category,
                    request.CustomDetails
                );

                _telemetryClient.TrackEvent("IncidentCreatedViaAPI", new Dictionary<string, string>
                {
                    ["IncidentId"] = incident.Id,
                    ["Severity"] = request.Severity.ToString(),
                    ["Category"] = request.Category.ToString(),
                    ["Source"] = "REST API"
                });

                return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, incident);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating incident: {Title}", request.Title);
                return StatusCode(500, new { error = "Failed to create incident", details = ex.Message });
            }
        }

        /// <summary>
        /// Get incident by ID
        /// </summary>
        /// <param name="id">Incident ID</param>
        /// <returns>Incident details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Incident), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetIncident(string id)
        {
            try
            {
                var incident = await _incidentService.GetIncidentAsync(id);
                if (incident == null)
                {
                    return NotFound(new { error = "Incident not found", incidentId = id });
                }

                return Ok(incident);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting incident: {IncidentId}", id);
                return StatusCode(500, new { error = "Failed to get incident", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all active incidents
        /// </summary>
        /// <returns>List of active incidents</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<Incident>), 200)]
        public async Task<IActionResult> GetActiveIncidents()
        {
            try
            {
                var incidents = await _incidentService.GetActiveIncidentsAsync();
                return Ok(incidents);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting active incidents");
                return StatusCode(500, new { error = "Failed to get active incidents", details = ex.Message });
            }
        }

        /// <summary>
        /// Get incident history with optional filtering
        /// </summary>
        /// <param name="from">Start date (optional)</param>
        /// <param name="to">End date (optional)</param>
        /// <param name="limit">Maximum number of incidents to return</param>
        /// <returns>List of incidents</returns>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<Incident>), 200)]
        public async Task<IActionResult> GetIncidentHistory(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int limit = 100)
        {
            try
            {
                var incidents = await _incidentService.GetIncidentHistoryAsync(from, to, limit);
                return Ok(incidents);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting incident history");
                return StatusCode(500, new { error = "Failed to get incident history", details = ex.Message });
            }
        }

        /// <summary>
        /// Acknowledge an incident
        /// </summary>
        /// <param name="id">Incident ID</param>
        /// <param name="request">Acknowledgment request</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/acknowledge")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AcknowledgeIncident(string id, [FromBody] AcknowledgeIncidentRequest request)
        {
            try
            {
                var success = await _incidentService.AcknowledgeIncidentAsync(id, request.AcknowledgedBy);
                if (!success)
                {
                    return NotFound(new { error = "Incident not found or already acknowledged", incidentId = id });
                }

                return Ok(new { message = "Incident acknowledged successfully", incidentId = id, acknowledgedBy = request.AcknowledgedBy });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error acknowledging incident: {IncidentId}", id);
                return StatusCode(500, new { error = "Failed to acknowledge incident", details = ex.Message });
            }
        }

        /// <summary>
        /// Update incident status
        /// </summary>
        /// <param name="id">Incident ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Success status</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateIncidentStatus(string id, [FromBody] UpdateIncidentStatusRequest request)
        {
            try
            {
                var success = await _incidentService.UpdateIncidentStatusAsync(id, request.Status, request.UpdatedBy, request.Message);
                if (!success)
                {
                    return NotFound(new { error = "Incident not found", incidentId = id });
                }

                return Ok(new { message = "Incident status updated successfully", incidentId = id, newStatus = request.Status });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating incident status: {IncidentId}", id);
                return StatusCode(500, new { error = "Failed to update incident status", details = ex.Message });
            }
        }

        /// <summary>
        /// Resolve an incident
        /// </summary>
        /// <param name="id">Incident ID</param>
        /// <param name="request">Resolution request</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/resolve")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResolveIncident(string id, [FromBody] ResolveIncidentRequest request)
        {
            try
            {
                var success = await _incidentService.ResolveIncidentAsync(id, request.ResolvedBy, request.ResolutionNote);
                if (!success)
                {
                    return NotFound(new { error = "Incident not found", incidentId = id });
                }

                return Ok(new { message = "Incident resolved successfully", incidentId = id, resolvedBy = request.ResolvedBy });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error resolving incident: {IncidentId}", id);
                return StatusCode(500, new { error = "Failed to resolve incident", details = ex.Message });
            }
        }

        /// <summary>
        /// Add update to incident timeline
        /// </summary>
        /// <param name="id">Incident ID</param>
        /// <param name="request">Update request</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/updates")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddIncidentUpdate(string id, [FromBody] AddIncidentUpdateRequest request)
        {
            try
            {
                var success = await _incidentService.AddIncidentUpdateAsync(id, request.Message, request.UpdatedBy, request.Metadata);
                if (!success)
                {
                    return NotFound(new { error = "Incident not found", incidentId = id });
                }

                return Ok(new { message = "Incident update added successfully", incidentId = id });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error adding incident update: {IncidentId}", id);
                return StatusCode(500, new { error = "Failed to add incident update", details = ex.Message });
            }
        }

        /// <summary>
        /// Get incident metrics for dashboards
        /// </summary>
        /// <param name="days">Number of days to include in metrics (default: 30)</param>
        /// <returns>Incident metrics</returns>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(IncidentMetrics), 200)]
        public async Task<IActionResult> GetIncidentMetrics([FromQuery] int days = 30)
        {
            try
            {
                var period = TimeSpan.FromDays(days);
                var metrics = await _incidentService.GetIncidentMetricsAsync(period);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting incident metrics");
                return StatusCode(500, new { error = "Failed to get incident metrics", details = ex.Message });
            }
        }

        /// <summary>
        /// Create post-mortem for resolved incident
        /// </summary>
        /// <param name="id">Incident ID</param>
        /// <param name="request">Post-mortem creation request</param>
        /// <returns>Created post-mortem</returns>
        [HttpPost("{id}/postmortem")]
        [ProducesResponseType(typeof(PostMortem), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreatePostMortem(string id, [FromBody] CreatePostMortemRequest request)
        {
            try
            {
                var postMortem = await _incidentService.CreatePostMortemAsync(id, request.CreatedBy);
                return CreatedAtAction(nameof(GetPostMortem), new { id = postMortem.Id }, postMortem);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating post-mortem for incident: {IncidentId}", id);
                return StatusCode(500, new { error = "Failed to create post-mortem", details = ex.Message });
            }
        }

        /// <summary>
        /// Get post-mortem by ID (placeholder - would need implementation)
        /// </summary>
        /// <param name="id">Post-mortem ID</param>
        /// <returns>Post-mortem details</returns>
        [HttpGet("postmortem/{id}")]
        [ProducesResponseType(typeof(PostMortem), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPostMortem(string id)
        {
            // This would need to be implemented in the service
            await Task.CompletedTask;
            return NotFound(new { error = "Post-mortem retrieval not yet implemented" });
        }

        /// <summary>
        /// Trigger maritime emergency incident
        /// </summary>
        /// <param name="request">Emergency incident request</param>
        /// <returns>Success status</returns>
        [HttpPost("emergency/maritime")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> TriggerMaritimeEmergency([FromBody] MaritimeEmergencyRequest request)
        {
            try
            {
                var success = await _incidentService.TriggerMaritimeEmergencyIncidentAsync(
                    request.VesselId, 
                    request.EmergencyType, 
                    request.Details
                );

                if (success)
                {
                    return Ok(new { message = "Maritime emergency incident triggered", vesselId = request.VesselId, emergencyType = request.EmergencyType });
                }

                return BadRequest(new { error = "Failed to trigger maritime emergency incident" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error triggering maritime emergency: {VesselId}, {EmergencyType}", request.VesselId, request.EmergencyType);
                return StatusCode(500, new { error = "Failed to trigger maritime emergency", details = ex.Message });
            }
        }

        /// <summary>
        /// Trigger environmental compliance incident
        /// </summary>
        /// <param name="request">Environmental compliance request</param>
        /// <returns>Success status</returns>
        [HttpPost("environmental/compliance")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> TriggerEnvironmentalCompliance([FromBody] EnvironmentalComplianceRequest request)
        {
            try
            {
                var success = await _incidentService.TriggerEnvironmentalComplianceIncidentAsync(
                    request.VesselId,
                    request.ViolationType,
                    request.ThresholdValue,
                    request.ActualValue
                );

                if (success)
                {
                    return Ok(new { message = "Environmental compliance incident triggered", vesselId = request.VesselId, violationType = request.ViolationType });
                }

                return BadRequest(new { error = "Failed to trigger environmental compliance incident" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error triggering environmental compliance incident: {VesselId}, {ViolationType}", request.VesselId, request.ViolationType);
                return StatusCode(500, new { error = "Failed to trigger environmental compliance incident", details = ex.Message });
            }
        }

        /// <summary>
        /// Trigger system outage incident
        /// </summary>
        /// <param name="request">System outage request</param>
        /// <returns>Success status</returns>
        [HttpPost("system/outage")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> TriggerSystemOutage([FromBody] SystemOutageRequest request)
        {
            try
            {
                var success = await _incidentService.TriggerSystemOutageIncidentAsync(
                    request.ServiceName,
                    request.OutageType,
                    request.AffectedServices
                );

                if (success)
                {
                    return Ok(new { message = "System outage incident triggered", serviceName = request.ServiceName, outageType = request.OutageType });
                }

                return BadRequest(new { error = "Failed to trigger system outage incident" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error triggering system outage incident: {ServiceName}, {OutageType}", request.ServiceName, request.OutageType);
                return StatusCode(500, new { error = "Failed to trigger system outage incident", details = ex.Message });
            }
        }

        /// <summary>
        /// Get current on-call information
        /// </summary>
        /// <returns>Current on-call team</returns>
        [HttpGet("oncall/current")]
        [ProducesResponseType(typeof(List<OnCallEngineer>), 200)]
        public async Task<IActionResult> GetCurrentOnCall()
        {
            try
            {
                var onCallTeam = await _onCallService.GetOnCallTeamAsync();
                return Ok(onCallTeam);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting current on-call team");
                return StatusCode(500, new { error = "Failed to get current on-call team", details = ex.Message });
            }
        }

        /// <summary>
        /// Test PagerDuty integration
        /// </summary>
        /// <returns>Test result</returns>
        [HttpPost("test/pagerduty")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TestPagerDuty()
        {
            try
            {
                var success = await _pagerDutyService.TestIntegrationAsync();
                if (success)
                {
                    return Ok(new { message = "PagerDuty integration test successful" });
                }

                return StatusCode(500, new { error = "PagerDuty integration test failed" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error testing PagerDuty integration");
                return StatusCode(500, new { error = "PagerDuty integration test failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Test escalation chain
        /// </summary>
        /// <returns>Test result</returns>
        [HttpPost("test/escalation")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TestEscalation()
        {
            try
            {
                var success = await _onCallService.TestEscalationAsync();
                if (success)
                {
                    return Ok(new { message = "Escalation chain test successful" });
                }

                return StatusCode(500, new { error = "Escalation chain test failed" });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error testing escalation chain");
                return StatusCode(500, new { error = "Escalation chain test failed", details = ex.Message });
            }
        }
    }

    // Request/Response DTOs
    public class CreateIncidentRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IncidentSeverity Severity { get; set; }
        public IncidentCategory Category { get; set; }
        public Dictionary<string, object>? CustomDetails { get; set; }
    }

    public class AcknowledgeIncidentRequest
    {
        public string AcknowledgedBy { get; set; } = string.Empty;
    }

    public class UpdateIncidentStatusRequest
    {
        public IncidentStatus Status { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class ResolveIncidentRequest
    {
        public string ResolvedBy { get; set; } = string.Empty;
        public string ResolutionNote { get; set; } = string.Empty;
    }

    public class AddIncidentUpdateRequest
    {
        public string Message { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class CreatePostMortemRequest
    {
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class MaritimeEmergencyRequest
    {
        public string VesselId { get; set; } = string.Empty;
        public string EmergencyType { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
    }

    public class EnvironmentalComplianceRequest
    {
        public string VesselId { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty;
        public double ThresholdValue { get; set; }
        public double ActualValue { get; set; }
    }

    public class SystemOutageRequest
    {
        public string ServiceName { get; set; } = string.Empty;
        public string OutageType { get; set; } = string.Empty;
        public List<string> AffectedServices { get; set; } = new();
    }
}
