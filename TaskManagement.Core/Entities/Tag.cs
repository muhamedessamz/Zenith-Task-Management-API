namespace TaskManagement.Core.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = "#6b7280"; // Default gray
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // User relationship
        public string UserId { get; set; } = null!;
        public User? User { get; set; }

        // Many-to-many with Tasks
        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }

    // Junction table for many-to-many relationship
    public class TaskTag
    {
        public int TaskId { get; set; }
        public TaskItem? Task { get; set; }

        public int TagId { get; set; }
        public Tag? Tag { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
