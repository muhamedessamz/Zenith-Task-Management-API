using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.DTOs.Auth;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly UserManager<User> _userManager;

        public AuthController(
            IAuthService authService, 
            IEmailService emailService, 
            IOtpService otpService,
            UserManager<User> userManager)
        {
            _authService = authService;
            _emailService = emailService;
            _otpService = otpService;
            _userManager = userManager;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(new { error = "Passwords do not match." });
            }

            var (success, message, user) = await _authService.RegisterAsync(
                dto.Email, dto.Password, dto.FirstName, dto.LastName);

            if (!success)
            {
                return BadRequest(new { error = message });
            }

            // Send verification OTP
            var otpCode = await _otpService.GenerateAndStoreOtpAsync(user!.Email!, "EmailVerification");

            await _emailService.SendOtpEmailAsync(user.Email!, otpCode, "EmailVerification");

            return Ok(new { 
                message = "Registration successful. Please check your email for verification code.",
                email = user.Email
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var (success, message, user) = await _authService.LoginAsync(dto.Email, dto.Password);

            if (!success)
            {
                return Unauthorized(new { error = message });
            }

            // ✅ Check if email is verified
            if (!user!.EmailConfirmed)
            {
                return StatusCode(403, new
                {
                    error = "Please verify your email before logging in.",
                    message = "Please verify your email before logging in.",
                    requiresVerification = true,
                    email = user.Email
                });
            }

            var token = await _authService.GenerateJwtTokenAsync(user!);
            var refreshToken = await _authService.GenerateRefreshTokenAsync();

            var response = new AuthResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = 1800, // 30 minutes
                TokenType = "Bearer",
                User = new UserDto
                {
                    Id = user!.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };

            return Ok(response);
        }

        [HttpPost("send-verification-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendVerificationOtp([FromBody] SendOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new { error = "User not found" });

            // Generate and send new OTP
            var otpCode = await _otpService.GenerateAndStoreOtpAsync(dto.Email, "EmailVerification");

            await _emailService.SendOtpEmailAsync(dto.Email, otpCode, "EmailVerification");

            return Ok(new { message = "OTP sent to your email" });
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpDto dto)
        {
            var isValid = await _otpService.VerifyOtpAsync(dto.Email, dto.OtpCode, "EmailVerification");

            if (!isValid)
                return BadRequest(new { error = "Invalid or expired OTP" });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            return Ok(new { message = "Email verified successfully" });
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] SendOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new { error = "User not found" });

            if (user.EmailConfirmed)
                return BadRequest(new { error = "Email already verified" });

            // Generate and send new OTP
            var otpCode = await _otpService.GenerateAndStoreOtpAsync(dto.Email, "EmailVerification");

            await _emailService.SendOtpEmailAsync(dto.Email, otpCode, "EmailVerification");

            return Ok(new { message = "Verification code resent successfully" });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] SendOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Ok(new { message = "If the email exists, you will receive a password reset code" });

            // Generate and send new OTP
            var otpCode = await _otpService.GenerateAndStoreOtpAsync(dto.Email, "PasswordReset");

            await _emailService.SendOtpEmailAsync(dto.Email, otpCode, "PasswordReset");

            // Return code for development (remove in production)
            return Ok(new { 
                message = "If the email exists, you will receive a password reset code",
                code = otpCode  // For development only
            });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var isValid = await _otpService.VerifyOtpAsync(dto.Email, dto.OtpCode, "PasswordReset");

            if (!isValid)
                return BadRequest(new { error = "Invalid or expired OTP" });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { error = "User not found" });

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { error = "Failed to reset password", errors = result.Errors });

            return Ok(new { message = "Password reset successfully" });
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }

    // DTOs
    public class SendOtpDto
    {
        public string Email { get; set; } = null!;
    }

    public class VerifyOtpDto
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
