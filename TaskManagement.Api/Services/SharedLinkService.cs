using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Services
{
    public class SharedLinkService : ISharedLinkService
    {
        private readonly AppDbContext _context;

        public SharedLinkService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SharedLink> GenerateLinkAsync(string userId, string entityType, int entityId, DateTime? expiry = null)
        {
            if (entityType != "Task" && entityType != "Project") 
                throw new ArgumentException("Invalid entity type. Must be 'Task' or 'Project'.");

            // Verify existence and ownership
            if (entityType == "Task")
            {
                 var task = await _context.Tasks.FindAsync(entityId);
                 if (task == null) throw new KeyNotFoundException("Task not found.");
                 // In a real app, check if user has permission to share (e.g. is owner or collaborator)
                 if (task.UserId != userId) throw new UnauthorizedAccessException("You do not have permission to share this task.");
            }
            // Add Project logic here later

            var token = Guid.NewGuid().ToString("N");
            // Ensure token is unique (highly probable, but good practice)
            while(await _context.SharedLinks.AsNoTracking().AnyAsync(s => s.Token == token)) 
            {
                token = Guid.NewGuid().ToString("N");
            }

            var link = new SharedLink
            {
                Token = token,
                EntityType = entityType,
                EntityId = entityId,
                CreatedByUserId = userId,
                ExpiresAt = expiry,
                IsActive = true
            };

            _context.SharedLinks.Add(link);
            await _context.SaveChangesAsync();
            return link;
        }

        public async Task<SharedLink?> GetLinkAsync(string token)
        {
            var link = await _context.SharedLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Token == token);
            
            if (link == null) return null;
            if (!link.IsActive) return null;
            if (link.ExpiresAt.HasValue && link.ExpiresAt < DateTime.UtcNow) return null;
            
            return link;
        }

        public async Task<object?> GetPayloadAsync(string token)
        {
            var link = await GetLinkAsync(token);
            if (link == null) return null;

            // Increment view count
            try {
                // Use a separate context or attach to update view count to avoid tracking issues if needed
                // For simplicity, we use same context but watch out for concurrency
                link.ViewCount++;
                await _context.SaveChangesAsync();
            } catch {} // Ignore ViewCount errors, not critical

            if (link.EntityType == "Task")
            {
                var taskData = await _context.Tasks
                    .Include(t => t.User)
                    .Include(t => t.Category)
                    .Include(t => t.Project)
                    .AsNoTracking()
                    .Where(t => t.Id == link.EntityId)
                    .Select(t => new { 
                        t.Id, 
                        t.Title, 
                        t.Description, 
                        t.Priority, 
                        t.DueDate, 
                        t.IsCompleted, 
                        Status = t.IsCompleted ? "Completed" : "Pending",
                        CreatedBy = t.User != null ? t.User.DisplayName : "Unknown", 
                        CategoryName = t.Category != null ? t.Category.Name : "Uncategorized",
                        ProjectTitle = t.Project != null ? t.Project.Title : null,
                        t.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                 if (taskData != null)
                 {
                     return new { EntityType = "Task", Data = taskData };
                 }
            }
            
            return null;
        }

        public async Task<bool> RevokeLinkAsync(string token, string userId)
        {
            var link = await _context.SharedLinks.FirstOrDefaultAsync(l => l.Token == token);
            if (link == null) return false;
            
            if (link.CreatedByUserId != userId) 
                throw new UnauthorizedAccessException("You are not the creator of this link.");
            
            link.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
