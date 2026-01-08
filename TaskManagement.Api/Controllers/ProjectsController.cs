using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Api.DTOs.Project;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskService _taskService;

        public ProjectsController(IProjectRepository projectRepository, ITaskService taskService)
        {
            _projectRepository = projectRepository;
            _taskService = taskService;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectReadDto>>> GetAllProjects()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            Console.WriteLine($"=== GetAllProjects called for user: {userId} ===");

            // Get only projects where the user is a member
            var projects = await _projectRepository.GetUserProjectsAsync(userId);

            Console.WriteLine($"Found {projects.Count} projects for user {userId}");
            foreach (var p in projects)
            {
                Console.WriteLine($"  - Project: {p.Title} (ID: {p.Id}, Owner: {p.UserId})");
                Console.WriteLine($"    Members: {string.Join(", ", p.Members.Select(m => m.UserId))}");
            }

            var projectDtos = projects.Select(p => MapToReadDto(p, userId)).ToList();

            return Ok(projectDtos);
        }

        // GET: api/projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectReadDto>> GetProject(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            // Check access - Allow viewing for all authenticated users
            // But role will be "Viewer" for non-members
            bool isMember = project.Members.Any(m => m.UserId == userId);
            bool isOwner = project.UserId == userId;

            // Everyone can view, but role determines permissions
            // No need to block access here
            
            return Ok(MapToReadDto(project, userId));
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<ProjectReadDto>> CreateProject(ProjectCreateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var project = new Project
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Priority = dto.Priority,
                    UserId = userId,
                    CategoryId = dto.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdProject = await _projectRepository.CreateAsync(project, userId);

                return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, MapToReadDto(createdProject, userId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null) return NotFound();

            // Permission Check: Owner or Editor can update project details
            var member = project.Members.FirstOrDefault(m => m.UserId == userId);
            bool isOwner = project.UserId == userId || (member != null && member.Role == "Owner");
            bool isEditor = member != null && member.Role == "Editor";

            if (!isOwner && !isEditor)
            {
                return Forbid("You do not have permission to edit this project.");
            }

            if (dto.Title != null) project.Title = dto.Title;
            if (dto.Description != null) project.Description = dto.Description;
            if (dto.StartDate.HasValue) project.StartDate = dto.StartDate;
            if (dto.EndDate.HasValue) project.EndDate = dto.EndDate;
            if (dto.Priority.HasValue) project.Priority = dto.Priority.Value;
            if (dto.CategoryId.HasValue) project.CategoryId = dto.CategoryId;
            
            if (dto.IsCompleted.HasValue && dto.IsCompleted != project.IsCompleted)
            {
                project.IsCompleted = dto.IsCompleted.Value;
                project.CompletedAt = project.IsCompleted ? DateTime.UtcNow : null;
            }

            try
            {
                await _projectRepository.UpdateAsync(project);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _projectRepository.ExistsAsync(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null) return NotFound();

            // Permission Check: Only Owner can delete
            bool isOwner = project.UserId == userId;
            if (!isOwner)
            {
                var member = project.Members.FirstOrDefault(m => m.UserId == userId);
                if (member == null || member.Role != "Owner") 
                    return Forbid("Only the project owner can delete it.");
            }

            try
            {
                await _projectRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the error (Serilog is configured)
                Console.WriteLine($"Error deleting project {id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new { message = "An error occurred while deleting the project.", error = ex.Message });
            }
        }

        private async Task<bool> ProjectExists(int id)
        {
            return await _projectRepository.ExistsAsync(id);
        }

        private ProjectReadDto MapToReadDto(Project project, string currentUserId)
        {
            var tasks = project.Tasks ?? new List<TaskItem>();
            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.IsCompleted);
            double progress = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;

            string role = "Viewer";
            if (project.UserId == currentUserId) role = "Owner";
            else 
            {
                var member = project.Members?.FirstOrDefault(m => m.UserId == currentUserId);
                if (member != null) role = member.Role;
            }

            return new ProjectReadDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Priority = project.Priority.ToString(),
                IsCompleted = project.IsCompleted,
                CreatedAt = project.CreatedAt,
                CompletedAt = project.CompletedAt,
                OwnerId = project.UserId,
                OwnerName = project.User?.DisplayName ?? "Unknown",
                CategoryId = project.CategoryId,
                CategoryName = project.Category?.Name,
                MemberCount = project.Members?.Count ?? 0,
                TaskCount = totalTasks,
                CompletedTaskCount = completedTasks,
                Progress = Math.Round(progress, 1),
                CurrentUserRole = role
            };
        }
    
        // GET: api/projects/5/members
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetProjectMembers(int id)
    {
        try
        {
            Console.WriteLine($"=== GetProjectMembers called for project {id} ===");
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                Console.WriteLine("User not authenticated");
                return Unauthorized();
            }
            Console.WriteLine($"Current user: {userId}");

            // First, check if project exists
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                Console.WriteLine($"Project {id} not found");
                return NotFound();
            }
            Console.WriteLine($"Project found: {project.Title}");

            // Get project members
            var projectMembers = await _projectRepository.GetProjectMembersAsync(id);
            
            Console.WriteLine($"Total members in project: {projectMembers.Count}");


            // Allow all authenticated users to view members (public viewing)
            // No access restriction needed for viewing

            // Return empty list if no members
            if (!projectMembers.Any())
            {
                Console.WriteLine("No members found, returning empty list");
                return Ok(new List<ProjectMemberDto>());
            }

            // Map to DTOs
            var members = projectMembers.Select(pm => new ProjectMemberDto
            {
                Id = pm.Id,
                UserId = pm.UserId,
                DisplayName = pm.User?.DisplayName ?? $"{pm.User?.FirstName} {pm.User?.LastName}",
                Email = pm.User?.Email ?? "",
                ProfilePicture = pm.User?.ProfilePicture,
                Role = pm.Role,
                JoinedAt = pm.JoinedAt
            }).ToList();

            Console.WriteLine($"Returning {members.Count} members");
            return Ok(members);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in GetProjectMembers: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
            }
            return StatusCode(500, new { message = "An error occurred while fetching project members.", error = ex.Message, details = ex.InnerException?.Message });
        }
    }

        // POST: api/projects/5/members
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddProjectMember(int id, [FromBody] AddProjectMemberDto dto)
        {
            try
            {
                Console.WriteLine($"=== AddProjectMember called for project {id} ===");
                Console.WriteLine($"UserIdentifier: {dto.UserIdentifier}, Role: {dto.Role}");
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    Console.WriteLine("User not authenticated");
                    return Unauthorized();
                }
                Console.WriteLine($"Current user: {userId}");

                var project = await _projectRepository.GetByIdAsync(id);

                if (project == null)
                {
                    Console.WriteLine($"Project {id} not found");
                    return NotFound();
                }
                Console.WriteLine($"Project found: {project.Title}");

                // Permission: Only Owner can add members
                if (project.UserId != userId)
                {
                    var currentMember = project.Members.FirstOrDefault(m => m.UserId == userId);
                    if (currentMember == null || currentMember.Role != "Owner")
                    {
                        Console.WriteLine("User does not have permission to add members");
                        return Forbid("Only project owners can add members.");
                    }
                }

                Console.WriteLine($"Searching for user with identifier: {dto.UserIdentifier}");
                var userToAdd = await _projectRepository.FindUserByIdentifierAsync(dto.UserIdentifier);
                
                if (userToAdd == null)
                {
                    Console.WriteLine($"User not found with identifier: {dto.UserIdentifier}");
                    return BadRequest("User not found.");
                }
                Console.WriteLine($"User found: {userToAdd.Email} (ID: {userToAdd.Id})");

                if (project.Members.Any(m => m.UserId == userToAdd.Id))
                {
                    Console.WriteLine($"User {userToAdd.Email} is already a member");
                    return BadRequest("User is already a member of this project.");
                }

                Console.WriteLine($"Adding member: {userToAdd.Email} with role {dto.Role}");
                await _projectRepository.AddMemberAsync(id, userToAdd.Id, dto.Role);
                Console.WriteLine("Member added successfully");

                return Ok(new { message = "Member added successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddProjectMember: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "An error occurred while adding project member.", error = ex.Message });
            }
        }

        // DELETE: api/projects/5/members/userId
        [HttpDelete("{id}/members/{memberUserId}")]
        public async Task<IActionResult> RemoveProjectMember(int id, string memberUserId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null) return NotFound();

            bool isOwner = project.UserId == currentUserId || project.Members.Any(m => m.UserId == currentUserId && m.Role == "Owner");

            if (!isOwner && currentUserId != memberUserId)
            {
                return Forbid("You do not have permission to remove this member.");
            }

            if (project.UserId == memberUserId)
            {
                return BadRequest("Cannot remove the project owner.");
            }

            var memberToRemove = project.Members.FirstOrDefault(m => m.UserId == memberUserId);
            if (memberToRemove == null)
            {
                return NotFound("Member not found in this project.");
            }

            await _projectRepository.RemoveMemberAsync(id, memberUserId);

            return NoContent();
        }
    }
}
