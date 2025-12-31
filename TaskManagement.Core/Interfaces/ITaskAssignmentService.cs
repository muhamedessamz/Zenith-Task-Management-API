using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskAssignmentService
    {
        Task<TaskAssignment> AssignUserAsync(int taskId, string assignedToUserIdentifier, string assignedByUserId, string note = null, string permission = "Editor");
        Task<bool> RemoveAssignmentAsync(int taskId, int assignmentId, string requestedByUserId);
        Task<IEnumerable<TaskAssignment>> GetTaskAssignmentsAsync(int taskId);
        Task<IEnumerable<TaskItem>> GetTasksAssignedToUserAsync(string userId);
        Task<IEnumerable<TaskItem>> GetTasksAssignedByUserAsync(string userId);
        Task<IEnumerable<TaskAssignment>> AssignMultipleUsersAsync(int taskId, IEnumerable<string> userIds, string assignedByUserId, string permission = "Editor");
    }
}
