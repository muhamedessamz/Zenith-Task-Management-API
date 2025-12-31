using System;

namespace TaskManagement.Core.Entities
{
    public class UserCalendarIntegration
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public string Provider { get; set; } = "Google"; // e.g., "Google"
        public string ConnectedEmail { get; set; } = null!; // The gmail address
        
        // OAuth Tokens
        // IMPORTANT: In production, these fields should be Encrypted at Rest using Data Protection API
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime TokenExpiry { get; set; }

        public string ExternalCalendarId { get; set; } = "primary"; // The calendar ID to sync to (usually 'primary')
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSyncedAt { get; set; }
    }
}
