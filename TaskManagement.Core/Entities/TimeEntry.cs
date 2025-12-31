using System;

namespace TaskManagement.Core.Entities
{
    public class TimeEntry
    {
        public int Id { get; set; }
        
        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        public string? Notes { get; set; }
        public bool IsManual { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
