using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Api.DTOs.User;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;
        private readonly ITaskService _taskService;

        public CommentsController(
            ICommentRepository commentRepository, 
            INotificationService notificationService, 
            UserManager<User> userManager,
            ITaskService taskService)
        {
            _commentRepository = commentRepository;
            _notificationService = notificationService;
            _userManager = userManager;
            _taskService = taskService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetAll(int taskId)
        {
            var userId = GetUserId();
            var task = await _taskService.GetByIdAsync(taskId, userId);
            
            if (task == null) return NotFound();

            var comments = await _commentRepository.GetTaskCommentsAsync(taskId);
            
            var commentDtos = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UserId = c.UserId,
                User = c.User == null ? null : new UserSummaryDto
                {
                     Id = c.User.Id,
                     Email = c.User.Email ?? "",
                     UserName = c.User.UserName ?? c.User.Email ?? "",
                     FirstName = c.User.FirstName ?? "",
                     LastName = c.User.LastName ?? "",
                     DisplayName = c.User.DisplayName ?? "",
                     ProfilePicture = c.User.ProfilePicture
                }
            }).ToList();

            return Ok(commentDtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int taskId, [FromBody] CommentCreateDto dto)
        {
            var userId = GetUserId();
            var task = await _taskService.GetByIdAsync(taskId, userId);
            
            if (task == null) return NotFound();

            var comment = new Comment
            {
                Content = dto.Content,
                TaskId = taskId,
                UserId = userId
            };

            var createdComment = await _commentRepository.CreateAsync(comment);

            // Send notification
            var commenter = await _userManager.FindByIdAsync(userId);
            var commenterName = commenter?.DisplayName ?? commenter?.FirstName ?? "Someone";
            
            await _notificationService.NotifyCommentAddedAsync(createdComment, task, commenterName);

            return Ok(new CommentDto
            { 
                Id = createdComment.Id, 
                Content = createdComment.Content, 
                CreatedAt = createdComment.CreatedAt, 
                UserId = userId,
                User = new UserSummaryDto
                {
                    Id = commenter!.Id,
                    Email = commenter.Email!,
                    UserName = commenter.UserName ?? commenter.Email!,
                    FirstName = commenter.FirstName,
                    LastName = commenter.LastName,
                    DisplayName = commenter.DisplayName ?? "",
                    ProfilePicture = commenter.ProfilePicture
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int taskId, int id, [FromBody] CommentUpdateDto dto)
        {
            var userId = GetUserId();
            var comment = await _commentRepository.GetByIdAsync(id, taskId, userId);

            if (comment == null) return NotFound();

            comment.Content = dto.Content;
            await _commentRepository.UpdateAsync(comment);
            
            return Ok(new { comment.Id, comment.Content, comment.UpdatedAt });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int taskId, int id)
        {
            var userId = GetUserId();
            var deleted = await _commentRepository.DeleteAsync(id, taskId, userId);

            if (!deleted) return NotFound();

            return NoContent();
        }
    }

    public class CommentCreateDto
    {
        public string Content { get; set; } = null!;
    }

    public class CommentUpdateDto
    {
        public string Content { get; set; } = null!;
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; } = null!;
        public TaskManagement.Api.DTOs.User.UserSummaryDto User { get; set; } = null!;
    }
}
