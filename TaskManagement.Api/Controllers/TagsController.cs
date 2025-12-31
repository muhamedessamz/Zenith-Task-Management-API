using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Api.DTOs.Tag;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly ITagRepository _repository;

        public TagsController(ITagRepository repository) => _repository = repository;

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var tags = await _repository.GetAllAsync(userId);
            var dtos = tags.Select(t => new TagReadDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                CreatedAt = t.CreatedAt,
                TaskCount = t.TaskTags.Count
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var tag = await _repository.GetByIdAsync(id, userId);
            if (tag == null) return NotFound();

            var dto = new TagReadDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color,
                CreatedAt = tag.CreatedAt,
                TaskCount = tag.TaskTags.Count
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TagCreateDto dto)
        {
            var userId = GetUserId();
            var tag = new Tag
            {
                Name = dto.Name,
                Color = dto.Color,
                UserId = userId
            };

            var created = await _repository.AddAsync(tag);
            var readDto = new TagReadDto
            {
                Id = created.Id,
                Name = created.Name,
                Color = created.Color,
                CreatedAt = created.CreatedAt,
                TaskCount = 0
            };
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, readDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TagUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var userId = GetUserId();
            var existing = await _repository.GetByIdAsync(id, userId);
            if (existing == null) return NotFound();

            existing.Name = dto.Name;
            existing.Color = dto.Color;

            var updated = await _repository.UpdateAsync(existing);
            var readDto = new TagReadDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Color = updated.Color,
                CreatedAt = updated.CreatedAt,
                TaskCount = updated.TaskTags.Count
            };
            return Ok(readDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (!await _repository.ExistsAsync(id, userId))
                return NotFound();

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
