using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Services
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly AppDbContext _context;

        public TaskAssignmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskAssignment> AssignUserAsync(int taskId, string assignedToUserId, string assignedByUserId, string note = null, string permission = "Editor")
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) throw new KeyNotFoundException("Task not found.");

            // Check if user exists
            var user = await _context.Users.FindAsync(assignedToUserId);
            // If passed as email or username? For now assume ID as per frontend logic
            if (user == null)
            {
                 // Try to find by email if ID fails
                 user = await _context.Users.FirstOrDefaultAsync(u => u.Email == assignedToUserId);
            }
            if (user == null) throw new ArgumentException("User not found.");

            // Check if already assigned
            var existing = await _context.TaskAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == user.Id);
            
            if (existing != null)
            {
                // Update existing? Or just return it?
                // Let's update permission/note and return
                existing.Permission = permission;
                existing.Note = note;
                await _context.SaveChangesAsync();
                return existing;
            }

            var assignment = new TaskAssignment
            {
                TaskId = taskId,
                AssignedToUserId = user.Id,
                AssignedByUserId = assignedByUserId,
                AssignedAt = DateTime.UtcNow,
                Note = note,
                Permission = permission
            };

            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Load relationships for return
            await _context.Entry(assignment).Reference(a => a.AssignedToUser).LoadAsync();
            
            if (assignment.AssignedToUser == null)
            {
                 // Fallback if load fails (shouldn't happen if FK is valid)
                 assignment.AssignedToUser = user;
            }

            return assignment;
        }

        public async Task<bool> RemoveAssignmentAsync(int taskId, int assignmentId, string requestedByUserId)
        {
            var assignment = await _context.TaskAssignments
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.TaskId == taskId);

            if (assignment == null) return false;

            // Optional: Check permissions (only owner or creator or the assignee can remove?)
            // For now allow any authenticated user who has access to the task 
            // (Assuming controller handles task access check)
            
            _context.TaskAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskAssignment?> GetAssignmentAsync(int taskId, string userId)
        {
            return await _context.TaskAssignments
                .AsNoTracking()
                .Include(a => a.AssignedToUser)
                .Include(a => a.AssignedByUser)
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == userId);
        }

        public async Task<IEnumerable<TaskAssignment>> GetTaskAssignmentsAsync(int taskId)
        {
            return await _context.TaskAssignments
                .AsNoTracking()
                .Include(a => a.AssignedToUser)
                .Include(a => a.AssignedByUser)
                .Where(a => a.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksAssignedToUserAsync(string userId)
        {
            return await _context.TaskAssignments
                .Include(a => a.Task)
                .Where(a => a.AssignedToUserId == userId)
                .Select(a => a.Task!)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskAssignment>> GetUserAssignmentsAsync(string userId)
        {
            return await _context.TaskAssignments
                .AsNoTracking()
                .Include(a => a.Task)
                .Include(a => a.AssignedByUser)
                .Where(a => a.AssignedToUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksAssignedByUserAsync(string userId)
        {
            return await _context.TaskAssignments
                .Include(a => a.Task)
                .Where(a => a.AssignedByUserId == userId)
                .Select(a => a.Task!)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskAssignment>> AssignMultipleUsersAsync(int taskId, IEnumerable<string> userIds, string assignedByUserId, string permission = "Editor")
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) throw new KeyNotFoundException("Task not found.");

            var assignments = new List<TaskAssignment>();

            foreach (var userId in userIds)
            {
                // Check if already assigned
                var existing = await _context.TaskAssignments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == userId);
                
                if (existing == null)
                {
                    var assignment = new TaskAssignment
                    {
                        TaskId = taskId,
                        AssignedToUserId = userId,
                        AssignedByUserId = assignedByUserId,
                        AssignedAt = DateTime.UtcNow,
                        Permission = permission
                    };
                    
                    _context.TaskAssignments.Add(assignment);
                    assignments.Add(assignment);
                }
                else
                {
                    assignments.Add(existing);
                }
            }
            
            await _context.SaveChangesAsync();
            return assignments;
        }
    }
}
