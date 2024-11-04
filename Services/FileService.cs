using Microsoft.AspNetCore.Components.Forms;

namespace LMS.Web.Services;

public interface IFileService
{
    Task<string> UploadFileAsync(IBrowserFile file, string folder);
    Task DeleteFileAsync(string fileUrl);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IBrowserFile file, string folder)
    {
        try
        {
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using var stream = file.OpenReadStream(maxAllowedSize: 10485760); // 10MB max
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await stream.CopyToAsync(fileStream);

            return $"/uploads/{folder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.Name);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileUrl}", fileUrl);
            throw;
        }
    }
} 