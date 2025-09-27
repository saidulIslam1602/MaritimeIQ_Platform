using Microsoft.AspNetCore.Mvc;
using HavilaKystruten.Maritime.Models;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselController : ControllerBase
    {
        private readonly ILogger<VesselController> _logger;

        public VesselController(ILogger<VesselController> logger)
        {
            _logger = logger;
        }

        // Get all vessels in the fleet
        [HttpGet]
        public ActionResult<IEnumerable<Vessel>> GetFleet()
        {
            var fleet = GetHavilaFleet();
            _logger.LogInformation($"Retrieved {fleet.Count()} vessels from fleet");
            return Ok(fleet);
        }

        // Get specific vessel by ID
        [HttpGet("{id}")]
        public ActionResult<Vessel> GetVessel(int id)
        {
            var vessel = GetHavilaFleet().FirstOrDefault(v => v.Id == id);
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
            var vessel = GetHavilaFleet().FirstOrDefault(v => v.Id == id);
            if (vessel?.CurrentPosition == null)
            {
                return NotFound($"Position data not available for vessel {id}");
            }

            return Ok(vessel.CurrentPosition);
        }

        // Update vessel status (for operations center)
        [HttpPut("{id}/status")]
        public ActionResult UpdateVesselStatus(int id, [FromBody] VesselStatusUpdate statusUpdate)
        {
            var vessel = GetHavilaFleet().FirstOrDefault(v => v.Id == id);
            if (vessel == null)
            {
                return NotFound($"Vessel with ID {id} not found");
            }

            vessel.Status = statusUpdate.Status;
            vessel.LastUpdated = DateTime.UtcNow;
            
            _logger.LogInformation($"Updated vessel {vessel.Name} status to {statusUpdate.Status}");
            
            return Ok(new { 
                Message = $"Vessel {vessel.Name} status updated to {statusUpdate.Status}",
                Timestamp = vessel.LastUpdated
            });
        }

        // Get fleet status summary (for dashboard)
        [HttpGet("status")]
        public ActionResult<FleetStatusSummary> GetFleetStatus()
        {
            var fleet = GetHavilaFleet();
            var summary = new FleetStatusSummary
            {
                TotalVessels = fleet.Count(),
                InService = fleet.Count(v => v.Status == VesselStatus.InService),
                InPort = fleet.Count(v => v.Status == VesselStatus.InPort),
                Maintenance = fleet.Count(v => v.Status == VesselStatus.Maintenance),
                Emergency = fleet.Count(v => v.Status == VesselStatus.Emergency),
                LastUpdated = DateTime.UtcNow
            };

            return Ok(summary);
        }

        // Emergency alert endpoint
        [HttpPost("{id}/emergency")]
        public ActionResult<EmergencyResponse> ReportEmergency(int id, [FromBody] EmergencyReport report)
        {
            var vessel = GetHavilaFleet().FirstOrDefault(v => v.Id == id);
            if (vessel == null)
            {
                return NotFound($"Vessel with ID {id} not found");
            }

            vessel.Status = VesselStatus.Emergency;
            vessel.LastUpdated = DateTime.UtcNow;

            _logger.LogCritical($"EMERGENCY: {report.Type} reported on vessel {vessel.Name} - {report.Description}");

            // In real system: trigger emergency protocols, notify coast guard, etc.
            
            return Ok(new EmergencyResponse
            {
                EmergencyId = Guid.NewGuid(),
                VesselName = vessel.Name,
                Acknowledged = true,
                Response = "Emergency services have been notified. Coast Guard alerted.",
                Timestamp = DateTime.UtcNow
            });
        }

        // Mock Havila Kystruten fleet data
        private static IEnumerable<Vessel> GetHavilaFleet()
        {
            return new List<Vessel>
            {
                new Vessel
                {
                    Id = 1,
                    Name = "MS Havila Castor",
                    IMONumber = "9781782",
                    CallSign = "LAJB2",
                    Type = VesselType.PassengerFerry,
                    PassengerCapacity = 640,
                    CrewCapacity = 90,
                    LengthMeters = 123.0,
                    BeamMeters = 19.5,
                    DraftMeters = 5.1,
                    GrossTonnage = 15500,
                    Status = VesselStatus.InService,
                    LastUpdated = DateTime.UtcNow.AddMinutes(-5),
                    CurrentPosition = new Position
                    {
                        Latitude = 69.6496,  // Near Tromsø
                        Longitude = 18.9553,
                        SpeedKnots = 14.5,
                        HeadingDegrees = 045,
                        Timestamp = DateTime.UtcNow.AddMinutes(-2)
                    },
                    CurrentRouteId = 1
                },
                new Vessel
                {
                    Id = 2,
                    Name = "MS Havila Pollux",
                    IMONumber = "9781794",
                    CallSign = "LAJB3",
                    Type = VesselType.PassengerFerry,
                    PassengerCapacity = 640,
                    CrewCapacity = 90,
                    LengthMeters = 123.0,
                    BeamMeters = 19.5,
                    DraftMeters = 5.1,
                    GrossTonnage = 15500,
                    Status = VesselStatus.InPort,
                    LastUpdated = DateTime.UtcNow.AddHours(-1),
                    CurrentPosition = new Position
                    {
                        Latitude = 60.3913,  // Bergen
                        Longitude = 5.3221,
                        SpeedKnots = 0,
                        HeadingDegrees = 0,
                        Timestamp = DateTime.UtcNow.AddHours(-1)
                    }
                }
            };
        }
    }

    // Supporting models for API operations
    public class VesselStatusUpdate
    {
        public VesselStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class FleetStatusSummary
    {
        public int TotalVessels { get; set; }
        public int InService { get; set; }
        public int InPort { get; set; }
        public int Maintenance { get; set; }
        public int Emergency { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class EmergencyReport
    {
        public string Type { get; set; } = string.Empty; // "Medical", "Fire", "Collision", etc.
        public string Description { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
    }

    public class EmergencyResponse
    {
        public Guid EmergencyId { get; set; }
        public string VesselName { get; set; } = string.Empty;
        public bool Acknowledged { get; set; }
        public string Response { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}