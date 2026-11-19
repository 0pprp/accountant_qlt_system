using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/CustomerMedia")]
public class CustomerMediaController : ApiController
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public CustomerMediaController(IApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("photos/{id:int}")]
    public async Task<IActionResult> Photo([FromRoute] int id)
    {
        var photo = await _dbContext.CustomerShopPhotos.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Content, x.ContentType })
            .FirstOrDefaultAsync(CancellationToken);

        if (photo is null)
            return NotFound();

        return File(photo.Content, string.IsNullOrWhiteSpace(photo.ContentType)
            ? "image/jpeg"
            : photo.ContentType);
    }

    /// <summary>النظام القديم خزّن application/octet-stream لجزء من المستندات، فالامتداد أصدق منه.</summary>
    private static string ResolveContentType(string? stored, string path)
    {
        if (!string.IsNullOrWhiteSpace(stored)
            && !string.Equals(stored, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return stored;
        }

        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("documents/{id:int}")]
    public async Task<IActionResult> Document([FromRoute] int id)
    {
        var doc = await _dbContext.CustomerDocuments.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.FileKey, x.FileName, x.ContentType })
            .FirstOrDefaultAsync(CancellationToken);

        if (doc is null)
            return NotFound();

        var root = _configuration["NajafMedia:Root"];
        if (string.IsNullOrWhiteSpace(root))
            return NotFound();

        var relative = doc.FileKey.Replace('\\', '/').TrimStart('/');
        var marker = "App_Data/";
        var index = relative.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
            relative = relative[(index + marker.Length)..];

        var full = Path.GetFullPath(Path.Combine(root, "App_Data", relative));
        var safeRoot = Path.GetFullPath(root);
        if (!full.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(full))
            return NotFound();

        return PhysicalFile(full, ResolveContentType(doc.ContentType, full), doc.FileName);
    }
}
