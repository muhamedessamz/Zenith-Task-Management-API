using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _context;

        public TagRepository(AppDbContext context) => _context = context;

        public async Task<List<Tag>> GetAllAsync(string userId)
        {
            return await _context.Tags
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag?> GetByIdAsync(int id, string userId)
        {
            return await _context.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<Tag> AddAsync(Tag tag)
        {
            var entry = await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<Tag> UpdateAsync(Tag tag)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task DeleteAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null)
            {
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id, string userId)
        {
            return await _context.Tags
                .AsNoTracking()
                .AnyAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<bool> AddTagToTaskAsync(int taskId, int tagId, string userId)
        {
            // Verify tag belongs to user
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag == null || tag.UserId != userId)
                return false;

            // Verify task exists and user has access (basic check - task belongs to user)
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.UserId != userId)
                return false;

            // Check if relationship already exists
            var existingRelation = await _context.TaskTags
                .AsNoTracking()
                .FirstOrDefaultAsync(tt => tt.TaskId == taskId && tt.TagId == tagId);

            if (existingRelation != null)
                return false; // Already exists

            // Create relationship
            var taskTag = new TaskTag
            {
                TaskId = taskId,
                TagId = tagId,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskTags.Add(taskTag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTagFromTaskAsync(int taskId, int tagId, string userId)
        {
            // Verify task belongs to user
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.UserId != userId)
                return false;

            // Find and remove relationship
            var taskTag = await _context.TaskTags
                .FirstOrDefaultAsync(tt => tt.TaskId == taskId && tt.TagId == tagId);

            if (taskTag == null)
                return false;

            _context.TaskTags.Remove(taskTag);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
