using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Cloudinary-backed image storage. Implements <see cref="IImageStorage"/> so it
/// can be swapped in for <see cref="LocalImageStorage"/> via the
/// <c>ImageStorage:Provider</c> configuration key ("Local" or "Cloudinary").
/// </summary>
public class CloudinaryImageStorage : IImageStorage
{
    private readonly Account _account;
    private readonly string? _folderPrefix;
    private readonly ILogger<CloudinaryImageStorage> _logger;
    private Cloudinary? _cloudinary;

    public CloudinaryImageStorage(IConfiguration configuration, ILogger<CloudinaryImageStorage> logger)
    {
        _logger = logger;
        var section = configuration.GetSection("ImageStorage:Cloudinary");
        _account = new Account(
            section["CloudName"] ?? string.Empty,
            section["ApiKey"] ?? string.Empty,
            section["ApiSecret"] ?? string.Empty);
        _folderPrefix = section["Folder"];
    }

    private Cloudinary Client
    {
        get
        {
            if (_cloudinary is not null)
                return _cloudinary;

            if (string.IsNullOrWhiteSpace(_account.Cloud)
                || string.IsNullOrWhiteSpace(_account.ApiKey)
                || string.IsNullOrWhiteSpace(_account.ApiSecret))
                throw new InvalidOperationException(
                    "Cloudinary is not configured. Set ImageStorage:Cloudinary:CloudName/ApiKey/ApiSecret.");

            _cloudinary = new Cloudinary(_account);
            return _cloudinary;
        }
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, string folder)
    {
        var targetFolder = string.IsNullOrWhiteSpace(_folderPrefix)
            ? folder.Trim('/')
            : $"{_folderPrefix.Trim('/')}/{folder.Trim('/')}";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = targetFolder,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await Client.UploadAsync(uploadParams);
        if (result.Error is not null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new BadRequestException($"Image upload failed: {result.Error.Message}");
        }

        var url = result.SecureUrl?.ToString() ?? result.Url?.ToString();
        if (string.IsNullOrWhiteSpace(url))
            throw new BadRequestException("Image upload failed: no URL was returned.");

        _logger.LogInformation("Uploaded image {PublicId} to Cloudinary", result.PublicId);
        return url;
    }

    public async Task DeleteAsync(string url)
    {
        try
        {
            var publicId = ExtractPublicId(url);
            if (publicId is null)
                return; // not a Cloudinary URL — nothing to do

            var result = await Client.DestroyAsync(new DeletionParams(publicId));
            if (result.Error is not null)
                _logger.LogWarning("Cloudinary delete failed for {PublicId}: {Error}", publicId, result.Error.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete image {Url}", url);
        }
    }

    /// <summary>
    /// Extracts the public_id from a Cloudinary delivery URL of the form
    /// https://res.cloudinary.com/{cloud}/image/upload/[v12345/]{folder/...}/{publicId}.{ext}.
    /// Returns null when the URL is not a Cloudinary URL.
    /// </summary>
    private static string? ExtractPublicId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;
        if (!url.Contains("res.cloudinary.com", StringComparison.OrdinalIgnoreCase))
            return null;

        const string marker = "/upload/";
        var idx = url.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return null;

        var segments = url[(idx + marker.Length)..].Split('/', StringSplitOptions.RemoveEmptyEntries);

        // strip the optional version segment ("v1234567890/")
        if (segments.Length > 0 && segments[0].Length > 1 && segments[0][0] == 'v' && segments[0][1..].All(char.IsDigit))
            segments = segments[1..];
        if (segments.Length == 0)
            return null;

        // strip the file extension from the last segment
        var last = segments[^1];
        var dot = last.LastIndexOf('.');
        segments[^1] = dot > 0 ? last[..dot] : last;

        return string.Join('/', segments);
    }
}
