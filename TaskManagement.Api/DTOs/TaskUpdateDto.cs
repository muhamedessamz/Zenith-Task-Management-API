using TaskManagement.Core.Enums;

namespace TaskManagement.Api.DTOs
{
    public class TaskUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public int? CategoryId { get; set; }
        public int? ProjectId { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;
    }
}
