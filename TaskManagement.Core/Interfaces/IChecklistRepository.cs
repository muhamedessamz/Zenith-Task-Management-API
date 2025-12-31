using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface IChecklistRepository
    {
        Task<List<ChecklistItem>> GetByTaskIdAsync(int taskId);
        Task<ChecklistItem?> GetByIdAsync(int id);
        Task<ChecklistItem> AddAsync(ChecklistItem item);
        Task<ChecklistItem> UpdateAsync(ChecklistItem item);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
