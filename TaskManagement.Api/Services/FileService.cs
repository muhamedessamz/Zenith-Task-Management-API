using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TaskManagement.Api.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file);
        void DeleteFile(string relativePath);
        long GetFileSize(string relativePath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            // Ensure wwwroot exists
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                _environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "attachments");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Path.Combine("uploads", "attachments", uniqueFileName).Replace("\\", "/");
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            
            // Handle different path separators
            var normalizedPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            normalizedPath = normalizedPath.TrimStart(Path.DirectorySeparatorChar);

            var absolutePath = Path.Combine(_environment.WebRootPath, normalizedPath);
            
            try 
            {
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }
            catch (Exception ex)
            {
                // Log error ideally
                Console.WriteLine($"Error deleting file {absolutePath}: {ex.Message}");
            }
        }

        public long GetFileSize(string relativePath)
        {
             if (string.IsNullOrEmpty(relativePath)) return 0;
            
            var normalizedPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            normalizedPath = normalizedPath.TrimStart(Path.DirectorySeparatorChar);
            var absolutePath = Path.Combine(_environment.WebRootPath, normalizedPath);

            return File.Exists(absolutePath) ? new FileInfo(absolutePath).Length : 0;
        }
    }
}
