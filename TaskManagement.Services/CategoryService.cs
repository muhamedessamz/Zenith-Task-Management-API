using TaskManagement.Core.Entities;
using TaskManagement.Core.Exceptions;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo) => _repo = repo;

        public async Task<List<Category>> GetAllAsync(string userId) =>
            await _repo.GetAllAsync(userId);

        public async Task<Category?> GetByIdAsync(int id, string userId) =>
            await _repo.GetByIdAsync(id, userId);

        public async Task<Category> CreateAsync(Category category, string userId)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ValidationException("Category name is required.");

            if (category.Name.Length > 50)
                throw new ValidationException("Category name cannot exceed 50 characters.");

            if (!string.IsNullOrEmpty(category.Color) && !IsValidHexColor(category.Color))
                throw new ValidationException("Invalid color format. Use hex format (e.g., #6366f1).");

            category.UserId = userId;
            category.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(category);
        }

        public async Task<Category> UpdateAsync(Category category, string userId)
        {
            var existing = await _repo.GetByIdAsync(category.Id, userId);
            if (existing == null)
                throw new NotFoundException("Category", category.Id);

            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ValidationException("Category name is required.");

            if (category.Name.Length > 50)
                throw new ValidationException("Category name cannot exceed 50 characters.");

            if (!string.IsNullOrEmpty(category.Color) && !IsValidHexColor(category.Color))
                throw new ValidationException("Invalid color format. Use hex format (e.g., #6366f1).");

            existing.Name = category.Name;
            existing.Description = category.Description;
            existing.Color = category.Color;

            return await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var existing = await _repo.GetByIdAsync(id, userId);
            if (existing == null)
                throw new NotFoundException("Category", id);

            await _repo.DeleteAsync(id);
        }

        private bool IsValidHexColor(string color)
        {
            if (string.IsNullOrEmpty(color)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(color, "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
        }
    }
}
