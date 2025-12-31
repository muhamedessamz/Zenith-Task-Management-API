namespace TaskManagement.Api.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionRate { get; set; }
        
        public int TasksCreatedToday { get; set; }
        public int TasksCreatedThisWeek { get; set; }
        public int TasksCreatedThisMonth { get; set; }
        
        public PriorityStatsDto PriorityStats { get; set; } = new();
        public CategoryStatsDto CategoryStats { get; set; } = new();
        public List<DailyTaskCountDto> TasksPerDay { get; set; } = new();
    }

    public class PriorityStatsDto
    {
        public int Low { get; set; }
        public int Medium { get; set; }
        public int High { get; set; }
        public int Critical { get; set; }
    }

    public class CategoryStatsDto
    {
        public List<CategoryTaskCountDto> Categories { get; set; } = new();
        public int Uncategorized { get; set; }
    }

    public class CategoryTaskCountDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string Color { get; set; } = null!;
        public int TaskCount { get; set; }
        public int CompletedCount { get; set; }
    }

    public class DailyTaskCountDto
    {
        public DateTime Date { get; set; }
        public int TasksCreated { get; set; }
        public int TasksCompleted { get; set; }
    }
}
