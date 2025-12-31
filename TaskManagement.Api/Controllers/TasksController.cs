using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.DTOs.User;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _service;
        private readonly INotificationService _notificationService;
        private readonly ITaskDependencyService _depService;
        private readonly ICalendarService _calendarService;
        private readonly ITaskAssignmentService _assignmentService;
        private readonly ITagRepository _tagRepository;

        public TasksController(
            ITaskService service, 
            INotificationService notificationService, 
            ITaskDependencyService depService, 
            ICalendarService calendarService,
            ITaskAssignmentService assignmentService,
            ITagRepository tagRepository)
        {
            _service = service;
            _notificationService = notificationService;
            _depService = depService;
            _calendarService = calendarService;
            _assignmentService = assignmentService;
            _tagRepository = tagRepository;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

         private TaskReadDto MapToReadDto(TaskItem t)
        {
            return new TaskReadDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate,
                UserId = t.UserId,
                CategoryId = t.CategoryId,
                CategoryName = t.Category?.Name,
                ProjectId = t.ProjectId,
                ProjectTitle = t.Project?.Title,
                RecurrencePattern = t.RecurrencePattern,
                NextOccurrence = t.NextOccurrence,
                Assignments = t.Assignments.Select(a => new TaskAssignmentDto
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    AssignedAt = a.AssignedAt,
                    Note = a.Note,
                    Permission = a.Permission,
                    AssignedTo = a.AssignedToUser == null ? null : new UserSummaryDto
                    {
                        Id = a.AssignedToUser.Id,
                        Email = a.AssignedToUser.Email!,
                        DisplayName = a.AssignedToUser.DisplayName ?? "Unknown",
                        FirstName = a.AssignedToUser.FirstName,
                        LastName = a.AssignedToUser.LastName,
                        ProfilePicture = a.AssignedToUser.ProfilePicture
                    }
                }).ToList(),
                Tags = t.TaskTags.Select(tt => new DTOs.Tag.TagReadDto
                {
                    Id = tt.Tag.Id,
                    Name = tt.Tag.Name,
                    Color = tt.Tag.Color,
                    CreatedAt = tt.Tag.CreatedAt,
                    TaskCount = tt.Tag.TaskTags.Count
                }).ToList()
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var tasks = await _service.GetAllAsync(userId);
            var dtos = tasks.Select(MapToReadDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? priority = null,
            [FromQuery] bool? isCompleted = null)
        {
            var userId = GetUserId();
            var pagedResult = await _service.GetPagedAsync(userId, page, pageSize);
            
            // Apply filters
            var filteredData = pagedResult.Data.AsEnumerable();
            
            if (categoryId.HasValue)
            {
                filteredData = filteredData.Where(t => t.CategoryId == categoryId.Value);
            }
            
            if (!string.IsNullOrEmpty(priority))
            {
                filteredData = filteredData.Where(t => t.Priority.ToString() == priority);
            }
            
            if (isCompleted.HasValue)
            {
                filteredData = filteredData.Where(t => t.IsCompleted == isCompleted.Value);
            }
            
            var dtos = filteredData.Select(MapToReadDto).ToList();

            var response = new
            {
                data = dtos,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalItems = dtos.Count,
                    totalPages = (int)Math.Ceiling(dtos.Count / (double)pageSize),
                    hasNextPage = page * pageSize < dtos.Count,
                    hasPreviousPage = page > 1
                }
            };

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var t = await _service.GetByIdAsync(id, userId);
            if (t == null) return NotFound();

            if (await _depService.IsTaskBlockedAsync(id))
            {
                return StatusCode(403, new { Message = "Access Denied: This task is locked until prerequisite tasks are completed." });
            }

            var dto = MapToReadDto(t);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskCreateDto dto)
        {
            try
            {
                var userId = GetUserId();
                var entity = new TaskItem
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    IsCompleted = dto.IsCompleted,
                    Priority = dto.Priority,
                    DueDate = dto.DueDate,
                    CategoryId = dto.CategoryId,
                    ProjectId = dto.ProjectId,
                    RecurrencePattern = dto.RecurrencePattern
                };

                var created = await _service.CreateAsync(entity, userId);

                // Create task assignments if any users are assigned
                if (dto.AssignedUserIds != null && dto.AssignedUserIds.Any())
                {
                    await _assignmentService.AssignMultipleUsersAsync(created.Id, dto.AssignedUserIds, userId);
                }

                // Sync to Calendar
                try { await _calendarService.SyncTaskToCalendarAsync(created.Id); } catch {}

                // Re-fetch to get included properties
                var fetched = await _service.GetByIdAsync(created.Id, userId);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToReadDto(fetched ?? created));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto dto)
        {
            // Debug logging
            Console.WriteLine($"🔍 Update Task Request - ID: {id}");
            Console.WriteLine($"   DTO.Id: {dto.Id}");
            Console.WriteLine($"   Title: {dto.Title}");
            Console.WriteLine($"   Priority: {dto.Priority}");
            Console.WriteLine($"   IsCompleted: {dto.IsCompleted}");
            Console.WriteLine($"   DueDate: {dto.DueDate}");
            Console.WriteLine($"   CategoryId: {dto.CategoryId}");
            Console.WriteLine($"   ProjectId: {dto.ProjectId}");
            
            if (id != dto.Id) return BadRequest(new { error = "Id mismatch." });

            try
            {
                var userId = GetUserId();
                
                // Get existing task to check status change
                var existingTask = await _service.GetByIdAsync(id, userId);
                bool wasCompleted = existingTask?.IsCompleted ?? false;

                var entity = new TaskItem
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Description = dto.Description,
                    IsCompleted = dto.IsCompleted,
                    Priority = dto.Priority,
                    DueDate = dto.DueDate,
                    CategoryId = dto.CategoryId,
                    ProjectId = dto.ProjectId,
                    RecurrencePattern = dto.RecurrencePattern
                };

                var updated = await _service.UpdateAsync(entity, userId);

                // Check if completed status changed to true
                if (!wasCompleted && updated.IsCompleted)
                {
                    await _notificationService.NotifyTaskCompletedAsync(updated, userId);
                }

                // Sync to Calendar
                try { await _calendarService.SyncTaskToCalendarAsync(updated.Id); } catch {}
                
                // Re-fetch to allow mapping full details
                 var fetched = await _service.GetByIdAsync(updated.Id, userId);

                return Ok(MapToReadDto(fetched ?? updated));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"❌ ArgumentException: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetUserId();
                // Ensure calendar event is deleted before task is removed
                try { await _calendarService.DeleteEventAsync(id); } catch {}
                await _service.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] bool isCompleted)
        {
            var userId = GetUserId();
            var tasks = await _service.FilterByCompletionAsync(isCompleted, userId);
            var dtos = tasks.Select(MapToReadDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("filter-by-priority")]
        public async Task<IActionResult> FilterByPriority([FromQuery] TaskPriority priority)
        {
            var userId = GetUserId();
            var tasks = await _service.FilterByPriorityAsync(priority, userId);
            var dtos = tasks.Select(MapToReadDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("sort")]
        public async Task<IActionResult> Sort([FromQuery] bool ascending = true)
        {
            var userId = GetUserId();
            var tasks = await _service.SortByDueDateAsync(ascending, userId);
            var dtos = tasks.Select(MapToReadDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var userId = GetUserId();
            var tasks = await _service.SearchAsync(userId, query);
            var dtos = tasks.Select(MapToReadDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("advanced-filter")]
        public async Task<IActionResult> AdvancedFilter(
            [FromQuery] TaskPriority? priority,
            [FromQuery] int? categoryId,
            [FromQuery] bool? isCompleted)
        {
            var userId = GetUserId();
            var tasks = await _service.AdvancedFilterAsync(userId, priority, categoryId, isCompleted);
            var dtos = tasks.Select(MapToReadDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var userId = GetUserId();
            var tasks = await _service.GetByDateRangeAsync(userId, from, to);
            var dtos = tasks.Select(MapToReadDto).ToList();
            return Ok(dtos);
        }

        // ==================== Task-Tag Relationship Endpoints ====================

        /// <summary>
        /// Get all tags for a specific task
        /// </summary>
        [HttpGet("{taskId}/tags")]
        public async Task<IActionResult> GetTaskTags(int taskId)
        {
            try
            {
                var userId = GetUserId();
                var task = await _service.GetByIdAsync(taskId, userId);
                
                if (task == null)
                    return NotFound(new { error = "Task not found." });

                var tags = task.TaskTags.Select(tt => new DTOs.Tag.TagReadDto
                {
                    Id = tt.Tag.Id,
                    Name = tt.Tag.Name,
                    Color = tt.Tag.Color,
                    CreatedAt = tt.Tag.CreatedAt,
                    TaskCount = tt.Tag.TaskTags.Count
                }).ToList();

                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Add a tag to a task
        /// </summary>
        [HttpPost("{taskId}/tags/{tagId}")]
        public async Task<IActionResult> AddTagToTask(int taskId, int tagId)
        {
            try
            {
                var userId = GetUserId();
                
                // Verify task exists and belongs to user
                var task = await _service.GetByIdAsync(taskId, userId);
                if (task == null)
                    return NotFound(new { error = "Task not found." });

                var success = await _tagRepository.AddTagToTaskAsync(taskId, tagId, userId);

                if (!success)
                    return BadRequest(new { error = "Unable to add tag. Tag may already be added or not found." });

                return Ok(new { message = "Tag added to task successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a tag from a task
        /// </summary>
        [HttpDelete("{taskId}/tags/{tagId}")]
        public async Task<IActionResult> RemoveTagFromTask(int taskId, int tagId)
        {
            try
            {
                var userId = GetUserId();
                
                // Verify task exists and belongs to user
                var task = await _service.GetByIdAsync(taskId, userId);
                if (task == null)
                    return NotFound(new { error = "Task not found." });

                var success = await _tagRepository.RemoveTagFromTaskAsync(taskId, tagId, userId);

                if (!success)
                    return NotFound(new { error = "Tag not found on this task." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
