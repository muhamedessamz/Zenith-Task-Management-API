using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Api.DTOs.Category;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service) => _service = service;

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var categories = await _service.GetAllAsync(userId);
            var dtos = categories.Select(c => new CategoryReadDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Color = c.Color,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                TaskCount = c.Tasks?.Count ?? 0
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var category = await _service.GetByIdAsync(id, userId);
            if (category == null) return NotFound();

            var dto = new CategoryReadDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                CreatedAt = category.CreatedAt,
                UserId = category.UserId,
                TaskCount = category.Tasks?.Count ?? 0
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var userId = GetUserId();
            var entity = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Color = dto.Color
            };

            var created = await _service.CreateAsync(entity, userId);

            var readDto = new CategoryReadDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                Color = created.Color,
                CreatedAt = created.CreatedAt,
                UserId = created.UserId,
                TaskCount = 0
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, readDto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest(new { error = "Id mismatch." });

            var userId = GetUserId();
            var entity = new Category
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Color = dto.Color
            };

            var updated = await _service.UpdateAsync(entity, userId);

            var readDto = new CategoryReadDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Description = updated.Description,
                Color = updated.Color,
                CreatedAt = updated.CreatedAt,
                UserId = updated.UserId,
                TaskCount = updated.Tasks?.Count ?? 0
            };

            return Ok(readDto);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            await _service.DeleteAsync(id, userId);
            return NoContent();
        }
    }
}
