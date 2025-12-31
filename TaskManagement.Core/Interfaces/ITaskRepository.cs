using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetAllAsync(string userId);
        Task<(List<TaskItem> Items, int TotalCount)> GetPagedAsync(string userId, int page, int pageSize);
        Task<TaskItem?> GetByIdAsync(int id, string userId);
        Task<TaskItem> AddAsync(TaskItem item);
        Task<TaskItem> UpdateAsync(TaskItem item);
        Task DeleteAsync(int id);

        Task<List<TaskItem>> GetByCompletionStatusAsync(bool isCompleted, string userId);
        Task<List<TaskItem>> GetAllSortedByDueDateAsync(bool ascending, string userId);
        Task<List<TaskItem>> GetByPriorityAsync(TaskPriority priority, string userId);
        
        // Search & Advanced Filtering
        Task<List<TaskItem>> SearchAsync(string userId, string query);
        Task<List<TaskItem>> AdvancedFilterAsync(string userId, TaskPriority? priority, int? categoryId, bool? isCompleted);
        Task<List<TaskItem>> GetByDateRangeAsync(string userId, DateTime? from, DateTime? to);
    }
}


