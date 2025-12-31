using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.DTOs.User;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/assignments")]
    [Authorize]
    public class TaskAssignmentController : ControllerBase
    {
        private readonly ITaskAssignmentService _service;

        public TaskAssignmentController(ITaskAssignmentService service)
        {
            _service = service;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        [HttpGet]
        public async Task<IActionResult> GetAssignments(int taskId)
        {
            var assignments = await _service.GetTaskAssignmentsAsync(taskId);
            return Ok(assignments.Select(MapToDto));
        }

        [HttpPost]
        public async Task<IActionResult> AssignUser(int taskId, [FromBody] AssignTaskRequest request)
        {
            try 
            {
                var userId = GetUserId();
                var assignment = await _service.AssignUserAsync(taskId, request.UserIdentifier, userId, request.Note, request.Permission);
                return Ok(MapToDto(assignment));
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (KeyNotFoundException) { return NotFound("Task not found."); }
        }

        [HttpDelete("{assignmentId}")]
        public async Task<IActionResult> RemoveAssignment(int taskId, int assignmentId)
        {
             var userId = GetUserId();
             var success = await _service.RemoveAssignmentAsync(taskId, assignmentId, userId);
             if (!success) return NotFound();
             return NoContent();
        }

        private TaskAssignmentDto MapToDto(TaskAssignment entity)
        {
            return new TaskAssignmentDto
            {
                Id = entity.Id,
                TaskId = entity.TaskId,
                AssignedAt = entity.AssignedAt,
                Note = entity.Note,
                Permission = entity.Permission,
                AssignedTo = entity.AssignedToUser == null ? null : new UserSummaryDto 
                {
                    Id = entity.AssignedToUser.Id,
                    Email = entity.AssignedToUser.Email ?? "",
                    DisplayName = entity.AssignedToUser.DisplayName ?? "Unknown",
                    FirstName = entity.AssignedToUser.FirstName,
                    LastName = entity.AssignedToUser.LastName,
                    ProfilePicture = entity.AssignedToUser.ProfilePicture
                }
            };
        }
    }
}
