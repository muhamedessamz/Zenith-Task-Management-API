using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Services
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;

        public OtpService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAndStoreOtpAsync(string email, string purpose)
        {
            // Delete old OTPs for this email and purpose
            var oldOtps = await _context.EmailOtps
                .Where(o => o.Email == email && o.Purpose == purpose)
                .ToListAsync();
            _context.EmailOtps.RemoveRange(oldOtps);

            // Generate new OTP
            var otpCode = new Random().Next(100000, 999999).ToString();
            var otp = new EmailOtp
            {
                Email = email,
                OtpCode = otpCode,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            _context.EmailOtps.Add(otp);
            await _context.SaveChangesAsync();

            return otpCode;
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp, string purpose)
        {
            var emailOtp = await _context.EmailOtps
                .FirstOrDefaultAsync(o =>
                    o.Email == email &&
                    o.OtpCode == otp &&
                    o.Purpose == purpose &&
                    !o.IsUsed &&
                    o.ExpiresAt > DateTime.UtcNow);

            if (emailOtp == null)
                return false;

            emailOtp.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteOtpAsync(string email, string purpose)
        {
            var otps = await _context.EmailOtps
                .Where(o => o.Email == email && o.Purpose == purpose)
                .ToListAsync();

            if (!otps.Any())
                return false;

            _context.EmailOtps.RemoveRange(otps);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
