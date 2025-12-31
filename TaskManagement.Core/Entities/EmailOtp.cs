namespace TaskManagement.Core.Entities
{
    public class EmailOtp
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public string Purpose { get; set; } = null!; // "EmailVerification" or "PasswordReset"
    }
}
