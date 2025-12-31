namespace TaskManagement.Core.Entities
{
    public class TaskAttachment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;
        
        public string FileName { get; set; } = null!; // Original name, e.g., "report.pdf"
        public string StoredFileName { get; set; } = null!; // Unique name, e.g., "guid.pdf"
        public string FilePath { get; set; } = null!; // e.g., "/uploads/attachments/guid.pdf"
        public string ContentType { get; set; } = null!; // e.g., "application/pdf"
        public long FileSize { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public string UploadedByUserId { get; set; } = null!;
        public User? UploadedByUser { get; set; }
    }
}
