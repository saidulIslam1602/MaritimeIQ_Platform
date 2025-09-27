using Microsoft.AspNetCore.Mvc;
using HavilaKystruten.Maritime.Models;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly ILogger<RouteController> _logger;

        public RouteController(ILogger<RouteController> logger)
        {
            _logger = logger;
        }

        // Get all available routes
        [HttpGet]
        public ActionResult<IEnumerable<Models.Route>> GetRoutes()
        {
            var routes = GetNorwegianCoastalRoutes();
            return Ok(routes);
        }

        // Get specific route with ports
        [HttpGet("{id}")]
        public ActionResult<Models.Route> GetRoute(int id)
        {
            var route = GetNorwegianCoastalRoutes().FirstOrDefault(r => r.Id == id);
            if (route == null)
            {
                return NotFound($"Route with ID {id} not found");
            }

            return Ok(route);
        }

        // Calculate route between two ports
        [HttpPost("calculate")]
        public ActionResult<RouteCalculation> CalculateRoute([FromBody] RouteRequest request)
        {
            var ports = GetNorwegianPorts();
            var fromPort = ports.FirstOrDefault(p => p.Id == request.FromPortId);
            var toPort = ports.FirstOrDefault(p => p.Id == request.ToPortId);

            if (fromPort == null || toPort == null)
            {
                return BadRequest("Invalid port IDs provided");
            }

            // Simple distance calculation (in real system: use maritime routing algorithms)
            var distance = CalculateNauticalMiles(
                fromPort.Position.Latitude, fromPort.Position.Longitude,
                toPort.Position.Latitude, toPort.Position.Longitude);

            var calculation = new RouteCalculation
            {
                FromPort = fromPort.Name,
                ToPort = toPort.Name,
                DistanceNauticalMiles = distance,
                EstimatedDuration = TimeSpan.FromHours(distance / 14), // Assuming 14 knots average speed
                EstimatedFuelCost = distance * 12.5, // NOK per nautical mile
                WeatherWarnings = GetWeatherWarnings(fromPort, toPort),
                CalculatedAt = DateTime.UtcNow
            };

            return Ok(calculation);
        }

        // Get port information
        [HttpGet("ports")]
        public ActionResult<IEnumerable<Port>> GetPorts()
        {
            return Ok(GetNorwegianPorts());
        }

        // Get port by ID with current conditions
        [HttpGet("ports/{id}")]
        public ActionResult<PortInfo> GetPortInfo(int id)
        {
            var port = GetNorwegianPorts().FirstOrDefault(p => p.Id == id);
            if (port == null)
            {
                return NotFound($"Port with ID {id} not found");
            }

            var portInfo = new PortInfo
            {
                Port = port,
                CurrentWeather = GetPortWeather(port),
                AvailableTerminals = port.Terminals.Where(t => t.IsAvailable).ToList(),
                NextDepartures = GetNextDepartures(port),
                LastUpdated = DateTime.UtcNow
            };

            return Ok(portInfo);
        }

        // Mock Norwegian coastal ports
        private static List<Port> GetNorwegianPorts()
        {
            return new List<Port>
            {
                new Port
                {
                    Id = 1,
                    Name = "Bergen",
                    UNLocode = "NOBGO",
                    Position = new Position { Latitude = 60.3913, Longitude = 5.3221 },
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    MaxVesselLength = 200,
                    MaxDraft = 8.0,
                    Terminals = new List<Terminal>
                    {
                        new Terminal { Id = 1, Name = "Passenger Terminal", Type = TerminalType.Passenger, IsAvailable = true },
                        new Terminal { Id = 2, Name = "Vehicle Deck", Type = TerminalType.Vehicle, IsAvailable = true }
                    }
                },
                new Port
                {
                    Id = 2,
                    Name = "Tromsø",
                    UNLocode = "NOTRD",
                    Position = new Position { Latitude = 69.6496, Longitude = 18.9553 },
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    MaxVesselLength = 150,
                    MaxDraft = 7.0,
                    Terminals = new List<Terminal>
                    {
                        new Terminal { Id = 3, Name = "Prostneset Terminal", Type = TerminalType.Passenger, IsAvailable = true },
                        new Terminal { Id = 4, Name = "Cargo Berth", Type = TerminalType.Cargo, IsAvailable = false }
                    }
                },
                new Port
                {
                    Id = 3,
                    Name = "Kirkenes",
                    UNLocode = "NOKIR",
                    Position = new Position { Latitude = 69.7267, Longitude = 30.0458 },
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    MaxVesselLength = 140,
                    MaxDraft = 6.5,
                    Terminals = new List<Terminal>
                    {
                        new Terminal { Id = 5, Name = "Kirkenes Port", Type = TerminalType.Passenger, IsAvailable = true }
                    }
                }
            };
        }

        // Mock Norwegian coastal routes
        private static List<Models.Route> GetNorwegianCoastalRoutes()
        {
            var ports = GetNorwegianPorts();
            return new List<Models.Route>
            {
                new Models.Route
                {
                    Id = 1,
                    Name = "Bergen-Kirkenes Northbound",
                    Description = "Classic Norwegian coastal voyage from Bergen to Kirkenes",
                    Ports = ports,
                    EstimatedDuration = TimeSpan.FromDays(6),
                    DistanceNauticalMiles = 1485,
                    IsActive = true,
                    Type = RouteType.CoastalRoute
                },
                new Models.Route
                {
                    Id = 2,
                    Name = "Kirkenes-Bergen Southbound",
                    Description = "Return journey from Kirkenes to Bergen",
                    Ports = ports.AsEnumerable().Reverse().ToList(),
                    EstimatedDuration = TimeSpan.FromDays(5.5),
                    DistanceNauticalMiles = 1485,
                    IsActive = true,
                    Type = RouteType.CoastalRoute
                }
            };
        }

        // Distance calculation between two geographic points
        private static double CalculateNauticalMiles(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 3440.065; // Earth's radius in nautical miles
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static List<string> GetWeatherWarnings(Port from, Port to)
        {
            // Mock weather warnings based on location
            var warnings = new List<string>();
            
            if (from.Position.Latitude > 70 || to.Position.Latitude > 70)
            {
                warnings.Add("Arctic weather conditions - ice advisory in effect");
            }
            
            if (DateTime.UtcNow.Month >= 10 || DateTime.UtcNow.Month <= 3)
            {
                warnings.Add("Winter storm season - monitor weather closely");
            }

            return warnings;
        }

        private static WeatherCondition GetPortWeather(Port port)
        {
            // Mock weather data
            return new WeatherCondition
            {
                Temperature = port.Position.Latitude > 70 ? -5 : 8,
                WindSpeedKnots = 15,
                WaveHeightMeters = 1.5,
                VisibilityKm = 8,
                Conditions = "Partly cloudy"
            };
        }

        private static List<Departure> GetNextDepartures(Port port)
        {
            return new List<Departure>
            {
                new Departure
                {
                    VesselName = "MS Havila Castor",
                    Destination = "Kirkenes",
                    DepartureTime = DateTime.UtcNow.AddHours(2),
                    EstimatedArrival = DateTime.UtcNow.AddDays(6)
                }
            };
        }
    }

    // Supporting models for route operations
    public class RouteRequest
    {
        public int FromPortId { get; set; }
        public int ToPortId { get; set; }
        public DateTime PreferredDeparture { get; set; }
    }

    public class RouteCalculation
    {
        public string FromPort { get; set; } = string.Empty;
        public string ToPort { get; set; } = string.Empty;
        public double DistanceNauticalMiles { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public double EstimatedFuelCost { get; set; }
        public List<string> WeatherWarnings { get; set; } = new List<string>();
        public DateTime CalculatedAt { get; set; }
    }

    public class PortInfo
    {
        public Port Port { get; set; } = new Port();
        public WeatherCondition CurrentWeather { get; set; } = new WeatherCondition();
        public List<Terminal> AvailableTerminals { get; set; } = new List<Terminal>();
        public List<Departure> NextDepartures { get; set; } = new List<Departure>();
        public DateTime LastUpdated { get; set; }
    }

    public class WeatherCondition
    {
        public double Temperature { get; set; }
        public double WindSpeedKnots { get; set; }
        public double WaveHeightMeters { get; set; }
        public double VisibilityKm { get; set; }
        public string Conditions { get; set; } = string.Empty;
    }

    public class Departure
    {
        public string VesselName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime EstimatedArrival { get; set; }
    }
}