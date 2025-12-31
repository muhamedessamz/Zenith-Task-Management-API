using System.Threading.Tasks;

namespace TaskManagement.Core.Interfaces
{
    public interface ICalendarService
    {
        // 1. Generate the Google Auth URL for the user to visit
        string GetAuthUri(string userId);

        // 2. Exchange the returned code for tokens
        Task<bool> ExchangeCodeForTokenAsync(string userId, string code);

        // 3. Sync a specific task (Create or Update)
        Task SyncTaskToCalendarAsync(int taskId);

        // 4. Remove event if task is deleted or sync disabled
        Task DeleteEventAsync(int taskId);

        // 5. Get calendar connection status for a user
        Task<(bool IsConnected, DateTime? ConnectedAt)> GetConnectionStatusAsync(string userId);

        // 6. Disconnect calendar for a user
        Task<bool> DisconnectAsync(string userId);
    }
}
