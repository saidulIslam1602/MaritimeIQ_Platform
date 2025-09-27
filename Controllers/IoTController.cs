using Microsoft.AspNetCore.Mvc;
using HavilaKystruten.Maritime.Models;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IoTController : ControllerBase
    {
        private readonly ILogger<IoTController> _logger;

        public IoTController(ILogger<IoTController> logger)
        {
            _logger = logger;
        }

        // Get sensor data from vessel
        [HttpGet("vessel/{vesselId}/sensors")]
        public ActionResult<VesselSensorData> GetVesselSensors(int vesselId)
        {
            var sensorData = GetMockSensorData(vesselId);
            if (sensorData == null)
            {
                return NotFound($"Sensor data not available for vessel {vesselId}");
            }

            return Ok(sensorData);
        }

        // Receive sensor telemetry from vessel IoT systems
        [HttpPost("telemetry")]
        public ActionResult ReceiveTelemetry([FromBody] TelemetryData telemetry)
        {
            _logger.LogInformation($"Received telemetry from vessel {telemetry.VesselId}: {telemetry.SensorReadings.Count} readings");
            
            // In real system: store in time-series database, trigger alerts if needed
            var alerts = ProcessTelemetryAlerts(telemetry);
            
            if (alerts.Any())
            {
                _logger.LogWarning($"Telemetry alerts triggered for vessel {telemetry.VesselId}");
            }

            return Ok(new TelemetryResponse
            {
                Received = true,
                ProcessedAt = DateTime.UtcNow,
                AlertsTriggered = alerts.Count,
                NextExpectedTelemetry = DateTime.UtcNow.AddMinutes(5)
            });
        }

        // Get weather data from external weather services
        [HttpGet("weather/{latitude}/{longitude}")]
        public ActionResult<WeatherData> GetWeatherData(double latitude, double longitude)
        {
            // In real system: integrate with Norwegian Meteorological Institute API
            var weather = new WeatherData
            {
                Location = new Position { Latitude = latitude, Longitude = longitude },
                Temperature = GetTemperatureForLocation(latitude),
                WindSpeed = 12.5,
                WindDirection = 225,
                WaveHeight = 2.1,
                Visibility = 15.0,
                Precipitation = 0,
                BarometricPressure = 1013.25,
                Conditions = "Partly cloudy",
                Timestamp = DateTime.UtcNow,
                Source = "Norwegian Meteorological Institute"
            };

            return Ok(weather);
        }

        // AIS (Automatic Identification System) data
        [HttpGet("ais/vessel/{vesselId}")]
        public ActionResult<AISData> GetAISData(int vesselId)
        {
            var aisData = new AISData
            {
                VesselId = vesselId,
                MMSI = 259123000 + vesselId, // Norwegian MMSI range
                Position = new Position 
                { 
                    Latitude = 69.6496, 
                    Longitude = 18.9553,
                    SpeedKnots = 14.2,
                    HeadingDegrees = 045,
                    Timestamp = DateTime.UtcNow.AddMinutes(-1)
                },
                NavigationStatus = NavigationStatus.UnderWayUsingEngine,
                RateOfTurn = 0,
                Destination = "KIRKENES",
                ETA = DateTime.UtcNow.AddDays(3),
                LastUpdate = DateTime.UtcNow
            };

            return Ok(aisData);
        }

        // External system integrations
        [HttpGet("integrations/port-status/{portId}")]
        public ActionResult<PortSystemStatus> GetPortSystemStatus(int portId)
        {
            // Integration with port management systems
            var status = new PortSystemStatus
            {
                PortId = portId,
                PortName = GetPortName(portId),
                BerthAvailability = new List<BerthStatus>
                {
                    new BerthStatus { BerthNumber = "A1", IsAvailable = true, VesselName = null },
                    new BerthStatus { BerthNumber = "A2", IsAvailable = false, VesselName = "MS Havila Castor" },
                    new BerthStatus { BerthNumber = "B1", IsAvailable = true, VesselName = null }
                },
                TideInfo = new TideInfo
                {
                    CurrentHeight = 2.1,
                    NextHighTide = DateTime.UtcNow.AddHours(3),
                    NextLowTide = DateTime.UtcNow.AddHours(9),
                    MaxHeight = 3.2,
                    MinHeight = 0.4
                },
                Services = new PortServices
                {
                    FuelAvailable = true,
                    FreshWaterAvailable = true,
                    WasteDisposalAvailable = true,
                    CargoHandlingAvailable = true,
                    PilotServiceRequired = portId == 3 // Kirkenes requires pilot
                },
                LastUpdated = DateTime.UtcNow
            };

            return Ok(status);
        }

        // Fuel monitoring and optimization
        [HttpGet("fuel/vessel/{vesselId}")]
        public ActionResult<FuelData> GetFuelData(int vesselId)
        {
            var fuelData = new FuelData
            {
                VesselId = vesselId,
                CurrentLevel = 78.5, // Percentage
                ConsumptionRate = 2.8, // Tons per hour
                EstimatedRange = 485, // Nautical miles
                FuelType = "Marine Gas Oil",
                LastRefueling = DateTime.UtcNow.AddDays(-2),
                NextRefuelingRecommended = DateTime.UtcNow.AddDays(1),
                EfficiencyScore = 87.2, // 0-100 scale
                Recommendations = new List<string>
                {
                    "Reduce speed by 1 knot to improve efficiency by 8%",
                    "Weather routing suggests alternate course for fuel savings"
                }
            };

            return Ok(fuelData);
        }

        // Coast Guard and emergency services integration
        [HttpPost("emergency/notify-coastguard")]
        public ActionResult<EmergencyNotificationResponse> NotifyCoastGuard([FromBody] EmergencyNotification notification)
        {
            _logger.LogCritical($"EMERGENCY: Notifying Coast Guard - {notification.EmergencyType} on vessel {notification.VesselId}");
            
            // In real system: integrate with Norwegian Coast Guard systems
            var response = new EmergencyNotificationResponse
            {
                NotificationId = Guid.NewGuid(),
                Acknowledged = true,
                AssignedUnit = "HV-01 Svalbard",
                EstimatedResponseTime = TimeSpan.FromMinutes(45),
                Instructions = "Maintain current position. Coast Guard vessel dispatched.",
                ContactFrequency = "VHF Channel 16",
                NotifiedAt = DateTime.UtcNow
            };

            return Ok(response);
        }

        // Helper methods
        private static VesselSensorData? GetMockSensorData(int vesselId)
        {
            if (vesselId <= 0) return null;

            return new VesselSensorData
            {
                VesselId = vesselId,
                EngineData = new EngineData
                {
                    MainEngineRPM = 1450,
                    MainEngineTemperature = 85.2,
                    FuelPressure = 2.8,
                    OilPressure = 4.2,
                    CoolantTemperature = 78.5
                },
                NavigationData = new NavigationSensorData
                {
                    GPSAccuracy = 2.1,
                    CompassHeading = 045,
                    GyroHeading = 044.8,
                    Depth = 185.7,
                    LogSpeed = 14.2
                },
                EnvironmentalData = new EnvironmentalSensorData
                {
                    ExternalTemperature = 8.5,
                    Humidity = 78,
                    BarometricPressure = 1013.25,
                    WindSpeed = 12.3,
                    WindDirection = 225
                },
                Timestamp = DateTime.UtcNow
            };
        }

        private static List<TelemetryAlert> ProcessTelemetryAlerts(TelemetryData telemetry)
        {
            var alerts = new List<TelemetryAlert>();

            foreach (var reading in telemetry.SensorReadings)
            {
                // Example alert conditions
                if (reading.SensorType == "Engine Temperature" && reading.Value > 90)
                {
                    alerts.Add(new TelemetryAlert
                    {
                        SensorType = reading.SensorType,
                        AlertType = "High Temperature",
                        Value = reading.Value,
                        Threshold = 90,
                        Severity = AlertSeverity.High
                    });
                }
            }

            return alerts;
        }

        private static double GetTemperatureForLocation(double latitude)
        {
            // Simple temperature model based on latitude
            return latitude > 70 ? -2.0 : 8.0;
        }

        private static string GetPortName(int portId)
        {
            return portId switch
            {
                1 => "Bergen",
                2 => "Tromsø",
                3 => "Kirkenes",
                _ => "Unknown Port"
            };
        }
    }

    // IoT and integration models
    public class VesselSensorData
    {
        public int VesselId { get; set; }
        public EngineData EngineData { get; set; } = new EngineData();
        public NavigationSensorData NavigationData { get; set; } = new NavigationSensorData();
        public EnvironmentalSensorData EnvironmentalData { get; set; } = new EnvironmentalSensorData();
        public DateTime Timestamp { get; set; }
    }

    public class EngineData
    {
        public double MainEngineRPM { get; set; }
        public double MainEngineTemperature { get; set; }
        public double FuelPressure { get; set; }
        public double OilPressure { get; set; }
        public double CoolantTemperature { get; set; }
    }

    public class NavigationSensorData
    {
        public double GPSAccuracy { get; set; }
        public double CompassHeading { get; set; }
        public double GyroHeading { get; set; }
        public double Depth { get; set; }
        public double LogSpeed { get; set; }
    }

    public class EnvironmentalSensorData
    {
        public double ExternalTemperature { get; set; }
        public double Humidity { get; set; }
        public double BarometricPressure { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
    }

    public class TelemetryData
    {
        public int VesselId { get; set; }
        public List<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
        public DateTime Timestamp { get; set; }
    }

    public class SensorReading
    {
        public string SensorType { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class TelemetryResponse
    {
        public bool Received { get; set; }
        public DateTime ProcessedAt { get; set; }
        public int AlertsTriggered { get; set; }
        public DateTime NextExpectedTelemetry { get; set; }
    }

    public class TelemetryAlert
    {
        public string SensorType { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public double Value { get; set; }
        public double Threshold { get; set; }
        public AlertSeverity Severity { get; set; }
    }

    public class WeatherData
    {
        public Position Location { get; set; } = new Position();
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
        public double WaveHeight { get; set; }
        public double Visibility { get; set; }
        public double Precipitation { get; set; }
        public double BarometricPressure { get; set; }
        public string Conditions { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class AISData
    {
        public int VesselId { get; set; }
        public int MMSI { get; set; }
        public Position Position { get; set; } = new Position();
        public NavigationStatus NavigationStatus { get; set; }
        public double RateOfTurn { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateTime ETA { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public enum NavigationStatus
    {
        UnderWayUsingEngine,
        AtAnchor,
        NotUnderCommand,
        Moored,
        Aground
    }

    public class PortSystemStatus
    {
        public int PortId { get; set; }
        public string PortName { get; set; } = string.Empty;
        public List<BerthStatus> BerthAvailability { get; set; } = new List<BerthStatus>();
        public TideInfo TideInfo { get; set; } = new TideInfo();
        public PortServices Services { get; set; } = new PortServices();
        public DateTime LastUpdated { get; set; }
    }

    public class BerthStatus
    {
        public string BerthNumber { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? VesselName { get; set; }
    }

    public class TideInfo
    {
        public double CurrentHeight { get; set; }
        public DateTime NextHighTide { get; set; }
        public DateTime NextLowTide { get; set; }
        public double MaxHeight { get; set; }
        public double MinHeight { get; set; }
    }

    public class PortServices
    {
        public bool FuelAvailable { get; set; }
        public bool FreshWaterAvailable { get; set; }
        public bool WasteDisposalAvailable { get; set; }
        public bool CargoHandlingAvailable { get; set; }
        public bool PilotServiceRequired { get; set; }
    }

    public class FuelData
    {
        public int VesselId { get; set; }
        public double CurrentLevel { get; set; }
        public double ConsumptionRate { get; set; }
        public double EstimatedRange { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public DateTime LastRefueling { get; set; }
        public DateTime NextRefuelingRecommended { get; set; }
        public double EfficiencyScore { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    public class EmergencyNotification
    {
        public int VesselId { get; set; }
        public string EmergencyType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Position Location { get; set; } = new Position();
        public AlertSeverity Severity { get; set; }
    }

    public class EmergencyNotificationResponse
    {
        public Guid NotificationId { get; set; }
        public bool Acknowledged { get; set; }
        public string AssignedUnit { get; set; } = string.Empty;
        public TimeSpan EstimatedResponseTime { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public string ContactFrequency { get; set; } = string.Empty;
        public DateTime NotifiedAt { get; set; }
    }
}