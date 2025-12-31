using System.Threading.Tasks;
using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface ISharedLinkService
    {
        Task<SharedLink> GenerateLinkAsync(string userId, string entityType, int entityId, DateTime? expiry = null);
        Task<SharedLink?> GetLinkAsync(string token);
        Task<object?> GetPayloadAsync(string token); // Returns the actual Task or Project object
        Task<bool> RevokeLinkAsync(string token, string userId);
    }
}
