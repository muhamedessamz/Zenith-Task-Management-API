using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Services
{
    public class FileCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FileCleanupService> _logger;

        public FileCleanupService(IServiceScopeFactory scopeFactory, ILogger<FileCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FileCleanupService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupFilesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during file cleanup.");
                }

                // Run every 15 minutes
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private async Task CleanupFilesAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

                var now = DateTime.UtcNow;

                // 1. Standalone Task Completed > 30 mins
                // (Delete file after 30m of task completion)
                var completedStandaloneThreshold = now.AddMinutes(-30);
                var rule1Files = await context.TaskAttachments
                    .Include(a => a.Task)
                    .Where(a => a.Task.ProjectId == null &&
                                a.Task.IsCompleted &&
                                a.Task.CompletedAt != null &&
                                a.Task.CompletedAt < completedStandaloneThreshold)
                    .ToListAsync();

                // 2. Project Task (Project Completed) > 1 hour
                // (Delete file after 1h of project completion)
                var completedProjectThreshold = now.AddHours(-1);
                var rule2Files = await context.TaskAttachments
                    .Include(a => a.Task)
                    .ThenInclude(t => t.Project)
                    .Where(a => a.Task.ProjectId != null &&
                                a.Task.Project!.IsCompleted &&
                                a.Task.Project.CompletedAt != null &&
                                a.Task.Project.CompletedAt < completedProjectThreshold)
                    .ToListAsync();

                // 3. Stale Standalone Task > 7 days
                // (Delete file after 7 days if task is not finished)
                var staleStandaloneThreshold = now.AddDays(-7);
                var rule3Files = await context.TaskAttachments
                    .Include(a => a.Task)
                    .Where(a => a.Task.ProjectId == null &&
                                !a.Task.IsCompleted &&
                                a.UploadedAt < staleStandaloneThreshold)
                    .ToListAsync();

                // 4. Stale Project Task > 30 days
                // (Delete file after 30 days if project is not finished)
                var staleProjectThreshold = now.AddDays(-30);
                var rule4Files = await context.TaskAttachments
                    .Include(a => a.Task)
                    .ThenInclude(t => t.Project)
                    .Where(a => a.Task.ProjectId != null &&
                                !a.Task.Project!.IsCompleted &&
                                a.UploadedAt < staleProjectThreshold)
                    .ToListAsync();

                // Combine all files to delete
                var allFilesToDelete = rule1Files
                    .Concat(rule2Files)
                    .Concat(rule3Files)
                    .Concat(rule4Files)
                    .DistinctBy(a => a.Id)
                    .ToList();

                if (allFilesToDelete.Any())
                {
                    _logger.LogInformation($"Found {allFilesToDelete.Count} files to cleanup.");

                    foreach (var file in allFilesToDelete)
                    {
                        // Delete from disk
                        fileService.DeleteFile(file.FilePath);

                        // Remove from DB
                        context.TaskAttachments.Remove(file);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Cleanup completed successfully. Deleted {allFilesToDelete.Count} attachments.");
                }
            }
        }
    }
}
