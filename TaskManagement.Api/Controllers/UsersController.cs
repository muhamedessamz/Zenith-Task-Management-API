using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.DTOs.User;
using TaskManagement.Core.Entities;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/Users - Get all users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetAllUsers()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.DisplayName ?? u.Email)
                .Take(100) // Limit to 100 users for performance
                .ToListAsync();

            var dtos = users.Select(u => new UserSummaryDto
            {
                Id = u.Id,
                Email = u.Email!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DisplayName = u.DisplayName ?? $"{u.FirstName} {u.LastName}",
                ProfilePicture = u.ProfilePicture
            });

            return Ok(dtos);
        }


        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return BadRequest("Search query must be at least 2 characters long.");
            }

            query = query.ToLower();

            // Search by Email, FirstName, LastName, or DisplayName
            // Using StartsWith for more precise results
            var users = await _userManager.Users
                .Where(u => u.Email!.ToLower().StartsWith(query) || 
                            u.FirstName.ToLower().StartsWith(query) || 
                            u.LastName.ToLower().StartsWith(query) ||
                            (u.DisplayName != null && u.DisplayName.ToLower().StartsWith(query)))
                .Take(20) // Limit results
                .ToListAsync();

            var dtos = users.Select(u => new UserSummaryDto
            {
                Id = u.Id,
                Email = u.Email!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DisplayName = u.DisplayName ?? $"{u.FirstName} {u.LastName}",
                ProfilePicture = u.ProfilePicture
            });

            return Ok(dtos);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserSummaryDto>> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            return Ok(new UserSummaryDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName ?? $"{user.FirstName} {user.LastName}",
                ProfilePicture = user.ProfilePicture
            });
        }
    }
}
