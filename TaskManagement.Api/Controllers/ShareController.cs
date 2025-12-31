using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/share")]
    public class ShareController : ControllerBase
    {
        private readonly ISharedLinkService _service;

        public ShareController(ISharedLinkService service)
        {
            _service = service;
        }

        [HttpPost("generate")]
        [Authorize]
        public async Task<IActionResult> GenerateLink([FromBody] GenerateLinkRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                var link = await _service.GenerateLinkAsync(userId, request.EntityType, request.EntityId, request.ExpiresAt);
                
                // Construct the public URL
                // If you have a frontend, this should point to the frontend route, e.g., https://myapp.com/shared/task/{token}
                // For API testing, we point to the API endpoint
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var publicUrl = $"{baseUrl}/api/share/view/{link.Token}";
                
                return Ok(new { 
                    Token = link.Token, 
                    PublicUrl = publicUrl, 
                    Expiry = link.ExpiresAt 
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("view/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> ViewContent(string token)
        {
            var payload = await _service.GetPayloadAsync(token);
            if (payload == null) 
                return NotFound(new { error = "This link is invalid, expired, or has been revoked." });
            
            return Ok(payload);
        }

        [HttpDelete("revoke/{token}")]
        [Authorize]
        public async Task<IActionResult> RevokeLink(string token)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                var success = await _service.RevokeLinkAsync(token, userId);
                
                if (!success) return NotFound();
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }

    public class GenerateLinkRequest
    {
        public string EntityType { get; set; } = "Task"; // Default
        public int EntityId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
