using TaskManagement.Core.Enums;
using TaskManagement.Api.DTOs.Tag;

namespace TaskManagement.Api.DTOs
{
    public class TaskReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string UserId { get; set; } = null!;
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public DateTime? NextOccurrence { get; set; }
        public List<TaskAssignmentDto> Assignments { get; set; } = new List<TaskAssignmentDto>();
        public List<TagReadDto> Tags { get; set; } = new List<TagReadDto>();
    }
}
