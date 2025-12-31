using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface ITimeTrackingService
    {
        Task<TimeEntry> StartTimerAsync(int taskId, string userId);
        Task<TimeEntry> StopTimerAsync(int taskId, string userId);
        Task<TimeEntry> LogManualAsync(int taskId, string userId, DateTime start, DateTime end, string? notes);
        Task<IEnumerable<TimeEntry>> GetTaskHistoryAsync(int taskId);
        Task<TimeSpan> GetTotalTimeSpentAsync(int taskId);
    }
}
