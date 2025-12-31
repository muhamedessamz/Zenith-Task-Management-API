using System;

namespace TaskManagement.Core.Entities
{
    public class SharedLink
    {
        public int Id { get; set; }
        
        // The secret part of the URL (e.g. /public/task/abcd1234xyz)
        public string Token { get; set; } = Guid.NewGuid().ToString("N"); 
        
        // What are we sharing?
        public string EntityType { get; set; } = "Task"; // "Task", "Project"
        public int EntityId { get; set; } 
        
        // Options
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; } 
        public bool IsActive { get; set; } = true;
        public string? PasswordHash { get; set; } // Optional password protection
        
        // Audit
        public string CreatedByUserId { get; set; } = null!;
        public User CreatedBy { get; set; } = null!;
        
        public int ViewCount { get; set; } = 0;
    }
}
