using ECommerce.Application.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Local-disk image storage. Implements <see cref="IImageStorage"/> so a cloud
/// provider (Cloudinary / S3) can replace it later without touching callers.
/// </summary>
public class LocalImageStorage : IImageStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalImageStorage> _logger;

    public LocalImageStorage(IWebHostEnvironment env, ILogger<LocalImageStorage> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, string folder)
    {
        var uploadsRoot = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", folder);
        Directory.CreateDirectory(uploadsRoot);

        var ext = Path.GetExtension(fileName);
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsRoot, safeName);

        await using var file = File.Create(fullPath);
        await stream.CopyToAsync(file);

        _logger.LogInformation("Saved image {Path}", fullPath);
        return $"/uploads/{folder}/{safeName}";
    }

    public Task DeleteAsync(string url)
    {
        try
        {
            var relative = url.TrimStart('/');
            var full = Path.Combine(_env.ContentRootPath, relative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(full))
                File.Delete(full);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete image {Url}", url);
        }
        return Task.CompletedTask;
    }
}