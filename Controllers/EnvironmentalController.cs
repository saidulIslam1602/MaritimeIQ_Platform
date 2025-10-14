using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Services;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// Environmental Monitoring Controller for MaritimeIQ Platform
    /// 
    /// This controller provides comprehensive environmental monitoring capabilities including:
    /// - Real-time emission tracking and compliance monitoring
    /// - Environmental data processing and analysis
    /// - Alert generation for environmental thresholds
    /// - Integration with hybrid propulsion systems
    /// - Compliance reporting for maritime regulations
    /// 
    /// Supports IMO 2020 sulfur regulations, NOx Tier III compliance,
    /// and hybrid battery system monitoring for sustainable maritime operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Environmental Monitoring")]
    public class EnvironmentalController : ControllerBase
    {
        private readonly EnvironmentalMonitoringService _environmentalService;
        private readonly ILogger<EnvironmentalController> _logger;

        public EnvironmentalController(
            EnvironmentalMonitoringService environmentalService,
            ILogger<EnvironmentalController> logger)
        {
            _environmentalService = environmentalService;
            _logger = logger;
        }

        /// <summary>
        /// Get current environmental compliance report for the fleet
        /// </summary>
        /// <returns>Environmental compliance status and emission data</returns>
        [HttpGet("compliance-report")]
        [ProducesResponseType(typeof(EnvironmentalComplianceReport), 200)]
        public async Task<ActionResult<EnvironmentalComplianceReport>> GetComplianceReport()
        {
            try
            {
                _logger.LogInformation("Retrieving environmental compliance report");
                var report = await _environmentalService.GetComplianceReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance report");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Process environmental data readings for compliance monitoring
        /// </summary>
        /// <param name="environmentalData">Array of environmental readings to process</param>
        /// <returns>Processing results including alerts generated</returns>
        [HttpPost("process-environmental-data")]
        [ProducesResponseType(typeof(EnvironmentalProcessingResult), 200)]
        public async Task<ActionResult<EnvironmentalProcessingResult>> ProcessEnvironmentalData(
            [FromBody] string[] environmentalData)
        {
            try
            {
                if (environmentalData == null || environmentalData.Length == 0)
                {
                    return BadRequest("Environmental data is required");
                }

                _logger.LogInformation($"Processing {environmentalData.Length} environmental readings");
                var result = await _environmentalService.ProcessEnvironmentalDataAsync(environmentalData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing environmental data");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get environmental readings sample data for testing
        /// </summary>
        /// <returns>Sample environmental readings</returns>
        [HttpGet("sample-data")]
        [ProducesResponseType(typeof(List<EnvironmentalReading>), 200)]
        public ActionResult<List<EnvironmentalReading>> GetSampleEnvironmentalData()
        {
            var sampleData = new List<EnvironmentalReading>
            {
                new EnvironmentalReading
                {
                    VesselId = "HK-001",
                    VesselName = "MS Arctic Explorer",
                    CO2Level = 45.2,
                    NOxLevel = 8.1,
                    SOxLevel = 0.3,
                    BatteryLevel = 89,
                    HybridModeActive = true,
                    Timestamp = DateTime.UtcNow
                },
                new EnvironmentalReading
                {
                    VesselId = "HK-002",
                    VesselName = "MS Nordic Aurora",
                    CO2Level = 48.3,
                    NOxLevel = 8.4,
                    SOxLevel = 0.4,
                    BatteryLevel = 86,
                    HybridModeActive = true,
                    Timestamp = DateTime.UtcNow
                }
            };

            return Ok(sampleData);
        }

        /// <summary>
        /// Get environmental alerts for the fleet
        /// </summary>
        /// <returns>Current environmental alerts</returns>
        [HttpGet("alerts")]
        [ProducesResponseType(typeof(List<EnvironmentalAlert>), 200)]
        public ActionResult<List<EnvironmentalAlert>> GetEnvironmentalAlerts()
        {
            try
            {
                _logger.LogInformation("Retrieving current environmental alerts");
                
                var alerts = new List<EnvironmentalAlert>
                {
                    new EnvironmentalAlert
                    {
                        VesselName = "MS Arctic Explorer",
                        AlertType = "Battery Low",
                        Severity = "Medium",
                        CurrentValue = 18,
                        ThresholdValue = 20,
                        Unit = "%",
                        Timestamp = DateTime.UtcNow.AddMinutes(-15),
                        Description = "Battery level 18% is below optimal threshold of 20%"
                    }
                };

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving environmental alerts");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}