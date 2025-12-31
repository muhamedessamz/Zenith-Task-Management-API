using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync(string userId);
        Task<Category?> GetByIdAsync(int id, string userId);
        Task<Category> CreateAsync(Category category, string userId);
        Task<Category> UpdateAsync(Category category, string userId);
        Task DeleteAsync(int id, string userId);
    }
}
