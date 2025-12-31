using System.ComponentModel.DataAnnotations;
using TaskManagement.Core.Enums;

namespace TaskManagement.Api.DTOs.Project
{
    public class ProjectCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public int? CategoryId { get; set; }
    }

    public class ProjectUpdateDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public TaskPriority? Priority { get; set; }

        public bool? IsCompleted { get; set; }

        public int? CategoryId { get; set; }
    }

    public class ProjectReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Priority { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string OwnerId { get; set; } = null!;
        public string OwnerName { get; set; } = null!;

        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int MemberCount { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public double Progress { get; set; }
        
        // Permissions for the current user requesting this DTO
        public string? CurrentUserRole { get; set; } 
    }

    public class ProjectMemberDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Email { get; set; }
        public string? ProfilePicture { get; set; }
        public string Role { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
    }

    public class AddProjectMemberDto
    {
        [Required]
        public string UserIdentifier { get; set; } = null!; // Email or DisplayName

        [Required]
        public string Role { get; set; } = "Viewer"; // Viewer, Editor, Owner
    }
    
    public class UpdateProjectMemberRoleDto
    {
        [Required]
        public string Role { get; set; } = "Viewer";
    }
}
