namespace TaskManagement.Api.DTOs.Category
{
    public class CategoryCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6366f1";
    }

    public class CategoryReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Color { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = null!;
        public int TaskCount { get; set; }
    }

    public class CategoryUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Color { get; set; } = null!;
    }
}
