namespace TaskManagement.Api.DTOs.Tag
{
    public class TagCreateDto
    {
        public string Name { get; set; } = null!;
        public string Color { get; set; } = "#6b7280";
    }

    public class TagReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int TaskCount { get; set; }
    }

    public class TagUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;
    }
}
