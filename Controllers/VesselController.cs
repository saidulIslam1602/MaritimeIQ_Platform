using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HavilaKystruten.Maritime.Models;
using HavilaKystruten.Maritime.Services;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselController : ControllerBase
    {
        private readonly ILogger<VesselController> _logger;
        private readonly IMaritimeDataService _maritimeData;

        public VesselController(ILogger<VesselController> logger, IMaritimeDataService maritimeData)
        {
            _logger = logger;
            _maritimeData = maritimeData;
        }

        /// <summary>
        /// Get all vessels in Havila Kystruten fleet with real-time data
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<Vessel>> GetFleet()
        {
            var fleet = _maritimeData.GetFleet();
            _logger.LogInformation($"Retrieved {fleet.Count()} vessels from Havila fleet");
            return Ok(new
            {
                TotalVessels = fleet.Count,
                FleetOperator = "Havila Kystruten AS",
                RouteNetwork = "Bergen-Kirkenes Coastal Route",
                Vessels = fleet,
                LastUpdated = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get comprehensive fleet analytics and performance metrics
        /// </summary>
        [HttpGet("analytics")]
        public ActionResult<object> GetFleetAnalytics()
        {
            var analytics = _maritimeData.GetFleetAnalytics();
            return Ok(analytics);
        }

        // Get specific vessel by ID
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

        // Get vessel current position
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

        // Update vessel status (for operations center)
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

        // Get fleet status summary (for dashboard)
        [HttpGet("status")]
        public ActionResult<FleetStatusSummary> GetFleetStatus()
        {
            var summary = _maritimeData.GetFleetStatus();
            return Ok(summary);
        }

        // Emergency alert endpoint
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