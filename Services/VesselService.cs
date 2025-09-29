using MaritimeIQ.Platform.Models;
using MaritimeIQ.Platform.Services.Interfaces;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Service for vessel management operations
    /// </summary>
    public class VesselService : BaseMaritimeService, IVesselService
    {
        public override string ServiceName => "Vessel Service";

        public VesselService(ILogger<VesselService> logger, IConfiguration? configuration = null) 
            : base(logger, configuration)
        {
        }

        public async Task<List<Vessel>> GetAllVesselsAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                LogInformation("Retrieving all vessels in fleet");
                
                await Task.Delay(100);
                
                return new List<Vessel>
                {
                    new Vessel
                    {
                        Id = 1,
                        Name = "MS Arctic Explorer",
                        IMONumber = "9876543",
                        CallSign = "LAJX",
                        Type = VesselType.PassengerFerry,
                        PassengerCapacity = 640,
                        CrewCapacity = 55,
                        Status = VesselStatus.InService,
                        CurrentPosition = new Position { Latitude = 69.6492, Longitude = 18.9553 },
                        LastUpdated = DateTime.UtcNow.AddMinutes(-5)
                    }
                };
            }, nameof(GetAllVesselsAsync));
        }

        public async Task<Vessel?> GetVesselByIdAsync(string vesselId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                LogInformation($"Retrieving vessel details for ID: {vesselId}");
                
                await Task.Delay(80);
                
                return new Vessel
                {
                    Id = 1,
                    Name = "MS Arctic Explorer",
                    IMONumber = "9876543",
                    CallSign = "LAJX",
                    Type = VesselType.PassengerFerry,
                    PassengerCapacity = 640,
                    CrewCapacity = 55,
                    Status = VesselStatus.InService,
                    CurrentPosition = new Position { Latitude = 69.6492, Longitude = 18.9553 },
                    LastUpdated = DateTime.UtcNow.AddMinutes(-2)
                };
            }, nameof(GetVesselByIdAsync));
        }

        public async Task<bool> UpdateVesselStatusAsync(string vesselId, VesselStatusUpdateRequest request)
        {
            return await ExecuteOperationAsync(async () =>
            {
                LogInformation($"Updating vessel status for {vesselId} to {request.Status}");
                
                await Task.Delay(50);
                
                return true;
            }, nameof(UpdateVesselStatusAsync));
        }

        public async Task<VesselOperationalData> GetVesselOperationalDataAsync(string vesselId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                LogInformation($"Retrieving operational data for vessel {vesselId}");
                
                await Task.Delay(90);
                
                return new VesselOperationalData
                {
                    VesselId = vesselId,
                    VesselName = "MS Arctic Explorer",
                    CurrentPosition = new Position { Latitude = 69.6492, Longitude = 18.9553 },
                    Status = VesselStatus.InService,
                    CurrentPassengers = 485,
                    CrewOnBoard = 52,
                    FuelLevel = 78.5,
                    CurrentRoute = "Bergen-Kirkenes",
                    NextPortETA = DateTime.UtcNow.AddHours(4.5)
                };
            }, nameof(GetVesselOperationalDataAsync));
        }

        public async Task<VesselEmergencyResponse> ReportVesselEmergencyAsync(string vesselId, VesselEmergencyReport emergency)
        {
            return await ExecuteOperationAsync(async () =>
            {
                LogInformation($"Reporting emergency for vessel {vesselId}: {emergency.Type}");
                
                await Task.Delay(200);
                
                return new VesselEmergencyResponse
                {
                    VesselName = "MS Arctic Explorer",
                    Acknowledged = true,
                    Response = "Emergency protocol activated, rescue coordination center notified"
                };
            }, nameof(ReportVesselEmergencyAsync));
        }
    }
}