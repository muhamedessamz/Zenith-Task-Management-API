using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/calendar")]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        [HttpGet("connect")]
        [Authorize]
        public IActionResult Connect()
        {
            var url = _calendarService.GetAuthUri(GetUserId());
            return Ok(new { AuthUrl = url });
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            // State contains userId
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                return BadRequest("Invalid response from Google.");

            var success = await _calendarService.ExchangeCodeForTokenAsync(state, code);
            
            if (success)
            {
                // Return simple HTML page closing itself or redirect to frontend
                return Content("<h1>Calendar Connected Successfully! ✅</h1><script>setTimeout(window.close, 2000);</script>", "text/html");
            }
            
            return BadRequest("Failed to verify Google account.");
        }

        /// <summary>
        /// Get Google Calendar connection status
        /// </summary>
        [HttpGet("status")]
        [Authorize]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var userId = GetUserId();
                
                var (isConnected, connectedAt) = await _calendarService.GetConnectionStatusAsync(userId);
                
                if (!isConnected)
                {
                    return Ok(new { 
                        isConnected = false 
                    });
                }
                
                return Ok(new { 
                    isConnected = true,
                    connectedAt = connectedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Disconnect Google Calendar
        /// </summary>
        [HttpPost("disconnect")]
        [Authorize]
        public async Task<IActionResult> Disconnect()
        {
            try
            {
                var userId = GetUserId();
                
                var success = await _calendarService.DisconnectAsync(userId);
                
                if (success)
                {
                    return Ok(new { message = "Calendar disconnected successfully" });
                }
                
                return Ok(new { message = "No calendar connection found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
