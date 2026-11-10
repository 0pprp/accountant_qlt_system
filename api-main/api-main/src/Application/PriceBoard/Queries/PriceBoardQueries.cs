using Application.Common.Interfaces;
using Application.PriceBoard.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.PriceBoard.Queries;

public sealed class ListPriceBoardQuery : IRequest<Result<PriceBoardListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public int? ProvinceId { get; init; }
    public string? Search { get; init; }
    /// <summary>عند true يقيّد بمحافظ فروع المستخدم (محاسب/مندوب).</summary>
    public bool ScopeToUserBranches { get; init; }
}

public sealed class ListPriceBoardHandler : IRequestHandler<ListPriceBoardQuery, Result<PriceBoardListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListPriceBoardHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<PriceBoardListResponse>> Handle(ListPriceBoardQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.PriceBoard.Read) == false)
            return PriceBoardErrors.Forbidden;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var q = _db.PriceBoardEntries.AsNoTracking().AsQueryable();

        if (request.ScopeToUserBranches)
        {
            var branchIds = _current.BranchIds ?? [];
            if (branchIds.Count == 0)
                return new PriceBoardListResponse([], 0, page, pageSize);
            var provinceIds = await _db.Branches.AsNoTracking()
                .Where(b => branchIds.Contains(b.Id))
                .Select(b => b.ProvinceId)
                .Distinct()
                .ToListAsync(ct);
            q = q.Where(x => provinceIds.Contains(x.TargetProvinceId));
        }
        else if (request.ProvinceId is int pid)
        {
            q = q.Where(x => x.TargetProvinceId == pid);
        }

        if (string.IsNullOrWhiteSpace(request.Search) == false)
        {
            var s = request.Search.Trim();
            q = q.Where(x => x.ProductName.Contains(s) || x.TargetProvinceName.Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PriceBoardListResponse(items.Select(PriceBoardMapper.ToDto).ToList(), total, page, pageSize);
    }
}

public sealed class GetPriceBoardByIdQuery : IRequest<Result<PriceBoardEntryDto>>
{
    public int Id { get; init; }
}

public sealed class GetPriceBoardByIdHandler : IRequestHandler<GetPriceBoardByIdQuery, Result<PriceBoardEntryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetPriceBoardByIdHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<PriceBoardEntryDto>> Handle(GetPriceBoardByIdQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.PriceBoard.Read) == false)
            return PriceBoardErrors.Forbidden;
        var e = await _db.PriceBoardEntries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        return e is null ? PriceBoardErrors.NotFound : PriceBoardMapper.ToDto(e);
    }
}
