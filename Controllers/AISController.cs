using Microsoft.AspNetCore.Mvc;
using HavilaKystruten.Maritime.Services;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AISController : ControllerBase
    {
        private readonly AISProcessingService _aisService;
        private readonly ILogger<AISController> _logger;

        public AISController(AISProcessingService aisService, ILogger<AISController> logger)
        {
            _aisService = aisService;
            _logger = logger;
        }

        /// <summary>
        /// Get real-time AIS analytics for the fleet
        /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAISAnalytics()
        {
            try
            {
                var analytics = await _aisService.GetAISAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AIS analytics");
                return StatusCode(500, "Error retrieving AIS analytics");
            }
        }

        /// <summary>
        /// Manually trigger AIS data processing (for testing)
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessAISData([FromBody] string[] events)
        {
            try
            {
                await _aisService.ProcessAISDataAsync(events);
                return Ok(new { message = "AIS data processing completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AIS data");
                return StatusCode(500, "Error processing AIS data");
            }
        }
    }
}