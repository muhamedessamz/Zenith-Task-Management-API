using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Core.Entities;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UserController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.DisplayName,
                user.ProfilePicture,
                user.CreatedAt
            });
        }

        /// <summary>
        /// Update DisplayName
        /// </summary>
        [HttpPut("DisplayName")]
        public async Task<IActionResult> UpdateDisplayName([FromBody] UpdateDisplayNameDto dto)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            // Check if DisplayName is already taken
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.DisplayName == dto.DisplayName && u.Id != userId);

            if (existingUser != null)
                return BadRequest(new { error = "DisplayName is already taken" });

            user.DisplayName = dto.DisplayName;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { error = "Failed to update DisplayName", errors = result.Errors });

            return Ok(new { message = "DisplayName updated successfully", DisplayName = user.DisplayName });
        }

        /// <summary>
        /// Update email
        /// </summary>
        [HttpPut("email")]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto dto)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            // Check if email is already taken
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null && existingUser.Id != userId)
                return BadRequest(new { error = "Email is already in use" });

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, dto.Email);
            var result = await _userManager.ChangeEmailAsync(user, dto.Email, token);

            if (!result.Succeeded)
                return BadRequest(new { error = "Failed to update email", errors = result.Errors });

            // Update UserName as well (since we use email as username for login)
            user.UserName = dto.Email;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Email updated successfully", email = user.Email });
        }

        /// <summary>
        /// Update password
        /// </summary>
        [HttpPut("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { error = "Failed to update password", errors = result.Errors });

            return Ok(new { message = "Password updated successfully" });
        }

        /// <summary>
        /// Update profile picture (URL)
        /// </summary>
        [HttpPut("profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureDto dto)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            user.ProfilePicture = dto.ProfilePictureUrl;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { error = "Failed to update profile picture", errors = result.Errors });

            return Ok(new { message = "Profile picture updated successfully", profilePicture = user.ProfilePicture });
        }

        /// <summary>
        /// Upload profile picture (File)
        /// </summary>
        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { error = "Invalid file type. Only JPG, PNG, and GIF are allowed" });

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "File size must be less than 5MB" });

            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Delete old profile picture if exists
                if (!string.IsNullOrEmpty(user.ProfilePicture) && user.ProfilePicture.StartsWith("/uploads/"))
                {
                    try
                    {
                        // Handle both forward and back slashes for compatibility
                        var relativePath = user.ProfilePicture.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with upload
                        Console.WriteLine($"Failed to delete old profile picture: {ex.Message}");
                    }
                }

                // Save new file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update user profile picture URL
                user.ProfilePicture = $"/uploads/profiles/{fileName}";
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest(new { error = "Failed to update profile picture", errors = result.Errors });

                return Ok(new 
                { 
                    message = "Profile picture uploaded successfully", 
                    profilePicture = user.ProfilePicture,
                    fullUrl = $"{Request.Scheme}://{Request.Host}{user.ProfilePicture}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to upload file", details = ex.Message });
            }
        }

        /// <summary>
        /// Update profile (FirstName, LastName)
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            
            // Update DisplayName if provided
            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                // Check uniqueness if changed
                if (user.DisplayName != dto.Username)
                {
                    var existingUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.DisplayName == dto.Username && u.Id != userId);
                    
                    if (existingUser != null)
                        return BadRequest(new { error = "Username (Display Name) is already taken" });
                    
                    user.DisplayName = dto.Username;
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { error = "Failed to update profile", errors = result.Errors });

            return Ok(new
            {
                message = "Profile updated successfully",
                user.FirstName,
                user.LastName,
                user.DisplayName
            });
        }

        /// <summary>
        /// Search users by DisplayName or email (for task assignment)
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return BadRequest(new { error = "Query must be at least 2 characters" });

            var users = await _userManager.Users
                .Where(u => u.DisplayName!.Contains(query) || u.Email!.Contains(query))
                .Take(10)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.DisplayName,
                    u.FirstName,
                    u.LastName,
                    u.ProfilePicture
                })
                .ToListAsync();

            return Ok(users);
        }
    }

    // DTOs
    public class UpdateDisplayNameDto
    {
        public string DisplayName { get; set; } = null!;
    }

    public class UpdateEmailDto
    {
        public string Email { get; set; } = null!;
    }

    public class UpdatePasswordDto
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class UpdateProfilePictureDto
    {
        public string ProfilePictureUrl { get; set; } = null!;
    }

    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}

