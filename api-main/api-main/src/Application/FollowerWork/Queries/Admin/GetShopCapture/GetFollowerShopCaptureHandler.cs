using Application.SalesRequests.Common;
using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.ManagementSummary.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetShopCapture;

public sealed class GetFollowerShopCaptureHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<GetFollowerShopCaptureQuery, Result<GetFollowerShopCaptureResponse>>
{
    public async Task<Result<GetFollowerShopCaptureResponse>> Handle(
        GetFollowerShopCaptureQuery request,
        CancellationToken ct)
    {
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.LocationRead) == false)
            return FollowerWorkErrors.AccessDenied;

        if (request.BranchId <= 0 || FollowerWorkAccess.CanAccessBranch(currentUser, request.BranchId) == false)
            return FollowerWorkErrors.BranchAccessDenied;

        var listsQuery = db.OrderLists.AsNoTracking().Where(l => l.BranchId == request.BranchId);
        if (request.FollowerId is int followerId)
            listsQuery = listsQuery.Where(l => l.MotabaId == followerId);

        var lists = await listsQuery
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.MotabaId,
                FollowerName = l.Motaba != null ? l.Motaba.FullName : null
            })
            .ToListAsync(ct);

        if (request.FollowerId is not null && lists.Count == 0)
            return FollowerWorkErrors.FollowerBranchMismatch;

        var rows = await ShopCaptureProgressQuery.LoadAsync(db, lists.Select(l => l.Id).ToList(), ct);

        var listDtos = lists
            .Select(l =>
            {
                var r = rows.Where(x => x.ListId == l.Id).ToList();
                return new FollowerShopCaptureListDto(
                    l.Id,
                    l.Name,
                    l.MotabaId,
                    l.FollowerName,
                    r.Count,
                    r.Count(x => x.HasLocation),
                    r.Count(x => x.HasPhoto),
                    r.Count(x => x.IsComplete));
            })
            .Where(l => l.TotalCustomers > 0)
            .OrderBy(l => l.ListName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var customers = rows
            .OrderBy(x => x.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .Select(x => new FollowerShopCaptureCustomerDto(
                x.CustomerId,
                x.CustomerName,
                x.ShopName,
                x.PhoneNumber,
                x.ListId,
                x.AmountRemaining,
                x.HasLocation,
                x.Latitude,
                x.Longitude,
                x.HasPhoto,
                x.Photo?.PhotoId,
                x.Photo?.Source,
                x.Photo?.CapturedAtUtc))
            .ToList();

        return new GetFollowerShopCaptureResponse(
            rows.Count,
            rows.Count(x => x.HasLocation),
            rows.Count(x => x.HasPhoto),
            rows.Count(x => x.IsComplete),
            listDtos,
            customers,
            IraqCalendar.ToLocalDate(clock.UtcNowOffset).ToString("yyyy-MM-dd"));
    }
}
