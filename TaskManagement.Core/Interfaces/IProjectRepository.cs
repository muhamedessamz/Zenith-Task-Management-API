using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync();
        Task<List<Project>> GetUserProjectsAsync(string userId);
        Task<Project?> GetByIdAsync(int id);
        Task<Project> CreateAsync(Project project, string userId);
        Task<Project> UpdateAsync(Project project);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // Project Members
        Task<List<ProjectMember>> GetProjectMembersAsync(int projectId);
        Task<ProjectMember> AddMemberAsync(int projectId, string userId, string role);
        Task<bool> RemoveMemberAsync(int projectId, string userId);
        Task<ProjectMember?> GetMemberAsync(int projectId, string userId);
        
        // User lookup
        Task<User?> FindUserByIdentifierAsync(string identifier);
    }
}
