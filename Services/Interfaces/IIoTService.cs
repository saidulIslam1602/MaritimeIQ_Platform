namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for IoT device management and sensor data processing
    /// </summary>
    public interface IIoTService
    {
        /// <summary>
        /// Get IoT device status for all vessels
        /// </summary>
        Task<IoTDeviceStatus> GetDeviceStatusAsync();

        /// <summary>
        /// Get sensor data for a specific vessel
        /// </summary>
        Task<List<SensorReading>> GetVesselSensorDataAsync(string vesselId, DateTime? startTime = null);

        /// <summary>
        /// Process incoming sensor data batch
        /// </summary>
        Task<SensorDataProcessingResult> ProcessSensorDataAsync(string vesselId, List<SensorReading> readings);

        /// <summary>
        /// Get IoT alerts and notifications
        /// </summary>
        Task<List<IoTAlert>> GetIoTAlertsAsync();
    }

    public class IoTDeviceStatus
    {
        public int TotalDevices { get; set; }
        public int OnlineDevices { get; set; }
        public int OfflineDevices { get; set; }
        public double HealthScore { get; set; }
        public List<DeviceInfo> Devices { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class DeviceInfo
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string VesselId { get; set; } = string.Empty;
        public DateTime LastSeen { get; set; }
        public double BatteryLevel { get; set; }
    }

    public class SensorReading
    {
        public string SensorId { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Quality { get; set; } = "Good";
    }

    public class SensorDataProcessingResult
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public int ProcessedReadings { get; set; }
        public int RejectedReadings { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<IoTAlert> GeneratedAlerts { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class IoTAlert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DeviceId { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
    }
}