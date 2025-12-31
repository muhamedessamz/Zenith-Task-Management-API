using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Settings;

namespace TaskManagement.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose)
    {
        var subject = purpose == "EmailVerification" 
            ? "Verify Your Email - Zenith" 
            : "Reset Your Password - Zenith";

        var body = GetOtpEmailTemplate(otpCode, purpose);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendTaskAssignmentNotificationAsync(string toEmail, string taskTitle, string assignedBy)
    {
        var subject = "New Task Assigned - Zenith";
        var body = GetTaskAssignmentTemplate(taskTitle, assignedBy);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendCommentNotificationAsync(string toEmail, string taskTitle, string commenterName, string comment)
    {
        var subject = "New Comment on Your Task - Zenith";
        var body = GetCommentNotificationTemplate(taskTitle, commenterName, comment);
        await SendEmailAsync(toEmail, subject, body);
    }

    private string GetOtpEmailTemplate(string otpCode, string purpose)
    {
        var title = purpose == "EmailVerification" ? "Verify Your Email" : "Reset Your Password";
        var icon = purpose == "EmailVerification" ? "✉️" : "🔑";
        
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>{title} - Zenith</title>
            </head>
            <body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background: linear-gradient(135deg, #f0f9ff 0%, #e0e7ff 100%);'>
                <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);'>
                    
                    <!-- Header with Gradient -->
                    <div style='background: linear-gradient(135deg, #6366f1 0%, #06b6d4 100%); padding: 50px 20px; text-align: center;'>
                        <h1 style='margin: 0; color: white; font-size: 48px; font-weight: 800; letter-spacing: 2px; text-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);'>ZENITH</h1>
                        <p style='margin: 12px 0 0 0; color: rgba(255, 255, 255, 0.95); font-size: 16px; font-weight: 500; letter-spacing: 1px;'>Reach the Peak</p>
                    </div>

                    <!-- Content -->
                    <div style='padding: 48px 32px; text-align: center;'>
                        <div style='font-size: 48px; margin-bottom: 16px;'>{icon}</div>
                        <h2 style='margin: 0 0 12px 0; color: #111827; font-size: 28px; font-weight: 700;'>{title}</h2>
                        <p style='margin: 0 0 32px 0; color: #6b7280; font-size: 16px; line-height: 1.6;'>
                            Enter this verification code to continue:
                        </p>

                        <!-- OTP Code Box -->
                        <div style='background: linear-gradient(135deg, #f0f9ff 0%, #e0e7ff 100%); border: 2px solid #e0e7ff; border-radius: 12px; padding: 24px; margin: 0 auto 32px auto; max-width: 280px;'>
                            <div style='font-size: 40px; font-weight: 800; color: #6366f1; letter-spacing: 8px; font-family: ""Courier New"", monospace;'>{otpCode}</div>
                        </div>

                        <!-- Info Box -->
                        <div style='display: inline-block; background: #fef3c7; border-left: 4px solid #f59e0b; padding: 12px 20px; border-radius: 8px; margin: 32px auto; text-align: center;'>
                            <p style='margin: 0; color: #92400e; font-size: 14px; line-height: 1.6;'>
                                <strong>⏱️ Important:</strong> This code will expire in <strong>10 minutes</strong>.
                            </p>
                        </div>

                        <p style='margin: 0; color: #9ca3af; font-size: 14px; line-height: 1.6;'>
                            If you didn't request this code, please ignore this email or contact support if you have concerns.
                        </p>
                    </div>

                    <!-- Footer -->
                    <div style='background: #f9fafb; padding: 32px; text-align: center; border-top: 1px solid #e5e7eb;'>
                        <p style='margin: 0 0 8px 0; color: #6b7280; font-size: 13px;'>
                            © 2025 <strong style='color: #6366f1;'>Zenith</strong>. All rights reserved.
                        </p>
                        <p style='margin: 0; color: #9ca3af; font-size: 12px;'>
                            Task Management System
                        </p>
                    </div>
                </div>

                <!-- Bottom Spacing -->
                <div style='height: 40px;'></div>
            </body>
            </html>";
    }

    private string GetTaskAssignmentTemplate(string taskTitle, string assignedBy)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
                <div style='max-width: 600px; margin: 50px auto; background-color: #ffffff; padding: 20px; border-radius: 10px;'>
                    <div style='background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1>📋 New Task Assigned</h1>
                    </div>
                    <div style='padding: 30px;'>
                        <p>Hello,</p>
                        <p><strong>{assignedBy}</strong> has assigned you a new task:</p>
                        <div style='font-size: 20px; font-weight: bold; color: #1f2937; margin: 20px 0;'>&quot;{taskTitle}&quot;</div>
                        <p>Please log in to Zenith to view the details.</p>
                    </div>
                    <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
                        <p>&copy; 2025 Zenith. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string GetCommentNotificationTemplate(string taskTitle, string commenterName, string comment)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
                <div style='max-width: 600px; margin: 50px auto; background-color: #ffffff; padding: 20px; border-radius: 10px;'>
                    <div style='background-color: #8b5cf6; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1>💬 New Comment</h1>
                    </div>
                    <div style='padding: 30px;'>
                        <p>Hello,</p>
                        <p><strong>{commenterName}</strong> commented on your task:</p>
                        <div style='font-size: 18px; font-weight: bold; color: #1f2937; margin: 15px 0;'>&quot;{taskTitle}&quot;</div>
                        <div style='background-color: #f9fafb; padding: 15px; border-left: 4px solid #8b5cf6; margin: 20px 0;'>
                            <p>{comment}</p>
                        </div>
                        <p>Please log in to Zenith to reply.</p>
                    </div>
                    <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
                        <p>&copy; 2025 Zenith. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
