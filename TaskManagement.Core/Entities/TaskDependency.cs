using System;

namespace TaskManagement.Core.Entities
{
    public class TaskDependency
    {
        // The dependent task (the one that waits)
        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        // The prerequisite task (the one that needs to be finished)
        public int DependsOnTaskId { get; set; }
        public TaskItem DependsOnTask { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
