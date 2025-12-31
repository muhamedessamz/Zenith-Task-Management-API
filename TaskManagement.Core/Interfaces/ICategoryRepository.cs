using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(string userId);
        Task<Category?> GetByIdAsync(int id, string userId);
        Task<Category> AddAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id, string userId);
    }
}
