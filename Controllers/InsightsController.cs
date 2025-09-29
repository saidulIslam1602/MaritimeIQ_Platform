using MaritimeIQ.Platform.Models;
using MaritimeIQ.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaritimeIQ.Platform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InsightsController : ControllerBase
{
    private readonly IMaritimeDataService _maritimeData;
    private readonly ILogger<InsightsController> _logger;

    public InsightsController(IMaritimeDataService maritimeData, ILogger<InsightsController> logger)
    {
        _maritimeData = maritimeData;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<MaritimeInsightsOverview> GetInsights()
    {
        var insights = _maritimeData.GetPlatformInsights();
        _logger.LogInformation("Generated maritime insights overview at {Timestamp}", insights.GeneratedAt);
        return Ok(insights);
    }

    [HttpGet("fleet-status")]
    public ActionResult<FleetStatusSummary> GetFleetStatus()
    {
        var status = _maritimeData.GetFleetStatus();
        return Ok(status);
    }

    [HttpGet("environmental")]
    public ActionResult<EnvironmentalSummary> GetEnvironmentalSummary()
    {
        var summary = _maritimeData.GetEnvironmentalSummary();
        return Ok(summary);
    }

    [HttpGet("voyages")]
    public ActionResult<VoyageScheduleSnapshot> GetVoyageSchedule()
    {
        var schedule = _maritimeData.GetVoyageSchedule();
        return Ok(schedule);
    }
}
