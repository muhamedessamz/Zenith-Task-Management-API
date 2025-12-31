using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Api.DTOs.Checklist;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/checklist")]
    [Authorize]
    public class ChecklistController : ControllerBase
    {
        private readonly IChecklistService _service;

        public ChecklistController(IChecklistService service) => _service = service;

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetAll(int taskId)
        {
            var userId = GetUserId();
            var items = await _service.GetByTaskIdAsync(taskId, userId);
            var dtos = items.Select(i => new ChecklistItemReadDto
            {
                Id = i.Id,
                Title = i.Title,
                IsCompleted = i.IsCompleted,
                Order = i.Order,
                CreatedAt = i.CreatedAt,
                TaskId = i.TaskId
            }).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int taskId, [FromBody] ChecklistItemCreateDto dto)
        {
            var userId = GetUserId();
            var entity = new ChecklistItem
            {
                Title = dto.Title,
                Order = dto.Order
            };

            var created = await _service.AddAsync(taskId, entity, userId);

            var readDto = new ChecklistItemReadDto
            {
                Id = created.Id,
                Title = created.Title,
                IsCompleted = created.IsCompleted,
                Order = created.Order,
                CreatedAt = created.CreatedAt,
                TaskId = created.TaskId
            };

            return CreatedAtAction(nameof(GetAll), new { taskId }, readDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int taskId, int id, [FromBody] ChecklistItemUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest(new { error = "Id mismatch." });

            var userId = GetUserId();
            var entity = new ChecklistItem
            {
                Id = dto.Id,
                Title = dto.Title,
                IsCompleted = dto.IsCompleted,
                Order = dto.Order
            };

            var updated = await _service.UpdateAsync(entity, userId);

            var readDto = new ChecklistItemReadDto
            {
                Id = updated.Id,
                Title = updated.Title,
                IsCompleted = updated.IsCompleted,
                Order = updated.Order,
                CreatedAt = updated.CreatedAt,
                TaskId = updated.TaskId
            };

            return Ok(readDto);
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleCompletion(int taskId, int id)
        {
            var userId = GetUserId();
            var updated = await _service.ToggleCompletionAsync(id, userId);

            var readDto = new ChecklistItemReadDto
            {
                Id = updated.Id,
                Title = updated.Title,
                IsCompleted = updated.IsCompleted,
                Order = updated.Order,
                CreatedAt = updated.CreatedAt,
                TaskId = updated.TaskId
            };

            return Ok(readDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int taskId, int id)
        {
            var userId = GetUserId();
            await _service.DeleteAsync(id, userId);
            return NoContent();
        }
    }
}
