using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.DTOs.Dashboard;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context) => _context = context;

        public async Task<object> GetDashboardStatsAsync(string userId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var totalTasks = tasks.Count;
            var completedTasks = tasks.Count(t => t.IsCompleted);
            var inProgressTasks = tasks.Count(t => !t.IsCompleted && t.DueDate.HasValue);
            var overdueTasks = tasks.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow);
            var completionRate = totalTasks > 0 ? Math.Round((double)completedTasks / totalTasks * 100, 2) : 0;

            // Calculate tasks created in different time periods
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddDays(-30);

            var tasksCreatedToday = tasks.Count(t => t.CreatedAt.Date == today);
            var tasksCreatedThisWeek = tasks.Count(t => t.CreatedAt >= weekAgo);
            var tasksCreatedThisMonth = tasks.Count(t => t.CreatedAt >= monthAgo);

            // Priority stats
            var priorityStats = new PriorityStatsDto
            {
                Low = tasks.Count(t => t.Priority == TaskPriority.Low),
                Medium = tasks.Count(t => t.Priority == TaskPriority.Medium),
                High = tasks.Count(t => t.Priority == TaskPriority.High),
                Critical = tasks.Count(t => t.Priority == TaskPriority.Critical)
            };

            // Category stats
            var categoryGroups = tasks
                .Where(t => t.CategoryId.HasValue)
                .GroupBy(t => new { t.CategoryId, t.Category!.Name, t.Category.Color })
                .Select(g => new CategoryTaskCountDto
                {
                    CategoryId = g.Key.CategoryId!.Value,
                    CategoryName = g.Key.Name,
                    Color = g.Key.Color,
                    TaskCount = g.Count(),
                    CompletedCount = g.Count(t => t.IsCompleted)
                })
                .ToList();

            var categoryStats = new CategoryStatsDto
            {
                Categories = categoryGroups,
                Uncategorized = tasks.Count(t => !t.CategoryId.HasValue)
            };

            // Tasks per day (last 7 days)
            var tasksPerDay = await GetTasksPerDayInternalAsync(userId, 7);

            return new DashboardStatsDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                OverdueTasks = overdueTasks,
                CompletionRate = completionRate,
                TasksCreatedToday = tasksCreatedToday,
                TasksCreatedThisWeek = tasksCreatedThisWeek,
                TasksCreatedThisMonth = tasksCreatedThisMonth,
                PriorityStats = priorityStats,
                CategoryStats = categoryStats,
                TasksPerDay = tasksPerDay
            };
        }

        public async Task<object> GetTasksPerDayAsync(string userId, int days = 7)
        {
            return await GetTasksPerDayInternalAsync(userId, days);
        }

        private async Task<List<DailyTaskCountDto>> GetTasksPerDayInternalAsync(string userId, int days)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId && t.CreatedAt >= startDate)
                .ToListAsync();

            var result = new List<DailyTaskCountDto>();
            for (int i = 0; i < days; i++)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var dayTasks = tasks.Where(t => t.CreatedAt.Date == date).ToList();
                
                result.Add(new DailyTaskCountDto
                {
                    Date = date,
                    TasksCreated = dayTasks.Count,
                    TasksCompleted = dayTasks.Count(t => t.IsCompleted)
                });
            }

            return result.OrderBy(d => d.Date).ToList();
        }
    }
}
