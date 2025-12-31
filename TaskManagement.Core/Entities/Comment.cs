namespace TaskManagement.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Task relationship
        public int TaskId { get; set; }
        public TaskItem? Task { get; set; }

        // User relationship
        public string UserId { get; set; } = null!;
        public User? User { get; set; }
    }
}
