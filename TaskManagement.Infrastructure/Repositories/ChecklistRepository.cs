using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class ChecklistRepository : IChecklistRepository
    {
        private readonly AppDbContext _context;

        public ChecklistRepository(AppDbContext context) => _context = context;

        public async Task<List<ChecklistItem>> GetByTaskIdAsync(int taskId)
        {
            return await _context.ChecklistItems
                .AsNoTracking()
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.Order)
                .ToListAsync();
        }

        public async Task<ChecklistItem?> GetByIdAsync(int id)
        {
            return await _context.ChecklistItems
                .AsNoTracking()
                .Include(c => c.Task)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ChecklistItem> AddAsync(ChecklistItem item)
        {
            var entry = await _context.ChecklistItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<ChecklistItem> UpdateAsync(ChecklistItem item)
        {
            _context.ChecklistItems.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ChecklistItems.FindAsync(id);
            if (entity == null) return;
            _context.ChecklistItems.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ChecklistItems
                .AsNoTracking()
                .AnyAsync(c => c.Id == id);
        }
    }
}
