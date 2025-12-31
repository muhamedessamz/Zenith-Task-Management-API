using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Api.DTOs.Attachment;
using TaskManagement.Api.Services;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/attachments")]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IFileService _fileService;
        private readonly UserManager<User> _userManager;
        private readonly ITaskService _taskService;

        public AttachmentsController(
            IAttachmentRepository attachmentRepository, 
            IFileService fileService, 
            UserManager<User> userManager,
            ITaskService taskService)
        {
            _attachmentRepository = attachmentRepository;
            _fileService = fileService;
            _userManager = userManager;
            _taskService = taskService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        [HttpPost]
        public async Task<IActionResult> Upload(int taskId, IFormFile file)
        {
            // 1. Validation
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (file.Length > 10 * 1024 * 1024) // 10MB
                return BadRequest("File size exceeds the maximum limit of 10MB.");

            var userId = GetUserId();
            var task = await _taskService.GetByIdAsync(taskId, userId);

            if (task == null) return NotFound("Task not found.");

            // 2. Save File
            var filePath = await _fileService.SaveFileAsync(file);

            // 3. Save Entity
            var attachment = new TaskAttachment
            {
                TaskId = taskId,
                FileName = file.FileName,
                StoredFileName = Path.GetFileName(filePath),
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow
            };

            var createdAttachment = await _attachmentRepository.CreateAsync(attachment);

            // Return DTO
            var uploader = await _userManager.FindByIdAsync(userId);
            return Ok(new AttachmentDto
            {
                Id = createdAttachment.Id,
                FileName = createdAttachment.FileName,
                FilePath = createdAttachment.FilePath,
                ContentType = createdAttachment.ContentType,
                FileSize = createdAttachment.FileSize,
                UploadedAt = createdAttachment.UploadedAt,
                UploadedBy = uploader?.DisplayName ?? "Unknown"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int taskId)
        {
             var userId = GetUserId();
             var task = await _taskService.GetByIdAsync(taskId, userId);

             if (task == null) return NotFound();
             
             var attachments = await _attachmentRepository.GetTaskAttachmentsAsync(taskId);

             var attachmentDtos = attachments.Select(a => new AttachmentDto
             {
                 Id = a.Id,
                 FileName = a.FileName,
                 FilePath = a.FilePath,
                 ContentType = a.ContentType,
                 FileSize = a.FileSize,
                 UploadedAt = a.UploadedAt,
                 UploadedBy = a.UploadedByUser != null ? a.UploadedByUser.DisplayName : "Unknown"
             }).ToList();

            return Ok(attachmentDtos);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int taskId, int id)
        {
            var userId = GetUserId();
            var attachment = await _attachmentRepository.GetByIdAsync(id, taskId);

            if (attachment == null) return NotFound();

            // Permission: Uploader or Task Owner
            if (attachment.UploadedByUserId != userId && attachment.Task.UserId != userId)
                return Forbid("Only the uploader or task owner can delete this attachment.");

            // Delete file from disk
            _fileService.DeleteFile(attachment.FilePath);

            // Delete from DB
            var deleted = await _attachmentRepository.DeleteAsync(id, taskId, userId);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
