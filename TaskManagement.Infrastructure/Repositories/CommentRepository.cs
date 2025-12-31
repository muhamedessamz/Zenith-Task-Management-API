using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Comment>> GetTaskCommentsAsync(int taskId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> GetByIdAsync(int id, int taskId, string userId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Include(c => c.Task)
                .FirstOrDefaultAsync(c => c.Id == id && c.TaskId == taskId && c.UserId == userId);
        }

        public async Task<Comment> UpdateAsync(Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteAsync(int id, int taskId, string userId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id && c.TaskId == taskId && c.UserId == userId);

            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
