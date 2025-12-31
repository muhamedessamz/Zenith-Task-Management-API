namespace TaskManagement.Api.DTOs.TimeTracking
{
    public class TimeEntryDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        // Helper to format duration
        public string Duration => EndTime.HasValue 
            ? (EndTime.Value - StartTime).ToString(@"hh\:mm\:ss") 
            : "Running...";
        
        public string? Notes { get; set; }
        public bool IsManual { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!; // Display Name
        public DateTime CreatedAt { get; set; }
    }

    public class ManualTimeLogDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Notes { get; set; }
    }
}
