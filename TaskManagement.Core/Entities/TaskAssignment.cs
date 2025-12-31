namespace TaskManagement.Core.Entities
{
    public class TaskAssignment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public TaskItem? Task { get; set; }

        public string AssignedToUserId { get; set; } = null!;
        public User? AssignedToUser { get; set; }

        public string AssignedByUserId { get; set; } = null!;
        public User? AssignedByUser { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
        
        // Permission: "Viewer" or "Editor"
        public string Permission { get; set; } = "Viewer";
    }
}
