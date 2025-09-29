using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for vessel management operations
    /// </summary>
    public interface IVesselService
    {
        /// <summary>
        /// Get all vessels in the fleet
        /// </summary>
        Task<List<Vessel>> GetAllVesselsAsync();

        /// <summary>
        /// Get vessel by ID
        /// </summary>
        Task<Vessel?> GetVesselByIdAsync(string vesselId);

        /// <summary>
        /// Update vessel status
        /// </summary>
        Task<bool> UpdateVesselStatusAsync(string vesselId, VesselStatusUpdateRequest request);

        /// <summary>
        /// Get vessel operational data
        /// </summary>
        Task<VesselOperationalData> GetVesselOperationalDataAsync(string vesselId);

        /// <summary>
        /// Report vessel emergency
        /// </summary>
        Task<VesselEmergencyResponse> ReportVesselEmergencyAsync(string vesselId, VesselEmergencyReport emergency);
    }

    public class VesselOperationalData
    {
        public string VesselId { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public Position? CurrentPosition { get; set; }
        public VesselStatus Status { get; set; }
        public int CurrentPassengers { get; set; }
        public int CrewOnBoard { get; set; }
        public double FuelLevel { get; set; }
        public string CurrentRoute { get; set; } = string.Empty;
        public DateTime? NextPortETA { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}