using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;
using Google.Apis.Util;

namespace TaskManagement.Api.Services
{
    public class GoogleCalendarService : ICalendarService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<GoogleCalendarService> _logger;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public GoogleCalendarService(AppDbContext context, IConfiguration config, ILogger<GoogleCalendarService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
            // Fallback for nulls to avoid instant crash if configured poorly
            _clientId = config["GoogleCalendar:ClientId"] ?? "";
            _clientSecret = config["GoogleCalendar:ClientSecret"] ?? "";
            _redirectUri = config["GoogleCalendar:RedirectUri"] ?? "";
        }

        private GoogleAuthorizationCodeFlow GetFlow()
        {
            return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret },
                Scopes = new[] { CalendarService.Scope.CalendarEvents },
                DataStore = null // We manage storage manually in DB
            });
        }

        public string GetAuthUri(string userId)
        {
            var flow = GetFlow();
            // We pass userId in "state" to know who is completing the auth flow
            // Explicit cast to GoogleAuthorizationCodeRequestUrl to access AccessType and Prompt
            var request = (GoogleAuthorizationCodeRequestUrl)flow.CreateAuthorizationCodeRequest(_redirectUri);
            
            request.State = userId;
            request.AccessType = "offline"; // Critical to get Refresh Token
            request.Prompt = "consent"; // Force consent to ensure refresh token is returned
            
            return request.Build().AbsoluteUri;
        }

        public async Task<bool> ExchangeCodeForTokenAsync(string userId, string code)
        {
            try
            {
                var flow = GetFlow();
                var token = await flow.ExchangeCodeForTokenAsync(userId, code, _redirectUri, CancellationToken.None);

                if (token == null) return false;

                // Save to DB
                var integration = await _context.UserCalendarIntegrations
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.Provider == "Google");

                if (integration == null)
                {
                    integration = new UserCalendarIntegration 
                    { 
                        UserId = userId, 
                        Provider = "Google",
                        ConnectedEmail = "Linked Account" // Ideally fetch user profile to get email
                    };
                    _context.UserCalendarIntegrations.Add(integration);
                }

                integration.AccessToken = token.AccessToken;
                // Important: Refresh token is only sent on first consent. If null, keep existing.
                if (!string.IsNullOrEmpty(token.RefreshToken)) 
                    integration.RefreshToken = token.RefreshToken;
                
                integration.TokenExpiry = DateTime.UtcNow.AddSeconds(token.ExpiresInSeconds ?? 3599);
                integration.LastSyncedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to exchange Google token");
                return false;
            }
        }

        private async Task<CalendarService?> GetServiceAsync(string userId)
        {
             var integration = await _context.UserCalendarIntegrations
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Provider == "Google");
            
            if (integration == null) return null;

            var tokenResponse = new TokenResponse 
            { 
                AccessToken = integration.AccessToken, 
                RefreshToken = integration.RefreshToken,
                ExpiresInSeconds = (long)(integration.TokenExpiry - DateTime.UtcNow).TotalSeconds,
                IssuedUtc = DateTime.UtcNow.AddSeconds(-(3600 - (integration.TokenExpiry - DateTime.UtcNow).TotalSeconds)) // Approx
            };

            var flow = GetFlow();
            var credential = new UserCredential(flow, userId, tokenResponse);
            
            // Check if expired and refresh
             if (credential.Token.IsStale)
            {
                if(await credential.RefreshTokenAsync(CancellationToken.None))
                {
                    integration.AccessToken = credential.Token.AccessToken;
                    integration.TokenExpiry = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3599);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning($"Could not refresh token for user {userId}");
                    return null; // Auth lost
                }
            }

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Task Management System"
            });
        }

        public async Task SyncTaskToCalendarAsync(int taskId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            // Only sync if DueDate exists
            if (task == null || task.DueDate == null) return;

            var service = await GetServiceAsync(task.UserId);
            // If user not connected or token invalid, just return (don't break task creation)
            if (service == null) return; 

            var ev = new Event
            {
                Summary = $"📌 {task.Title}",
                Description = $"{task.Description}\n\nPriority: {task.Priority}\nLink: http://localhost:5173/tasks/{task.Id}",
                Start = new EventDateTime { DateTimeRaw = task.DueDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz") },
                // Default duration 1 hour for now
                End = new EventDateTime { DateTimeRaw = task.DueDate.Value.AddHours(1).ToString("yyyy-MM-ddTHH:mm:sszzz") }
            };

            try 
            {
                if (!string.IsNullOrEmpty(task.ExternalCalendarEventId))
                {
                    // Update
                    await service.Events.Update(ev, "primary", task.ExternalCalendarEventId).ExecuteAsync();
                }
                else
                {
                    // Create
                    var newEvent = await service.Events.Insert(ev, "primary").ExecuteAsync();
                    task.ExternalCalendarEventId = newEvent.Id;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to sync task {taskId} to calendar");
                // If 404/410 (Gone), clear ID and recreate next time?
                if (ex.Message.Contains("404") || ex.Message.Contains("410"))
                {
                    task.ExternalCalendarEventId = null;
                    await _context.SaveChangesAsync();
                }
            }
        }
        
         public async Task DeleteEventAsync(int taskId)
        {
             var task = await _context.Tasks.FindAsync(taskId);
             if (task == null || string.IsNullOrEmpty(task.ExternalCalendarEventId)) return;

             var service = await GetServiceAsync(task.UserId);
             if (service == null) return;

             try {
                await service.Events.Delete("primary", task.ExternalCalendarEventId).ExecuteAsync();
             } catch {}
             
             task.ExternalCalendarEventId = null;
             await _context.SaveChangesAsync();
        }

        public async Task<(bool IsConnected, DateTime? ConnectedAt)> GetConnectionStatusAsync(string userId)
        {
            var integration = await _context.UserCalendarIntegrations
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Provider == "Google");
            
            if (integration == null)
            {
                return (false, null);
            }
            
            return (true, integration.CreatedAt);
        }

        public async Task<bool> DisconnectAsync(string userId)
        {
            var integration = await _context.UserCalendarIntegrations
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Provider == "Google");
            
            if (integration != null)
            {
                _context.UserCalendarIntegrations.Remove(integration);
                await _context.SaveChangesAsync();
                return true;
            }
            
            return false;
        }
    }
}
