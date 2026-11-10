using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.WrongSales.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.WrongSales.Queries;

public sealed class ListWrongSalesQuery : IRequest<Result<WrongSaleListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
    public int? BranchId { get; init; }
    /// <summary>unread | read | unreviewed | reviewed | all</summary>
    public string? Mailbox { get; init; }
}

public sealed class ListWrongSalesHandler : IRequestHandler<ListWrongSalesQuery, Result<WrongSaleListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListWrongSalesHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<WrongSaleListResponse>> Handle(ListWrongSalesQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.WrongSale.Read) == false)
            return WrongSaleErrors.Forbidden;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var branchScope = _current.BranchIds ?? [];
        if (branchScope.Count == 0)
            return new WrongSaleListResponse([], 0, 0, 0, page, pageSize);

        var q = _db.WrongSaleNotices.AsNoTracking()
            .Where(x => branchScope.Contains(x.BranchId));

        if (request.BranchId is int bid)
        {
            if (branchScope.Contains(bid) == false)
                return WrongSaleErrors.BranchScope;
            q = q.Where(x => x.BranchId == bid);
        }

        var mailbox = (request.Mailbox ?? "all").Trim().ToLowerInvariant();
        q = mailbox switch
        {
            "unread" => q.Where(x => x.DelegateReadAtUtc == null),
            "read" => q.Where(x => x.DelegateReadAtUtc != null),
            "unreviewed" => q.Where(x => x.BranchManagerReviewedAtUtc == null),
            "reviewed" => q.Where(x => x.BranchManagerReviewedAtUtc != null),
            _ => q
        };

        var baseForCounts = _db.WrongSaleNotices.AsNoTracking()
            .Where(x => branchScope.Contains(x.BranchId));
        if (request.BranchId is int b2)
            baseForCounts = baseForCounts.Where(x => x.BranchId == b2);

        var unread = await baseForCounts.CountAsync(x => x.DelegateReadAtUtc == null, ct);
        var unreviewed = await baseForCounts.CountAsync(x => x.BranchManagerReviewedAtUtc == null, ct);
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new WrongSaleListResponse(
            items.Select(WrongSaleMapper.ToDto).ToList(),
            total, unread, unreviewed, page, pageSize);
    }
}
