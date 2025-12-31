using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/kanban")]
    [Authorize]
    public class KanbanController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public KanbanController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Get Kanban board view (tasks grouped by status)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBoard([FromQuery] int? projectId = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var board = await _taskService.GetKanbanBoardAsync(userId, projectId);
            
            return Ok(board);
        }

        /// <summary>
        /// Update task status (move between columns)
        /// </summary>
        [HttpPut("{taskId}/status")]
        public async Task<IActionResult> UpdateStatus(int taskId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                await _taskService.UpdateStatusAsync(taskId, request.Status, userId);
                
                return Ok(new { message = "Status updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update task position (reorder within column)
        /// </summary>
        [HttpPut("{taskId}/position")]
        public async Task<IActionResult> UpdatePosition(int taskId, [FromBody] UpdatePositionRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                await _taskService.UpdatePositionAsync(taskId, request.OrderIndex, userId);
                
                return Ok(new { message = "Position updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = "Todo";
    }

    public class UpdatePositionRequest
    {
        public int OrderIndex { get; set; }
    }
}
