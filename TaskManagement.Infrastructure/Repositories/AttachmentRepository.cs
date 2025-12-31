using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly AppDbContext _context;

        public AttachmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskAttachment>> GetTaskAttachmentsAsync(int taskId)
        {
            return await _context.TaskAttachments
                .AsNoTracking()
                .Include(a => a.UploadedByUser)
                .Where(a => a.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<TaskAttachment> CreateAsync(TaskAttachment attachment)
        {
            _context.TaskAttachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<TaskAttachment?> GetByIdAsync(int id, int taskId)
        {
            return await _context.TaskAttachments
                .AsNoTracking()
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.Id == id && a.TaskId == taskId);
        }

        public async Task<bool> DeleteAsync(int id, int taskId, string userId)
        {
            var attachment = await _context.TaskAttachments
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.Id == id && a.TaskId == taskId);

            if (attachment == null) return false;

            // Permission: Uploader or Task Owner
            if (attachment.UploadedByUserId != userId && attachment.Task.UserId != userId)
                return false;

            _context.TaskAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
