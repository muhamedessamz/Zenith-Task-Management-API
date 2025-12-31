using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password, string firstName, string lastName);
        Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync();
    }
}
