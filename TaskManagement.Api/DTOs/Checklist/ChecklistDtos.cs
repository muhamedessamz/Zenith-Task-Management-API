namespace TaskManagement.Api.DTOs.Checklist
{
    public class ChecklistItemCreateDto
    {
        public string Title { get; set; } = null!;
        public int Order { get; set; }
    }

    public class ChecklistItemReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskId { get; set; }
    }

    public class ChecklistItemUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public int Order { get; set; }
    }
}
