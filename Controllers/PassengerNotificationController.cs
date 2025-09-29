using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Services;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// Passenger Notification Controller - Provides REST API endpoints for
    /// passenger communications, boarding notifications, and Northern Lights alerts
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Passenger Notifications")]
    public class PassengerNotificationController : ControllerBase
    {
        private readonly PassengerNotificationService _notificationService;
        private readonly ILogger<PassengerNotificationController> _logger;

        public PassengerNotificationController(
            PassengerNotificationService notificationService,
            ILogger<PassengerNotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get current passenger notification summary
        /// </summary>
        /// <returns>Summary of active notifications and statistics</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(PassengerNotificationSummary), 200)]
        public async Task<ActionResult<PassengerNotificationSummary>> GetNotificationSummary()
        {
            try
            {
                _logger.LogInformation("Retrieving passenger notification summary");
                var summary = await _notificationService.GetNotificationSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification summary");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Process boarding notifications for passengers
        /// </summary>
        /// <param name="notificationMessages">Array of boarding notification messages</param>
        /// <returns>Processing results</returns>
        [HttpPost("process-boarding-notifications")]
        [ProducesResponseType(typeof(BoardingNotificationResult), 200)]
        public async Task<ActionResult<BoardingNotificationResult>> ProcessBoardingNotifications(
            [FromBody] string[] notificationMessages)
        {
            try
            {
                if (notificationMessages == null || notificationMessages.Length == 0)
                {
                    return BadRequest("Notification messages are required");
                }

                _logger.LogInformation($"Processing {notificationMessages.Length} boarding notifications");
                var result = await _notificationService.ProcessBoardingNotificationsAsync(notificationMessages);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing boarding notifications");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check Northern Lights viewing conditions and generate alerts
        /// </summary>
        /// <returns>Northern Lights viewing conditions and alert</returns>
        [HttpGet("northern-lights-conditions")]
        [ProducesResponseType(typeof(NorthernLightsAlert), 200)]
        public async Task<ActionResult<NorthernLightsAlert>> CheckNorthernLightsConditions()
        {
            try
            {
                _logger.LogInformation("Checking Northern Lights viewing conditions");
                var alert = await _notificationService.CheckNorthernLightsConditionsAsync();
                return Ok(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Northern Lights conditions");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Send delay notification to passengers
        /// </summary>
        /// <param name="delayInfo">Delay notification information</param>
        /// <returns>Result of delay notification processing</returns>
        [HttpPost("send-delay-notification")]
        [ProducesResponseType(typeof(DelayNotificationResult), 200)]
        public async Task<ActionResult<DelayNotificationResult>> SendDelayNotification(
            [FromBody] DelayNotification delayInfo)
        {
            try
            {
                if (delayInfo == null)
                {
                    return BadRequest("Delay notification information is required");
                }

                _logger.LogInformation($"Sending delay notification for {delayInfo.VesselName}");
                var result = await _notificationService.SendDelayNotificationAsync(delayInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending delay notification");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get sample boarding notification data for testing
        /// </summary>
        /// <returns>Sample boarding notification messages</returns>
        [HttpGet("sample-boarding-data")]
        [ProducesResponseType(typeof(List<BoardingNotification>), 200)]
        public ActionResult<List<BoardingNotification>> GetSampleBoardingData()
        {
            var sampleData = new List<BoardingNotification>
            {
                new BoardingNotification
                {
                    VesselName = "MS Arctic Explorer",
                    BoardingTime = DateTime.UtcNow.AddMinutes(30),
                    Gate = "A1",
                    PassengerIds = new List<string> { "P001", "P002", "P003" },
                    NotificationType = "Boarding",
                    Message = "Boarding for MS Arctic Explorer will begin in 30 minutes at Gate A1"
                },
                new BoardingNotification
                {
                    VesselName = "MS Nordic Aurora",
                    BoardingTime = DateTime.UtcNow.AddHours(2),
                    Gate = "B2",
                    PassengerIds = new List<string> { "P004", "P005", "P006" },
                    NotificationType = "Pre-boarding",
                    Message = "Pre-boarding information for MS Nordic Aurora departure"
                }
            };

            return Ok(sampleData);
        }

        /// <summary>
        /// Get sample delay notification data for testing
        /// </summary>
        /// <returns>Sample delay notification</returns>
        [HttpGet("sample-delay-data")]
        [ProducesResponseType(typeof(DelayNotification), 200)]
        public ActionResult<DelayNotification> GetSampleDelayData()
        {
            var sampleDelay = new DelayNotification
            {
                VesselName = "MS Arctic Explorer",
                DelayMinutes = 45,
                Reason = "Weather conditions - strong winds",
                AffectedPassengerIds = new List<string> { "P001", "P002", "P003", "P004", "P005" }
            };

            return Ok(sampleDelay);
        }
    }
}