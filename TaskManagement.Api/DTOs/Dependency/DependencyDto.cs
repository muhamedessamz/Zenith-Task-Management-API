namespace TaskManagement.Api.DTOs.Dependency
{
    public class DependencyDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public string? AssignedTo { get; set; }
    }
}
