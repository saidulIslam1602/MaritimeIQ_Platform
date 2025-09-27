using Microsoft.AspNetCore.Mvc;
using HavilaKystruten.Maritime.Models;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SafetyController : ControllerBase
    {
        private readonly ILogger<SafetyController> _logger;

        public SafetyController(ILogger<SafetyController> logger)
        {
            _logger = logger;
        }

        // Get safety status for all vessels
        [HttpGet("fleet-status")]
        public ActionResult<FleetSafetyStatus> GetFleetSafetyStatus()
        {
            var vessels = GetVesselSafetyData();
            var status = new FleetSafetyStatus
            {
                TotalVessels = vessels.Count,
                SafeVessels = vessels.Count(v => v.SafetyLevel == SafetyLevel.Safe),
                CautionVessels = vessels.Count(v => v.SafetyLevel == SafetyLevel.Caution),
                CriticalVessels = vessels.Count(v => v.SafetyLevel == SafetyLevel.Critical),
                ActiveAlerts = vessels.SelectMany(v => v.ActiveAlerts).ToList(),
                LastUpdated = DateTime.UtcNow
            };

            return Ok(status);
        }

        // Get safety data for specific vessel
        [HttpGet("vessel/{vesselId}")]
        public ActionResult<VesselSafetyData> GetVesselSafety(int vesselId)
        {
            var vesselSafety = GetVesselSafetyData().FirstOrDefault(v => v.VesselId == vesselId);
            if (vesselSafety == null)
            {
                return NotFound($"Safety data not found for vessel {vesselId}");
            }

            return Ok(vesselSafety);
        }

        // Report safety incident
        [HttpPost("incident")]
        public ActionResult<IncidentResponse> ReportIncident([FromBody] IncidentReport report)
        {
            var incidentId = Guid.NewGuid();
            
            _logger.LogWarning($"Safety incident reported: {report.Type} on vessel {report.VesselId} - {report.Description}");
            
            // In real system: trigger safety protocols, notify authorities
            var response = new IncidentResponse
            {
                IncidentId = incidentId,
                Status = "Received",
                AssignedTo = "Safety Officer",
                EstimatedResponse = TimeSpan.FromMinutes(15),
                Instructions = GetSafetyInstructions(report.Type),
                ReportedAt = DateTime.UtcNow
            };

            return Ok(response);
        }

        // Get weather alerts for route
        [HttpGet("weather-alerts/{routeId}")]
        public ActionResult<List<WeatherAlert>> GetWeatherAlerts(int routeId)
        {
            var alerts = GetActiveWeatherAlerts().Where(a => a.AffectedRoutes.Contains(routeId)).ToList();
            return Ok(alerts);
        }

        // Get navigation hazards
        [HttpGet("navigation-hazards")]
        public ActionResult<List<NavigationHazard>> GetNavigationHazards()
        {
            var hazards = GetActiveNavigationHazards();
            return Ok(hazards);
        }

        // Emergency muster drill
        [HttpPost("muster-drill/{vesselId}")]
        public ActionResult<MusterDrillResult> ConductMusterDrill(int vesselId)
        {
            _logger.LogInformation($"Muster drill initiated on vessel {vesselId}");
            
            var result = new MusterDrillResult
            {
                VesselId = vesselId,
                DrillType = "Emergency Muster",
                StartTime = DateTime.UtcNow,
                ExpectedDuration = TimeSpan.FromMinutes(20),
                Status = "In Progress",
                ParticipantsExpected = 150,
                Instructions = new List<string>
                {
                    "All passengers proceed to muster stations",
                    "Crew report to assigned emergency positions",
                    "Life jackets to be worn by all personnel",
                    "Await further instructions from bridge"
                }
            };

            return Ok(result);
        }

        // Mock vessel safety data
        private static List<VesselSafetyData> GetVesselSafetyData()
        {
            return new List<VesselSafetyData>
            {
                new VesselSafetyData
                {
                    VesselId = 1,
                    VesselName = "MS Havila Castor",
                    SafetyLevel = SafetyLevel.Safe,
                    ActiveAlerts = new List<SafetyAlert>(),
                    LastSafetyInspection = DateTime.UtcNow.AddDays(-30),
                    NextSafetyInspection = DateTime.UtcNow.AddDays(60),
                    SafetyEquipment = new SafetyEquipment
                    {
                        LifeBoats = 6,
                        LifeRafts = 12,
                        LifeJackets = 800,
                        FireExtinguishers = 45,
                        LastEquipmentCheck = DateTime.UtcNow.AddDays(-7)
                    },
                    CrewCertifications = new CrewCertifications
                    {
                        CaptainLicense = DateTime.UtcNow.AddYears(2),
                        SafetyCertificates = DateTime.UtcNow.AddMonths(6),
                        MedicalCertificates = DateTime.UtcNow.AddMonths(3)
                    }
                },
                new VesselSafetyData
                {
                    VesselId = 2,
                    VesselName = "MS Havila Pollux",
                    SafetyLevel = SafetyLevel.Caution,
                    ActiveAlerts = new List<SafetyAlert>
                    {
                        new SafetyAlert
                        {
                            Id = Guid.NewGuid(),
                            Type = "Equipment",
                            Severity = AlertSeverity.Medium,
                            Message = "Port lifeboat winch requires maintenance",
                            Timestamp = DateTime.UtcNow.AddHours(-2)
                        }
                    }
                }
            };
        }

        private static List<WeatherAlert> GetActiveWeatherAlerts()
        {
            return new List<WeatherAlert>
            {
                new WeatherAlert
                {
                    Id = Guid.NewGuid(),
                    Type = "Storm Warning",
                    Severity = AlertSeverity.High,
                    Description = "Storm system approaching Norwegian Sea with winds up to 50 knots",
                    AffectedAreas = new List<string> { "Norwegian Sea", "Lofoten Islands" },
                    AffectedRoutes = new List<int> { 1, 2 },
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddHours(18),
                    IssuedBy = "Norwegian Meteorological Institute"
                }
            };
        }

        private static List<NavigationHazard> GetActiveNavigationHazards()
        {
            return new List<NavigationHazard>
            {
                new NavigationHazard
                {
                    Id = Guid.NewGuid(),
                    Type = "Ice Formation",
                    Description = "Sea ice reported in approaches to Kirkenes",
                    Position = new Position { Latitude = 69.7, Longitude = 30.0 },
                    Severity = AlertSeverity.Medium,
                    ReportedAt = DateTime.UtcNow.AddHours(-6),
                    ValidUntil = DateTime.UtcNow.AddDays(3)
                }
            };
        }

        private static List<string> GetSafetyInstructions(string incidentType)
        {
            return incidentType.ToLower() switch
            {
                "fire" => new List<string>
                {
                    "Activate fire alarm immediately",
                    "Notify bridge and engine room",
                    "Use appropriate fire suppression system",
                    "Evacuate area if necessary"
                },
                "medical" => new List<string>
                {
                    "Provide immediate first aid",
                    "Contact ship's medical officer",
                    "Prepare for potential helicopter evacuation",
                    "Notify nearest coast guard station"
                },
                "man overboard" => new List<string>
                {
                    "Sound man overboard alarm",
                    "Deploy life ring immediately",
                    "Mark GPS position",
                    "Execute man overboard maneuver"
                },
                _ => new List<string> { "Follow standard emergency procedures", "Notify bridge immediately" }
            };
        }
    }

    // Safety-related models
    public class VesselSafetyData
    {
        public int VesselId { get; set; }
        public string VesselName { get; set; } = string.Empty;
        public SafetyLevel SafetyLevel { get; set; }
        public List<SafetyAlert> ActiveAlerts { get; set; } = new List<SafetyAlert>();
        public DateTime LastSafetyInspection { get; set; }
        public DateTime NextSafetyInspection { get; set; }
        public SafetyEquipment SafetyEquipment { get; set; } = new SafetyEquipment();
        public CrewCertifications CrewCertifications { get; set; } = new CrewCertifications();
    }

    public enum SafetyLevel
    {
        Safe,
        Caution,
        Critical
    }

    public class SafetyAlert
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public enum AlertSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class FleetSafetyStatus
    {
        public int TotalVessels { get; set; }
        public int SafeVessels { get; set; }
        public int CautionVessels { get; set; }
        public int CriticalVessels { get; set; }
        public List<SafetyAlert> ActiveAlerts { get; set; } = new List<SafetyAlert>();
        public DateTime LastUpdated { get; set; }
    }

    public class IncidentReport
    {
        public int VesselId { get; set; }
        public string Type { get; set; } = string.Empty; // "Fire", "Medical", "Collision", etc.
        public string Description { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public string ReportedBy { get; set; } = string.Empty;
        public Position? Location { get; set; }
    }

    public class IncidentResponse
    {
        public Guid IncidentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public TimeSpan EstimatedResponse { get; set; }
        public List<string> Instructions { get; set; } = new List<string>();
        public DateTime ReportedAt { get; set; }
    }

    public class WeatherAlert
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> AffectedAreas { get; set; } = new List<string>();
        public List<int> AffectedRoutes { get; set; } = new List<int>();
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string IssuedBy { get; set; } = string.Empty;
    }

    public class NavigationHazard
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Position Position { get; set; } = new Position();
        public AlertSeverity Severity { get; set; }
        public DateTime ReportedAt { get; set; }
        public DateTime ValidUntil { get; set; }
    }

    public class SafetyEquipment
    {
        public int LifeBoats { get; set; }
        public int LifeRafts { get; set; }
        public int LifeJackets { get; set; }
        public int FireExtinguishers { get; set; }
        public DateTime LastEquipmentCheck { get; set; }
    }

    public class CrewCertifications
    {
        public DateTime CaptainLicense { get; set; }
        public DateTime SafetyCertificates { get; set; }
        public DateTime MedicalCertificates { get; set; }
    }

    public class MusterDrillResult
    {
        public int VesselId { get; set; }
        public string DrillType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan ExpectedDuration { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ParticipantsExpected { get; set; }
        public List<string> Instructions { get; set; } = new List<string>();
    }
}