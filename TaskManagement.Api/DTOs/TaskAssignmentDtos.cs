using TaskManagement.Api.DTOs.User;

namespace TaskManagement.Api.DTOs
{
    public class AssignTaskRequest
    {
        public string UserIdentifier { get; set; } = null!;
        public string? Note { get; set; }
        public string Permission { get; set; } = "Editor";
    }

    public class TaskAssignmentDto
    {
         public int Id { get; set; }
         public int TaskId { get; set; }
         public UserSummaryDto AssignedTo { get; set; } = null!;
         public DateTime AssignedAt { get; set; }
         public string? Note { get; set; }
         public string Permission { get; set; } = "Viewer";
    }
}
