using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models;

namespace TaskManagement.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repo;

        public TaskService(ITaskRepository repo) => _repo = repo;

        public async Task<List<TaskItem>> GetAllAsync(string userId) => 
            await _repo.GetAllAsync(userId);

        public async Task<PagedResult<TaskItem>> GetPagedAsync(string userId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Maximum page size

            var (items, totalCount) = await _repo.GetPagedAsync(userId, page, pageSize);
            return new PagedResult<TaskItem>(items, totalCount, page, pageSize);
        }

        public async Task<TaskItem?> GetByIdAsync(int id, string userId) => 
            await _repo.GetByIdAsync(id, userId);

        public async Task<TaskItem> CreateAsync(TaskItem item, string userId)
        {
            if (string.IsNullOrWhiteSpace(item.Title))
                throw new ArgumentException("Title is required.");

            if (item.DueDate.HasValue && item.DueDate.Value <= DateTime.UtcNow)
                throw new ArgumentException("DueDate must be in the future.");

            item.UserId = userId;
            item.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(item);
        }

        public async Task<TaskItem> UpdateAsync(TaskItem item, string userId)
        {
            var existing = await _repo.GetByIdAsync(item.Id, userId);
            if (existing == null) 
                throw new KeyNotFoundException("Task not found or you don't have permission to update it.");

            // Note: We don't validate DueDate on update because users should be able to:
            // 1. Complete old tasks that are overdue
            // 2. Update tasks without changing the due date
            // The validation only applies when creating new tasks

            // Handle Completion timestamp
            if (item.IsCompleted && !existing.IsCompleted)
            {
                existing.CompletedAt = DateTime.UtcNow;
            }
            else if (!item.IsCompleted)
            {
                existing.CompletedAt = null;
            }

            existing.Title = item.Title;
            existing.Description = item.Description;
            existing.IsCompleted = item.IsCompleted;
            existing.Priority = item.Priority;
            existing.DueDate = item.DueDate;
            existing.CategoryId = item.CategoryId;
            existing.ProjectId = item.ProjectId;

            return await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var existing = await _repo.GetByIdAsync(id, userId);
            if (existing == null)
                throw new KeyNotFoundException("Task not found or you don't have permission to delete it.");

            await _repo.DeleteAsync(id);
        }

        public async Task<List<TaskItem>> FilterByCompletionAsync(bool isCompleted, string userId) =>
            await _repo.GetByCompletionStatusAsync(isCompleted, userId);

        public async Task<List<TaskItem>> SortByDueDateAsync(bool ascending, string userId) =>
            await _repo.GetAllSortedByDueDateAsync(ascending, userId);

        public async Task<List<TaskItem>> FilterByPriorityAsync(TaskPriority priority, string userId) =>
            await _repo.GetByPriorityAsync(priority, userId);

        // Search & Advanced Filtering
        public async Task<List<TaskItem>> SearchAsync(string userId, string query) =>
            await _repo.SearchAsync(userId, query);

        public async Task<List<TaskItem>> AdvancedFilterAsync(string userId, TaskPriority? priority, int? categoryId, bool? isCompleted) =>
            await _repo.AdvancedFilterAsync(userId, priority, categoryId, isCompleted);

        public async Task<List<TaskItem>> GetByDateRangeAsync(string userId, DateTime? from, DateTime? to) =>
            await _repo.GetByDateRangeAsync(userId, from, to);

        // Kanban Board Methods
        public async Task<bool> UpdateStatusAsync(int taskId, string newStatus, string userId)
        {
            var validStatuses = new[] { "Todo", "InProgress", "Done" };
            if (!validStatuses.Contains(newStatus))
                throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

            var task = await _repo.GetByIdAsync(taskId, userId);
            if (task == null)
                throw new KeyNotFoundException("Task not found or access denied.");

            task.Status = newStatus;
            
            // Update IsCompleted for backward compatibility
            task.IsCompleted = (newStatus == "Done");
            
            await _repo.UpdateAsync(task);
            return true;
        }

        public async Task<bool> UpdatePositionAsync(int taskId, int newOrderIndex, string userId)
        {
            var task = await _repo.GetByIdAsync(taskId, userId);
            if (task == null)
                throw new KeyNotFoundException("Task not found or access denied.");

            var oldIndex = task.OrderIndex;
            task.OrderIndex = newOrderIndex;
            
            await _repo.UpdateAsync(task);
            
            // Shift other tasks if needed (simple implementation)
            // In production, you'd want to reorder all tasks in the same status column
            return true;
        }

        public async Task<Dictionary<string, List<TaskItem>>> GetKanbanBoardAsync(string userId, int? projectId = null)
        {
            var allTasks = projectId.HasValue 
                ? (await _repo.GetAllAsync(userId)).Where(t => t.ProjectId == projectId).ToList()
                : await _repo.GetAllAsync(userId);

            var board = new Dictionary<string, List<TaskItem>>
            {
                ["Todo"] = allTasks.Where(t => t.Status == "Todo").OrderBy(t => t.OrderIndex).ToList(),
                ["InProgress"] = allTasks.Where(t => t.Status == "InProgress").OrderBy(t => t.OrderIndex).ToList(),
                ["Done"] = allTasks.Where(t => t.Status == "Done").OrderBy(t => t.OrderIndex).ToList()
            };

            return board;
        }
    }
}
