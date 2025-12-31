using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Api.DTOs.TimeTracking;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Entities;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/time")]
    [Authorize]
    public class TimeTrackingController : ControllerBase
    {
        private readonly ITimeTrackingService _service;
        private readonly ITaskDependencyService _depService;

        public TimeTrackingController(ITimeTrackingService service, ITaskDependencyService depService)
        {
            _service = service;
            _depService = depService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        [HttpPost("start")]
        public async Task<IActionResult> StartTimer(int taskId)
        {
            try 
            {
                if (await _depService.IsTaskBlockedAsync(taskId))
                {
                    return BadRequest("Cannot start timer: Task is blocked by incomplete dependencies.");
                }

                var entry = await _service.StartTimerAsync(taskId, GetUserId());
                // Re-fetch or manually map user name? Service doesn't include User on create.
                // Minimal return is fine.
                // Let's rely on standard mapping. UserName might be null if not loaded.
                return Ok(MapToDto(entry));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Task not found.");
            }
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopTimer(int taskId)
        {
            try
            {
                var entry = await _service.StopTimerAsync(taskId, GetUserId());
                return Ok(MapToDto(entry));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("manual")]
        public async Task<IActionResult> LogManual(int taskId, [FromBody] ManualTimeLogDto dto)
        {
            try
            {
                var entry = await _service.LogManualAsync(taskId, GetUserId(), dto.StartTime, dto.EndTime, dto.Notes);
                return Ok(MapToDto(entry));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Task not found.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(int taskId)
        {
            var entries = await _service.GetTaskHistoryAsync(taskId);
            var totalTime = await _service.GetTotalTimeSpentAsync(taskId);
            
            return Ok(new 
            {
                TotalTime = totalTime.ToString(@"hh\:mm\:ss"),
                Entries = entries.Select(MapToDto)
            });
        }

        private TimeEntryDto MapToDto(TimeEntry entry)
        {
            return new TimeEntryDto
            {
                Id = entry.Id,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                Notes = entry.Notes,
                IsManual = entry.IsManual,
                UserId = entry.UserId,
                UserName = entry.User?.DisplayName ?? "Unknown",
                CreatedAt = entry.CreatedAt
            };
        }
    }
}
