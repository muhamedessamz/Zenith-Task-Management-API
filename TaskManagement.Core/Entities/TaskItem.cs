using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        // User relationship
        public string UserId { get; set; } = null!;
        public User? User { get; set; }

        // Category relationship
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Project relationship
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;
        public DateTime? NextOccurrence { get; set; }
        
        // Kanban Fields
        public string Status { get; set; } = "Todo"; // Todo, InProgress, Done
        public int OrderIndex { get; set; } = 0;

        public string? ExternalCalendarEventId { get; set; } // Google/Outlook Event ID

        // Checklist items
        public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

        // Tags (many-to-many)
        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();

        // Comments
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Assignments
        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();

        // Attachments
        public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    }
}
