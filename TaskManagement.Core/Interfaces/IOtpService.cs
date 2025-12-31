namespace TaskManagement.Core.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(string email, string purpose);
        Task<bool> VerifyOtpAsync(string email, string otp, string purpose);
        Task<bool> DeleteOtpAsync(string email, string purpose);
    }
}
