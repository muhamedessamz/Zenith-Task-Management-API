using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context) => _context = context;

        public async Task<List<TaskItem>> GetAllAsync(string userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .Where(t => t.UserId == userId || 
                           (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                           t.Assignments.Any(a => a.AssignedToUserId == userId))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<TaskItem> Items, int TotalCount)> GetPagedAsync(string userId, int page, int pageSize)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .Where(t => t.UserId == userId || 
                           (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                           t.Assignments.Any(a => a.AssignedToUserId == userId));
            
            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<TaskItem?> GetByIdAsync(int id, string userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.Id == id && 
                    (t.UserId == userId || 
                    (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                    t.Assignments.Any(a => a.AssignedToUserId == userId)));
        }

        public async Task<TaskItem> AddAsync(TaskItem item)
        {
            var entry = await _context.Tasks.AddAsync(item);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<TaskItem> UpdateAsync(TaskItem item)
        {
            _context.Tasks.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task DeleteAsync(int id)
        {
            // Remove all related entities first
            
            // 1. TaskAssignments
            var assignments = _context.TaskAssignments.Where(ta => ta.TaskId == id);
            _context.TaskAssignments.RemoveRange(assignments);

            // 2. Comments
            var comments = _context.Comments.Where(c => c.TaskId == id);
            _context.Comments.RemoveRange(comments);

            // 3. TaskAttachments (not Attachments!)
            var attachments = _context.TaskAttachments.Where(a => a.TaskId == id);
            _context.TaskAttachments.RemoveRange(attachments);

            // 4. TimeEntries
            var timeEntries = _context.TimeEntries.Where(te => te.TaskId == id);
            _context.TimeEntries.RemoveRange(timeEntries);

            // 5. ChecklistItems
            var checklistItems = _context.ChecklistItems.Where(ci => ci.TaskId == id);
            _context.ChecklistItems.RemoveRange(checklistItems);

            // 6. Dependencies (both directions)
            var dependencies = _context.TaskDependencies.Where(td => td.TaskId == id || td.DependsOnTaskId == id);
            _context.TaskDependencies.RemoveRange(dependencies);

            // Finally, remove the task itself
            var entity = await _context.Tasks.FindAsync(id);
            if (entity == null) return;
            _context.Tasks.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskItem>> GetByCompletionStatusAsync(bool isCompleted, string userId)
        {
            // Fixed: Filter in database instead of in-memory
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .Where(t => t.IsCompleted == isCompleted &&
                           (t.UserId == userId || 
                           (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                           t.Assignments.Any(a => a.AssignedToUserId == userId)))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetAllSortedByDueDateAsync(bool ascending, string userId)
        {
            // Fixed: Sort in database instead of in-memory
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .Where(t => t.UserId == userId || 
                           (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                           t.Assignments.Any(a => a.AssignedToUserId == userId));

            return ascending 
                ? await query.OrderBy(t => t.DueDate).ToListAsync()
                : await query.OrderByDescending(t => t.DueDate).ToListAsync();
        }

        public async Task<List<TaskItem>> GetByPriorityAsync(TaskPriority priority, string userId)
        {
            // Fixed: Filter in database instead of in-memory
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .Where(t => t.Priority == priority &&
                           (t.UserId == userId || 
                           (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                           t.Assignments.Any(a => a.AssignedToUserId == userId)))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // Search & Advanced Filtering
        public async Task<List<TaskItem>> SearchAsync(string userId, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync(userId);

            // Fixed: Search in database instead of in-memory
            query = query.ToLower();
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .Where(t => (t.Title.ToLower().Contains(query) || 
                            (t.Description != null && t.Description.ToLower().Contains(query))) &&
                           (t.UserId == userId || 
                           (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                           t.Assignments.Any(a => a.AssignedToUserId == userId)))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> AdvancedFilterAsync(string userId, TaskPriority? priority, int? categoryId, bool? isCompleted)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .Where(t => t.UserId == userId || 
                           (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                           t.Assignments.Any(a => a.AssignedToUserId == userId));

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            if (isCompleted.HasValue)
                query = query.Where(t => t.IsCompleted == isCompleted.Value);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetByDateRangeAsync(string userId, DateTime? from, DateTime? to)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Assignments)
                .Where(t => t.UserId == userId || 
                           (t.ProjectId != null && (t.Project.UserId == userId || t.Project.Members.Any(m => m.UserId == userId))) ||
                           t.Assignments.Any(a => a.AssignedToUserId == userId));

            if (from.HasValue)
                query = query.Where(t => t.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.CreatedAt <= to.Value);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
