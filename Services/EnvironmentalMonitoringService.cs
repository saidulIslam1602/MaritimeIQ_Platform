using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HavilaKystruten.Maritime.Services
{
    /// <summary>
    /// Environmental Monitoring Service - Real-time CO2 thresholds, emission alerts,
    /// and environmental compliance monitoring for Havila's hybrid fleet
    /// </summary>
    public class EnvironmentalMonitoringService
    {
        private readonly ILogger<EnvironmentalMonitoringService> _logger;

        public EnvironmentalMonitoringService(ILogger<EnvironmentalMonitoringService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Process environmental data readings for compliance monitoring
        /// </summary>
        public async Task<EnvironmentalProcessingResult> ProcessEnvironmentalDataAsync(string[] events)
        {
            _logger.LogInformation($"Processing {events.Length} environmental measurements");

            var processedReadings = new List<ProcessedEnvironmentalReading>();
            var alertsGenerated = new List<EnvironmentalAlert>();

            foreach (string eventData in events)
            {
                try
                {
                    var envData = JsonSerializer.Deserialize<EnvironmentalReading>(eventData);
                    if (envData != null)
                    {
                        var result = await ProcessEnvironmentalReadingAsync(envData);
                        processedReadings.Add(result.Reading);
                        
                        if (result.Alerts.Any())
                        {
                            alertsGenerated.AddRange(result.Alerts);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing environmental data: {eventData}");
                }
            }

            return new EnvironmentalProcessingResult
            {
                ProcessedReadings = processedReadings,
                Alerts = alertsGenerated,
                ProcessedAt = DateTime.UtcNow,
                TotalReadings = events.Length,
                AlertCount = alertsGenerated.Count
            };
        }

        /// <summary>
        /// Get current environmental compliance status
        /// </summary>
        public Task<EnvironmentalComplianceReport> GetComplianceReportAsync()
        {
            _logger.LogInformation("Generating environmental compliance report");

            var report = new EnvironmentalComplianceReport
            {
                ComplianceStatus = "Compliant",
                CO2Emissions = new EmissionData
                {
                    Current = 45.2,
                    Threshold = 50.0,
                    Unit = "g/kWh",
                    Status = "Normal"
                },
                NOxEmissions = new EmissionData
                {
                    Current = 8.1,
                    Threshold = 9.0,
                    Unit = "g/kWh",
                    Status = "Normal"
                },
                SOxEmissions = new EmissionData
                {
                    Current = 0.3,
                    Threshold = 0.5,
                    Unit = "g/kWh",
                    Status = "Normal"
                },
                BatteryEfficiency = 87.5,
                HybridModeActive = true,
                LastUpdated = DateTime.UtcNow,
                ActiveVessels = new List<VesselEnvironmentalStatus>
                {
                    new VesselEnvironmentalStatus
                    {
                        VesselName = "MS Havila Castor",
                        CO2Level = 42.1,
                        NOxLevel = 7.8,
                        SOxLevel = 0.2,
                        BatteryCharge = 89,
                        HybridMode = true,
                        ComplianceStatus = "Compliant"
                    },
                    new VesselEnvironmentalStatus
                    {
                        VesselName = "MS Havila Capella",
                        CO2Level = 48.3,
                        NOxLevel = 8.4,
                        SOxLevel = 0.4,
                        BatteryCharge = 86,
                        HybridMode = true,
                        ComplianceStatus = "Compliant"
                    }
                }
            };

            return Task.FromResult(report);
        }

        /// <summary>
        /// Process individual environmental reading
        /// </summary>
        private async Task<EnvironmentalReadingResult> ProcessEnvironmentalReadingAsync(EnvironmentalReading reading)
        {
            var processedReading = new ProcessedEnvironmentalReading
            {
                VesselId = reading.VesselId,
                VesselName = reading.VesselName,
                CO2Level = reading.CO2Level,
                NOxLevel = reading.NOxLevel,
                SOxLevel = reading.SOxLevel,
                BatteryLevel = reading.BatteryLevel,
                HybridModeActive = reading.HybridModeActive,
                Timestamp = reading.Timestamp,
                ProcessedAt = DateTime.UtcNow
            };

            var alerts = new List<EnvironmentalAlert>();

            // Check CO2 threshold
            if (reading.CO2Level > 50.0)
            {
                alerts.Add(new EnvironmentalAlert
                {
                    VesselName = reading.VesselName,
                    AlertType = "CO2 Threshold Exceeded",
                    Severity = "High",
                    CurrentValue = reading.CO2Level,
                    ThresholdValue = 50.0,
                    Unit = "g/kWh",
                    Timestamp = DateTime.UtcNow,
                    Description = $"CO2 emissions {reading.CO2Level} g/kWh exceed regulatory threshold of 50.0 g/kWh"
                });
            }

            // Check NOx threshold
            if (reading.NOxLevel > 9.0)
            {
                alerts.Add(new EnvironmentalAlert
                {
                    VesselName = reading.VesselName,
                    AlertType = "NOx Threshold Exceeded",
                    Severity = "Medium",
                    CurrentValue = reading.NOxLevel,
                    ThresholdValue = 9.0,
                    Unit = "g/kWh",
                    Timestamp = DateTime.UtcNow,
                    Description = $"NOx emissions {reading.NOxLevel} g/kWh exceed threshold of 9.0 g/kWh"
                });
            }

            // Check SOx threshold
            if (reading.SOxLevel > 0.5)
            {
                alerts.Add(new EnvironmentalAlert
                {
                    VesselName = reading.VesselName,
                    AlertType = "SOx Threshold Exceeded",
                    Severity = "Medium",
                    CurrentValue = reading.SOxLevel,
                    ThresholdValue = 0.5,
                    Unit = "g/kWh",
                    Timestamp = DateTime.UtcNow,
                    Description = $"SOx emissions {reading.SOxLevel} g/kWh exceed threshold of 0.5 g/kWh"
                });
            }

            // Check battery level
            if (reading.BatteryLevel < 20)
            {
                alerts.Add(new EnvironmentalAlert
                {
                    VesselName = reading.VesselName,
                    AlertType = "Low Battery Level",
                    Severity = "Medium",
                    CurrentValue = reading.BatteryLevel,
                    ThresholdValue = 20,
                    Unit = "%",
                    Timestamp = DateTime.UtcNow,
                    Description = $"Battery level {reading.BatteryLevel}% is below optimal threshold of 20%"
                });
            }

            await Task.Delay(1); // Simulate processing

            return new EnvironmentalReadingResult
            {
                Reading = processedReading,
                Alerts = alerts
            };
        }
    }

    // Supporting classes
    public class EnvironmentalReading
    {
        public string VesselId { get; set; } = "";
        public string VesselName { get; set; } = "";
        public double CO2Level { get; set; }
        public double NOxLevel { get; set; }
        public double SOxLevel { get; set; }
        public int BatteryLevel { get; set; }
        public bool HybridModeActive { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ProcessedEnvironmentalReading
    {
        public string VesselId { get; set; } = "";
        public string VesselName { get; set; } = "";
        public double CO2Level { get; set; }
        public double NOxLevel { get; set; }
        public double SOxLevel { get; set; }
        public int BatteryLevel { get; set; }
        public bool HybridModeActive { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    public class EnvironmentalAlert
    {
        public string VesselName { get; set; } = "";
        public string AlertType { get; set; } = "";
        public string Severity { get; set; } = "";
        public double CurrentValue { get; set; }
        public double ThresholdValue { get; set; }
        public string Unit { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = "";
    }

    public class EnvironmentalProcessingResult
    {
        public List<ProcessedEnvironmentalReading> ProcessedReadings { get; set; } = new();
        public List<EnvironmentalAlert> Alerts { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
        public int TotalReadings { get; set; }
        public int AlertCount { get; set; }
    }

    public class EnvironmentalReadingResult
    {
        public ProcessedEnvironmentalReading Reading { get; set; } = new();
        public List<EnvironmentalAlert> Alerts { get; set; } = new();
    }

    public class EnvironmentalComplianceReport
    {
        public string ComplianceStatus { get; set; } = "";
        public EmissionData CO2Emissions { get; set; } = new();
        public EmissionData NOxEmissions { get; set; } = new();
        public EmissionData SOxEmissions { get; set; } = new();
        public double BatteryEfficiency { get; set; }
        public bool HybridModeActive { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<VesselEnvironmentalStatus> ActiveVessels { get; set; } = new();
    }

    public class EmissionData
    {
        public double Current { get; set; }
        public double Threshold { get; set; }
        public string Unit { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class VesselEnvironmentalStatus
    {
        public string VesselName { get; set; } = "";
        public double CO2Level { get; set; }
        public double NOxLevel { get; set; }
        public double SOxLevel { get; set; }
        public int BatteryCharge { get; set; }
        public bool HybridMode { get; set; }
        public string ComplianceStatus { get; set; } = "";
    }
}