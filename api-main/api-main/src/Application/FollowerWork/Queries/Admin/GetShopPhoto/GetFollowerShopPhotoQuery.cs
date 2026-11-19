using Application.SalesRequests.Common;
using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Domain.Entities.AttachmentAggregate.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetShopPhoto;

public sealed record GetFollowerShopPhotoQuery(int PhotoId, int BranchId, string? Source = null) : IRequest<Result<FollowerShopPhotoFile>>;

public sealed record FollowerShopPhotoFile(byte[] Content, string ContentType);

public sealed class GetFollowerShopPhotoHandler(IApplicationDbContext db, ICurrentUserService currentUser, IWebHostEnvironment env)
    : IRequestHandler<GetFollowerShopPhotoQuery, Result<FollowerShopPhotoFile>>
{
    public async Task<Result<FollowerShopPhotoFile>> Handle(GetFollowerShopPhotoQuery request, CancellationToken ct)
    {
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.LocationRead) == false)
            return FollowerWorkErrors.AccessDenied;

        if (request.BranchId <= 0 || FollowerWorkAccess.CanAccessBranch(currentUser, request.BranchId) == false)
            return FollowerWorkErrors.BranchAccessDenied;

        var source = string.IsNullOrWhiteSpace(request.Source) ? FollowerShopPhotoSources.Imported : request.Source.Trim();
        if (FollowerShopPhotoSources.IsKnown(source) == false)
            return FollowerWorkErrors.CustomerOutOfScope;

        return source == FollowerShopPhotoSources.App
            ? await LoadAppPhotoAsync(request, ct)
            : await LoadImportedPhotoAsync(request, ct);
    }

    private async Task<Result<FollowerShopPhotoFile>> LoadImportedPhotoAsync(GetFollowerShopPhotoQuery request, CancellationToken ct)
    {
        var photo = await db.CustomerShopPhotos.AsNoTracking()
            .Where(p => p.Id == request.PhotoId)
            .Select(p => new { p.CustomerId, p.Content, p.ContentType })
            .FirstOrDefaultAsync(ct);
        if (photo is null || await CustomerInBranchAsync(photo.CustomerId, request.BranchId, ct) == false)
            return FollowerWorkErrors.CustomerOutOfScope;

        return new FollowerShopPhotoFile(photo.Content, string.IsNullOrWhiteSpace(photo.ContentType) ? "image/jpeg" : photo.ContentType);
    }

    private async Task<Result<FollowerShopPhotoFile>> LoadAppPhotoAsync(GetFollowerShopPhotoQuery request, CancellationToken ct)
    {
        var attachment = await db.Attachments.AsNoTracking()
            .Where(a => a.Id == request.PhotoId && a.Type == AttachmentType.CustomerShopPhoto && a.CustomerId != null)
            .Select(a => new { CustomerId = a.CustomerId!.Value, a.RelativePath, a.ContentType })
            .FirstOrDefaultAsync(ct);
        if (attachment is null || await CustomerInBranchAsync(attachment.CustomerId, request.BranchId, ct) == false)
            return FollowerWorkErrors.CustomerOutOfScope;

        if (string.IsNullOrWhiteSpace(env.WebRootPath))
            return FollowerWorkErrors.CustomerOutOfScope;

        var root = Path.GetFullPath(env.WebRootPath);
        var full = Path.GetFullPath(Path.Combine(root, attachment.RelativePath.TrimStart('/', '\\')));
        if (full.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) == false
            || File.Exists(full) == false)
            return FollowerWorkErrors.CustomerOutOfScope;

        var bytes = await File.ReadAllBytesAsync(full, ct);
        return new FollowerShopPhotoFile(bytes, string.IsNullOrWhiteSpace(attachment.ContentType) ? "image/jpeg" : attachment.ContentType);
    }

    private Task<bool> CustomerInBranchAsync(int customerId, int branchId, CancellationToken ct)
        => db.Orders.AsNoTracking()
            .AnyAsync(o => o.CustomerId == customerId
                           && o.OrderList != null
                           && o.OrderList.BranchId == branchId, ct);
}
