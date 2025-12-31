using TaskManagement.Core.Entities;
using TaskManagement.Core.Exceptions;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Services
{
    public class ChecklistService : IChecklistService
    {
        private readonly IChecklistRepository _checklistRepo;
        private readonly ITaskRepository _taskRepo;

        public ChecklistService(IChecklistRepository checklistRepo, ITaskRepository taskRepo)
        {
            _checklistRepo = checklistRepo;
            _taskRepo = taskRepo;
        }

        public async Task<List<ChecklistItem>> GetByTaskIdAsync(int taskId, string userId)
        {
            // Verify task belongs to user
            var task = await _taskRepo.GetByIdAsync(taskId, userId);
            if (task == null)
                throw new NotFoundException("Task", taskId);

            return await _checklistRepo.GetByTaskIdAsync(taskId);
        }

        public async Task<ChecklistItem> AddAsync(int taskId, ChecklistItem item, string userId)
        {
            // Verify task belongs to user
            var task = await _taskRepo.GetByIdAsync(taskId, userId);
            if (task == null)
                throw new NotFoundException("Task", taskId);

            if (string.IsNullOrWhiteSpace(item.Title))
                throw new ValidationException("Checklist item title is required.");

            item.TaskId = taskId;
            item.CreatedAt = DateTime.UtcNow;
            return await _checklistRepo.AddAsync(item);
        }

        public async Task<ChecklistItem> UpdateAsync(ChecklistItem item, string userId)
        {
            var existing = await _checklistRepo.GetByIdAsync(item.Id);
            if (existing == null)
                throw new NotFoundException("Checklist item", item.Id);

            // Verify task belongs to user
            var task = await _taskRepo.GetByIdAsync(existing.TaskId, userId);
            if (task == null)
                throw new UnauthorizedException("You don't have permission to update this checklist item.");

            if (string.IsNullOrWhiteSpace(item.Title))
                throw new ValidationException("Checklist item title is required.");

            existing.Title = item.Title;
            existing.IsCompleted = item.IsCompleted;
            existing.Order = item.Order;

            return await _checklistRepo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var existing = await _checklistRepo.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException("Checklist item", id);

            // Verify task belongs to user
            var task = await _taskRepo.GetByIdAsync(existing.TaskId, userId);
            if (task == null)
                throw new UnauthorizedException("You don't have permission to delete this checklist item.");

            await _checklistRepo.DeleteAsync(id);
        }

        public async Task<ChecklistItem> ToggleCompletionAsync(int id, string userId)
        {
            var existing = await _checklistRepo.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException("Checklist item", id);

            // Verify task belongs to user
            var task = await _taskRepo.GetByIdAsync(existing.TaskId, userId);
            if (task == null)
                throw new UnauthorizedException("You don't have permission to update this checklist item.");

            existing.IsCompleted = !existing.IsCompleted;
            return await _checklistRepo.UpdateAsync(existing);
        }
    }
}
