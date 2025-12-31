using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Api.DTOs.Dependency;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/dependencies")]
    [Authorize]
    public class DependenciesController : ControllerBase
    {
        private readonly ITaskDependencyService _service;

        public DependenciesController(ITaskDependencyService service)
        {
            _service = service;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        [HttpPost("{dependsOnTaskId}")]
        public async Task<IActionResult> Add(int taskId, int dependsOnTaskId)
        {
            try 
            {
                await _service.AddDependencyAsync(taskId, dependsOnTaskId);
                return Ok(new { Message = "Dependency added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("{dependsOnTaskId}")]
        public async Task<IActionResult> Remove(int taskId, int dependsOnTaskId)
        {
            await _service.RemoveDependencyAsync(taskId, dependsOnTaskId);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int taskId)
        {
            var userId = GetUserId();
            var dependencies = await _service.GetDependenciesAsync(taskId, userId);
            return Ok(dependencies);
        }

        [HttpGet("blockers")]
        public async Task<IActionResult> GetBlockers(int taskId)
        {
            var blockers = await _service.GetBlockersAsync(taskId);
            // Basic mapping logic
            return Ok(blockers.Select(t => new DependencyDto 
            {
                TaskId = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted,
                AssignedTo = "Unknown" // Can enhance later by querying assignments or user
            }));
        }
    }
}
