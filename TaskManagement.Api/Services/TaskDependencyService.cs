using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Services
{
    public class TaskDependencyService : ITaskDependencyService
    {
        private readonly AppDbContext _context;

        public TaskDependencyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddDependencyAsync(int taskId, int dependsOnTaskId)
        {
            if (taskId == dependsOnTaskId)
                throw new ArgumentException("Task cannot depend on itself.");

            // Check existence
            var taskExists = await _context.Tasks.AsNoTracking().AnyAsync(t => t.Id == taskId);
            var dependencyExists = await _context.Tasks.AsNoTracking().AnyAsync(t => t.Id == dependsOnTaskId);
            if (!taskExists || !dependencyExists)
                throw new KeyNotFoundException("One or both tasks not found.");

            // Check if already exists
            var exists = await _context.TaskDependencies
                .AsNoTracking()
                .AnyAsync(d => d.TaskId == taskId && d.DependsOnTaskId == dependsOnTaskId);
            if (exists) return; // Idempotent

            // Check Circular (Simple A->B->A check)
            // Ideally should traverse deeper, but for now strict direct cycle check
            var reverseExists = await _context.TaskDependencies
                .AsNoTracking()
                .AnyAsync(d => d.TaskId == dependsOnTaskId && d.DependsOnTaskId == taskId);
            if (reverseExists)
                throw new InvalidOperationException("Circular dependency detected.");

            var dependency = new TaskDependency
            {
                TaskId = taskId,
                DependsOnTaskId = dependsOnTaskId
            };

            _context.TaskDependencies.Add(dependency);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveDependencyAsync(int taskId, int dependsOnTaskId)
        {
            var dep = await _context.TaskDependencies
                .FirstOrDefaultAsync(d => d.TaskId == taskId && d.DependsOnTaskId == dependsOnTaskId);

            if (dep != null)
            {
                _context.TaskDependencies.Remove(dep);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TaskDependency>> GetDependenciesAsync(int taskId, string userId)
        {
            return await _context.TaskDependencies
                .AsNoTracking()
                .Include(d => d.DependsOnTask)
                .Where(d => d.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<bool> IsTaskBlockedAsync(int taskId)
        {
            // Blocked if ANY dependency is NOT completed
            return await _context.TaskDependencies
                .AsNoTracking()
                .Include(d => d.DependsOnTask)
                .AnyAsync(d => d.TaskId == taskId && !d.DependsOnTask.IsCompleted);
        }

        public async Task<IEnumerable<TaskItem>> GetBlockersAsync(int taskId)
        {
            return await _context.TaskDependencies
                .AsNoTracking()
                .Where(d => d.TaskId == taskId && !d.DependsOnTask.IsCompleted)
                .Select(d => d.DependsOnTask)
                .ToListAsync();
        }
    }
}
