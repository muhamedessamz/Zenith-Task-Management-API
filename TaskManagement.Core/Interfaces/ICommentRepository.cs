using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetTaskCommentsAsync(int taskId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment?> GetByIdAsync(int id, int taskId, string userId);
        Task<Comment> UpdateAsync(Comment comment);
        Task<bool> DeleteAsync(int id, int taskId, string userId);
    }
}
