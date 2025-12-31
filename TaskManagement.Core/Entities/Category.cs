namespace TaskManagement.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6366f1"; // Default indigo color
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // User relationship
        public string UserId { get; set; } = null!;
        public User? User { get; set; }

        // Navigation property
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
