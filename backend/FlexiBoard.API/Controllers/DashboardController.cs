using FlexiBoard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlexiBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get current dashboard data from cache
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            return StatusCode(500, new { message = "Error retrieving dashboard data" });
        }
    }
}
