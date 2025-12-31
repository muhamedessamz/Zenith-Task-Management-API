using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using TaskManagement.Api.Hubs;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;

        public NotificationService(
            IHubContext<NotificationsHub> hubContext, 
            IEmailService emailService,
            UserManager<User> userManager)
        {
            _hubContext = hubContext;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task NotifyTaskAssignedAsync(TaskItem task, string assignerName, string assigneeId)
        {
            var user = await _userManager.FindByIdAsync(assigneeId);
            if (user == null) return;

            // 1. SignalR Notification
            await _hubContext.Clients.User(assigneeId).SendAsync("ReceiveNotification", new 
            {
                Type = "TaskAssigned",
                Message = $"You have been assigned to task: {task.Title} by {assignerName}",
                TaskId = task.Id,
                ProjectId = task.ProjectId,
                Timestamp = DateTime.UtcNow
            });

            // 2. Email Notification
            await _emailService.SendEmailAsync(
                user.Email!,
                "New Task Assignment",
                $@"
                <h3>Hello {user.DisplayName},</h3>
                <p>You have been assigned to a new task by <strong>{assignerName}</strong>.</p>
                <p><strong>Task:</strong> {task.Title}</p>
                <p><strong>Due Date:</strong> {task.DueDate?.ToString("g") ?? "No due date"}</p>
                <p><strong>Priority:</strong> {task.Priority}</p>
                <br/>
                <p>Please log in to your dashboard to view details.</p>"
            );
        }

        public async Task NotifyTaskCompletedAsync(TaskItem task, string completedByUserId)
        {
            // Notify Project Owner if applicable
            // Notify Task Creator if not the same as completer
            
            var completer = await _userManager.FindByIdAsync(completedByUserId);
            var completerName = completer?.DisplayName ?? "Someone";

            // If task is in a project, notify project owner? OR just task creator?
            // Let's notify Task creator for now.
            if (task.UserId != completedByUserId) // Don't notify self
            {
                var creator = await _userManager.FindByIdAsync(task.UserId);
                if (creator != null)
                {
                    // SignalR
                    await _hubContext.Clients.User(task.UserId).SendAsync("ReceiveNotification", new 
                    {
                        Type = "TaskCompleted",
                        Message = $"Task '{task.Title}' was completed by {completerName}",
                        TaskId = task.Id,
                        Timestamp = DateTime.UtcNow
                    });

                    // Email
                    await _emailService.SendEmailAsync(
                        creator.Email!,
                        "Task Completed",
                        $@"
                        <h3>Hello {creator.DisplayName},</h3>
                        <p>The task <strong>{task.Title}</strong> has been marked as completed by <strong>{completerName}</strong>.</p>
                        "
                    );
                }
            }
        }

        public async Task NotifyCommentAddedAsync(Comment comment, TaskItem task, string commenterName)
        {
             // Notify task owner
             if (task.UserId != comment.UserId)
             {
                var owner = await _userManager.FindByIdAsync(task.UserId);
                if (owner != null)
                {
                    await _hubContext.Clients.User(task.UserId).SendAsync("ReceiveNotification", new 
                    {
                        Type = "CommentAdded",
                        Message = $"{commenterName} commented on task: {task.Title}",
                        TaskId = task.Id,
                        CommentId = comment.Id,
                        Timestamp = DateTime.UtcNow
                    });

                     await _emailService.SendEmailAsync(
                        owner.Email!,
                        "New Comment on Task",
                        $@"
                        <h3>Hello {owner.DisplayName},</h3>
                        <p><strong>{commenterName}</strong> commented on task <strong>{task.Title}</strong>:</p>
                        <blockquote>{comment.Content}</blockquote>
                        "
                    );
                }
             }
             
             // Also notify assignees? (Omitted for simplicity, but good for future)
        }

        public async Task NotifyProjectMemberAddedAsync(Project project, string addedUserId, string addedByUserName)
        {
            var user = await _userManager.FindByIdAsync(addedUserId);
            if (user == null) return;

             // SignalR
            await _hubContext.Clients.User(addedUserId).SendAsync("ReceiveNotification", new 
            {
                Type = "ProjectInvite",
                Message = $"You have been added to project: {project.Title} by {addedByUserName}",
                ProjectId = project.Id,
                Timestamp = DateTime.UtcNow
            });

            // Email
            await _emailService.SendEmailAsync(
                user.Email!,
                "Welcome to Project",
                $@"
                <h3>Hello {user.DisplayName},</h3>
                <p>You have been added to the project <strong>{project.Title}</strong> by <strong>{addedByUserName}</strong>.</p>
                <p><strong>Project Description:</strong> {project.Description}</p>
                <br/>
                <p>Log in to view the project details.</p>"
            );
        }
    }
}
