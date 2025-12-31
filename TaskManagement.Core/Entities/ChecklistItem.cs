namespace TaskManagement.Core.Entities
{
    public class ChecklistItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; } = false;
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Task relationship
        public int TaskId { get; set; }
        public TaskItem? Task { get; set; }
    }
}
