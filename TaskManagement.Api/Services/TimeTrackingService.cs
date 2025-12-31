using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly AppDbContext _context;

        public TimeTrackingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TimeEntry> StartTimerAsync(int taskId, string userId)
        {
            // Verify task exists
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) throw new KeyNotFoundException("Task not found.");

            // Check globally if user has any running timer
            var anyRunningTimer = await _context.TimeEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == userId && t.EndTime == null);
            
            if (anyRunningTimer != null)
                 throw new InvalidOperationException($"You already have a running timer on task #{anyRunningTimer.TaskId}. Stop it first.");

            var entry = new TimeEntry
            {
                TaskId = taskId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                IsManual = false
            };

            _context.TimeEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<TimeEntry> StopTimerAsync(int taskId, string userId)
        {
            var runningTimer = await _context.TimeEntries
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.UserId == userId && t.EndTime == null);

            if (runningTimer == null)
                throw new InvalidOperationException("No running timer found for this task.");

            runningTimer.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return runningTimer;
        }

        public async Task<TimeEntry> LogManualAsync(int taskId, string userId, DateTime start, DateTime end, string? notes)
        {
            // Verify task exists
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) throw new KeyNotFoundException("Task not found.");

            if (end <= start)
                throw new ArgumentException("End time must be after start time.");

            var entry = new TimeEntry
            {
                TaskId = taskId,
                UserId = userId,
                StartTime = start,
                EndTime = end,
                Notes = notes,
                IsManual = true
            };

            _context.TimeEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<IEnumerable<TimeEntry>> GetTaskHistoryAsync(int taskId)
        {
            return await _context.TimeEntries
                .AsNoTracking()
                .Where(t => t.TaskId == taskId)
                .Include(t => t.User)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<TimeSpan> GetTotalTimeSpentAsync(int taskId)
        {
            var entries = await _context.TimeEntries
                .AsNoTracking()
                .Where(t => t.TaskId == taskId && t.EndTime != null)
                .ToListAsync();

            double totalSeconds = entries.Sum(e => (e.EndTime!.Value - e.StartTime).TotalSeconds);
            return TimeSpan.FromSeconds(totalSeconds);
        }
    }
}
