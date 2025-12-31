namespace TaskManagement.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose);
        Task SendTaskAssignmentNotificationAsync(string toEmail, string taskTitle, string assignedBy);
        Task SendCommentNotificationAsync(string toEmail, string taskTitle, string commenterName, string comment);
    }
}
