namespace TaskManagement.Core.Entities
{
    public class ProjectMember
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public string UserId { get; set; } = null!;
        public User? User { get; set; }

        // Role: "Owner", "Editor", "Viewer"
        // Owner: Full control
        // Editor: Can edit project & tasks
        // Viewer: Can view, comment, mark complete only
        public string Role { get; set; } = "Viewer";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
