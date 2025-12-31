using TaskManagement.Core.Enums;

namespace TaskManagement.Api.DTOs
{
    public class TaskCreateDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDate { get; set; }
        public int? CategoryId { get; set; }
        public int? ProjectId { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;
        
        // List of user IDs to assign this task to
        public List<string>? AssignedUserIds { get; set; }
    }
}
