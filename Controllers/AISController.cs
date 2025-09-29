using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Services;

namespace MaritimeIQ.Platform.Controllers
{
    [Route("api/[controller]")]
    public class AISController : BaseMaritimeController
    {
        private readonly AISProcessingService _aisService;

        public AISController(AISProcessingService aisService, ILogger<AISController> logger) 
            : base(logger)
        {
            _aisService = aisService;
        }

        /// <summary>
        /// Get real-time AIS analytics for the fleet
        /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAISAnalytics()
        {
            return await ExecuteOperationAsync(
                () => _aisService.GetAISAnalyticsAsync(),
                "GetAISAnalytics"
            );
        }

        /// <summary>
        /// Manually trigger AIS data processing (for testing)
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessAISData([FromBody] string[] events)
        {
            return await ExecuteOperationAsync(
                async () => {
                    await _aisService.ProcessAISDataAsync(events);
                    return new { message = "AIS data processing completed successfully" };
                },
                "ProcessAISData"
            );
        }
    }
}