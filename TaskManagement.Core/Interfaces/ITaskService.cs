using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Models;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetAllAsync(string userId);
        Task<PagedResult<TaskItem>> GetPagedAsync(string userId, int page, int pageSize);
        Task<TaskItem?> GetByIdAsync(int id, string userId);
        Task<TaskItem> CreateAsync(TaskItem item, string userId);
        Task<TaskItem> UpdateAsync(TaskItem item, string userId);
        Task DeleteAsync(int id, string userId);

        Task<List<TaskItem>> FilterByCompletionAsync(bool isCompleted, string userId);
        Task<List<TaskItem>> SortByDueDateAsync(bool ascending, string userId);
        Task<List<TaskItem>> FilterByPriorityAsync(TaskPriority priority, string userId);
        
        // Search & Advanced Filtering
        Task<List<TaskItem>> SearchAsync(string userId, string query);
        Task<List<TaskItem>> AdvancedFilterAsync(string userId, TaskPriority? priority, int? categoryId, bool? isCompleted);
        Task<List<TaskItem>> GetByDateRangeAsync(string userId, DateTime? from, DateTime? to);
        
        // Kanban Board
        Task<bool> UpdateStatusAsync(int taskId, string newStatus, string userId);
        Task<bool> UpdatePositionAsync(int taskId, int newOrderIndex, string userId);
        Task<Dictionary<string, List<TaskItem>>> GetKanbanBoardAsync(string userId, int? projectId = null);
    }
}


