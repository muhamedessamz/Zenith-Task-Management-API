using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface INotificationService
    {
        Task NotifyTaskAssignedAsync(TaskItem task, string assignerName, string assigneeId);
        Task NotifyTaskCompletedAsync(TaskItem task, string completedByUserId);
        Task NotifyCommentAddedAsync(Comment comment, TaskItem task, string commenterName);
        Task NotifyProjectMemberAddedAsync(Project project, string addedUserId, string addedByUserName);
    }
}
