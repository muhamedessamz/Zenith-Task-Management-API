using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllAsync()
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project> CreateAsync(Project project, string userId)
        {
            // Validate category if provided
            if (project.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(project.CategoryId.Value);
                if (category == null)
                    throw new ArgumentException("Category not found");
            }

            _context.Projects.Add(project);

            // Add creator as member with 'Owner' role
            var member = new ProjectMember
            {
                Project = project,
                UserId = userId,
                Role = "Owner",
                JoinedAt = DateTime.UtcNow
            };
            _context.ProjectMembers.Add(member);

            await _context.SaveChangesAsync();

            // Load properties for return
            await _context.Entry(project).Reference(p => p.User).LoadAsync();
            if (project.CategoryId.HasValue)
                await _context.Entry(project).Reference(p => p.Category).LoadAsync();

            return project;
        }

        public async Task<Project> UpdateAsync(Project project)
        {
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return false;

            // Get all Task IDs for this project
            var taskIds = await _context.Tasks
                .Where(t => t.ProjectId == id)
                .Select(t => t.Id)
                .ToListAsync();

            if (taskIds.Any())
            {
                // Delete dependencies related to these tasks
                var dependencies = await _context.TaskDependencies
                    .Where(td => taskIds.Contains(td.TaskId) || taskIds.Contains(td.DependsOnTaskId))
                    .ToListAsync();
                _context.TaskDependencies.RemoveRange(dependencies);

                var timeEntries = await _context.TimeEntries
                    .Where(te => taskIds.Contains(te.TaskId))
                    .ToListAsync();
                _context.TimeEntries.RemoveRange(timeEntries);

                var comments = await _context.Comments
                    .Where(c => taskIds.Contains(c.TaskId))
                    .ToListAsync();
                _context.Comments.RemoveRange(comments);

                var attachments = await _context.TaskAttachments
                    .Where(a => taskIds.Contains(a.TaskId))
                    .ToListAsync();
                _context.TaskAttachments.RemoveRange(attachments);

                var checklistItems = await _context.ChecklistItems
                    .Where(ci => taskIds.Contains(ci.TaskId))
                    .ToListAsync();
                _context.ChecklistItems.RemoveRange(checklistItems);

                var taskTags = await _context.TaskTags
                    .Where(tt => taskIds.Contains(tt.TaskId))
                    .ToListAsync();
                _context.TaskTags.RemoveRange(taskTags);

                var assignments = await _context.TaskAssignments
                    .Where(ta => taskIds.Contains(ta.TaskId))
                    .ToListAsync();
                _context.TaskAssignments.RemoveRange(assignments);

                var tasks = await _context.Tasks
                    .Where(t => taskIds.Contains(t.Id))
                    .ToListAsync();
                _context.Tasks.RemoveRange(tasks);
            }

            // Delete Project Members
            var members = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == id)
                .ToListAsync();
            _context.ProjectMembers.RemoveRange(members);

            // Delete the Project
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Projects
                .AsNoTracking()
                .AnyAsync(p => p.Id == id);
        }

        // Project Members
        public async Task<List<ProjectMember>> GetProjectMembersAsync(int projectId)
        {
            // Single query with Include - more efficient than N+1
            return await _context.ProjectMembers
                .AsNoTracking()
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<ProjectMember> AddMemberAsync(int projectId, string userId, string role)
        {
            var newMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            _context.ProjectMembers.Add(newMember);
            await _context.SaveChangesAsync();

            return newMember;
        }

        public async Task<bool> RemoveMemberAsync(int projectId, string userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (member == null) return false;

            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectMember?> GetMemberAsync(int projectId, string userId)
        {
            return await _context.ProjectMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
        }

        public async Task<User?> FindUserByIdentifierAsync(string identifier)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == identifier || u.Id == identifier);
        }
    }
}
