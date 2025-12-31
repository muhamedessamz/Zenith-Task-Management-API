using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<List<TaskAttachment>> GetTaskAttachmentsAsync(int taskId);
        Task<TaskAttachment> CreateAsync(TaskAttachment attachment);
        Task<TaskAttachment?> GetByIdAsync(int id, int taskId);
        Task<bool> DeleteAsync(int id, int taskId, string userId);
    }
}
