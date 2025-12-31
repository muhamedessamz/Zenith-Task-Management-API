using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface IChecklistService
    {
        Task<List<ChecklistItem>> GetByTaskIdAsync(int taskId, string userId);
        Task<ChecklistItem> AddAsync(int taskId, ChecklistItem item, string userId);
        Task<ChecklistItem> UpdateAsync(ChecklistItem item, string userId);
        Task DeleteAsync(int id, string userId);
        Task<ChecklistItem> ToggleCompletionAsync(int id, string userId);
    }
}
