using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MaritimeIQ.Platform.Models;
using MaritimeIQ.Platform.Services;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// Vessel Management Controller for MaritimeIQ Platform
    /// 
    /// This controller provides comprehensive vessel management capabilities including:
    /// - Real-time vessel tracking and position monitoring
    /// - Fleet analytics and performance metrics
    /// - Vessel status management and updates
    /// - Emergency response and alert handling
    /// - Fleet status monitoring for operational dashboards
    /// 
    /// All endpoints are designed for maritime operations with real-time data requirements
    /// and support for emergency response scenarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VesselController : ControllerBase
    {
        private readonly ILogger<VesselController> _logger;
        private readonly IMaritimeDataService _maritimeData;

        /// <summary>
        /// Initializes a new instance of the VesselController
        /// </summary>
        /// <param name="logger">Logger instance for operation tracking and error handling</param>
        /// <param name="maritimeData">Service for accessing maritime data and vessel information</param>
        public VesselController(ILogger<VesselController> logger, IMaritimeDataService maritimeData)
        {
            _logger = logger;
            _maritimeData = maritimeData;
        }

        /// <summary>
        /// Retrieves the complete maritime fleet with real-time operational data
        /// 
        /// This endpoint provides comprehensive fleet information including:
        /// - Current vessel positions and status
        /// - Fleet composition and operational metrics
        /// - Real-time data timestamps for dashboard synchronization
        /// 
        /// Used by: Fleet management dashboards, operational centers, and monitoring systems
        /// </summary>
        /// <returns>Complete fleet data with metadata and operational context</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Vessel>> GetFleet()
        {
            var fleet = _maritimeData.GetFleet();
            _logger.LogInformation($"Retrieved {fleet.Count()} vessels from maritime fleet");
            return Ok(new
            {
                TotalVessels = fleet.Count,
                FleetOperator = "Maritime Operations AS",
                RouteNetwork = "Bergen-Kirkenes Coastal Route",
                Vessels = fleet,
                LastUpdated = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Retrieves comprehensive fleet analytics and performance metrics
        /// 
        /// Provides detailed analytics including:
        /// - Fleet performance KPIs and trends
        /// - Route efficiency and timing analysis
        /// - Fuel consumption and environmental metrics
        /// - Passenger satisfaction and operational statistics
        /// 
        /// Used by: Business intelligence dashboards, management reporting, and strategic planning
        /// </summary>
        /// <returns>Comprehensive analytics data with performance metrics</returns>
        [HttpGet("analytics")]
        public ActionResult<object> GetFleetAnalytics()
        {
            var analytics = _maritimeData.GetFleetAnalytics();
            return Ok(analytics);
        }

        /// <summary>
        /// Retrieves detailed information for a specific vessel by ID
        /// 
        /// Returns comprehensive vessel data including:
        /// - Technical specifications and capabilities
        /// - Current operational status and location
        /// - Maintenance history and compliance status
        /// - Crew information and operational metrics
        /// 
        /// Used by: Vessel detail views, operational planning, and maintenance scheduling
        /// </summary>
        /// <param name="id">Unique vessel identifier</param>
        /// <returns>Complete vessel information or 404 if not found</returns>
        [HttpGet("{id}")]
        public ActionResult<Vessel> GetVessel(int id)
        {
            var vessel = _maritimeData.GetVessel(id);
            if (vessel == null)
            {
                _logger.LogWarning($"Vessel with ID {id} not found");
                return NotFound($"Vessel with ID {id} not found");
            }

            _logger.LogInformation($"Retrieved vessel: {vessel.Name}");
            return Ok(vessel);
        }

        /// <summary>
        /// Retrieves real-time position data for a specific vessel
        /// 
        /// Provides current position information including:
        /// - GPS coordinates (latitude/longitude)
        /// - Speed, heading, and course over ground
        /// - Position accuracy and data quality indicators
        /// - Timestamp of last position update
        /// 
        /// Used by: Real-time tracking systems, navigation displays, and AIS integration
        /// </summary>
        /// <param name="id">Unique vessel identifier</param>
        /// <returns>Current position data or 404 if vessel not found</returns>
        [HttpGet("{id}/position")]
        public ActionResult<Position> GetVesselPosition(int id)
        {
            var position = _maritimeData.GetVesselPosition(id);
            if (position == null)
            {
                return NotFound($"Position data not available for vessel {id}");
            }

            return Ok(position);
        }

        /// <summary>
        /// Updates the operational status of a specific vessel
        /// 
        /// Allows operations center to update vessel status including:
        /// - Operational status changes (Active, Maintenance, Out of Service)
        /// - Status notes and operational comments
        /// - Timestamp tracking for status changes
        /// - Integration with fleet management systems
        /// 
        /// Used by: Operations center, fleet management, and maintenance scheduling systems
        /// </summary>
        /// <param name="id">Unique vessel identifier</param>
        /// <param name="statusUpdate">Status update request with new status and notes</param>
        /// <returns>Confirmation of status update or 404 if vessel not found</returns>
        [HttpPut("{id}/status")]
        public ActionResult UpdateVesselStatus(int id, [FromBody] VesselStatusUpdateRequest statusUpdate)
        {
            if (!_maritimeData.TryUpdateVesselStatus(id, statusUpdate.Status, statusUpdate.Notes, out var updatedVessel))
            {
                return NotFound($"Vessel with ID {id} not found");
            }

            _logger.LogInformation($"Updated vessel {updatedVessel.Name} status to {statusUpdate.Status}");

            return Ok(new { 
                Message = $"Vessel {updatedVessel.Name} status updated to {statusUpdate.Status}",
                Timestamp = updatedVessel.LastUpdated
            });
        }

        /// <summary>
        /// Retrieves comprehensive fleet status summary for operational dashboards
        /// 
        /// Provides high-level fleet overview including:
        /// - Total vessels and operational status distribution
        /// - Current route coverage and service status
        /// - Performance metrics and key indicators
        /// - Real-time operational alerts and notifications
        /// 
        /// Used by: Fleet management dashboards, operational centers, and executive reporting
        /// </summary>
        /// <returns>Complete fleet status summary with operational metrics</returns>
        [HttpGet("status")]
        public ActionResult<FleetStatusSummary> GetFleetStatus()
        {
            var summary = _maritimeData.GetFleetStatus();
            return Ok(summary);
        }

        /// <summary>
        /// Handles emergency declarations and critical incident reporting
        /// 
        /// Critical endpoint for emergency response including:
        /// - Emergency type classification and severity assessment
        /// - Automatic alert distribution to emergency services
        /// - Real-time emergency response coordination
        /// - Integration with coast guard and rescue services
        /// 
        /// Used by: Vessel crews, emergency response systems, and maritime safety authorities
        /// </summary>
        /// <param name="id">Unique vessel identifier</param>
        /// <param name="report">Emergency report with incident details and severity</param>
        /// <returns>Emergency response confirmation and tracking information</returns>
        [HttpPost("{id}/emergency")]
        public ActionResult<VesselEmergencyResponse> ReportEmergency(int id, [FromBody] VesselEmergencyReport report)
        {
            try
            {
                var response = _maritimeData.DeclareEmergency(id, report);
                _logger.LogCritical($"EMERGENCY: {report.Type} reported on vessel {response.VesselName} - {report.Description}");
                return Ok(response);
            }
            catch (ArgumentException)
            {
                return NotFound($"Vessel with ID {id} not found");
            }
        }
    }
}