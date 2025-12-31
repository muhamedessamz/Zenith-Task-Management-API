using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Get comprehensive dashboard statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userId = GetUserId();
            var stats = await _service.GetDashboardStatsAsync(userId);
            return Ok(stats);
        }

        /// <summary>
        /// Get tasks per day for the last N days
        /// </summary>
        [HttpGet("tasks-per-day")]
        public async Task<IActionResult> GetTasksPerDay([FromQuery] int days = 7)
        {
            if (days < 1 || days > 30)
                return BadRequest(new { error = "Days must be between 1 and 30." });

            var userId = GetUserId();
            var data = await _service.GetTasksPerDayAsync(userId, days);
            return Ok(data);
        }
    }
}
