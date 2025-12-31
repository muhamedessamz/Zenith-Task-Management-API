namespace TaskManagement.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<object> GetDashboardStatsAsync(string userId);
        Task<object> GetTasksPerDayAsync(string userId, int days = 7);
    }
}
