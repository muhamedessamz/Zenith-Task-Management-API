using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskDependencyService
    {
        Task AddDependencyAsync(int taskId, int dependsOnTaskId);
        Task RemoveDependencyAsync(int taskId, int dependsOnTaskId);
        Task<IEnumerable<TaskDependency>> GetDependenciesAsync(int taskId, string userId);
        Task<bool> IsTaskBlockedAsync(int taskId);
        Task<IEnumerable<TaskItem>> GetBlockersAsync(int taskId);
    }
}
