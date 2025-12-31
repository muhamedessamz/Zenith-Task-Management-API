using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface ITagRepository
    {
        Task<List<Tag>> GetAllAsync(string userId);
        Task<Tag?> GetByIdAsync(int id, string userId);
        Task<Tag> AddAsync(Tag tag);
        Task<Tag> UpdateAsync(Tag tag);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id, string userId);
        Task<bool> AddTagToTaskAsync(int taskId, int tagId, string userId);
        Task<bool> RemoveTagFromTaskAsync(int taskId, int tagId, string userId);
    }
}
