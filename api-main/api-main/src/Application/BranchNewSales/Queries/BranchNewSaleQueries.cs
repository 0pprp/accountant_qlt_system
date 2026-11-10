using Application.BranchNewSales.Common;
using Application.BranchVisits.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.BranchNewSales.Queries;

public sealed class ListBranchNewSalesQuery : IRequest<Result<BranchNewSaleListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public bool UnreadOnly { get; init; }
}

public sealed class GetBranchNewSaleQuery : IRequest<Result<BranchNewSaleItemDto>>
{
    public int OrderId { get; init; }
}

public sealed class GetBranchNewSalesUnreadCountQuery : IRequest<Result<BranchNewSaleUnreadCountDto>>;

public sealed class ListBranchNewSalesHandler
    : IRequestHandler<ListBranchNewSalesQuery, Result<BranchNewSaleListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListBranchNewSalesHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<BranchNewSaleListResponse>> Handle(ListBranchNewSalesQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchNewSale.Read) == false)
            return BranchNewSaleErrors.Forbidden;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: null, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        var visitBranchId = visitResult.Data!.BranchId;
        var page = Math.Max(1, request.Page);
        // مصدر BM: TOP 200
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var baseQ = _db.Orders.AsNoTracking()
            .Where(o => o.BranchId == visitBranchId && o.WorkflowStatus == SalesWorkflowStatus.Sold);

        var unread = await baseQ.CountAsync(o => o.BranchManagerReadAtUtc == null, ct);
        var q = request.UnreadOnly ? baseQ.Where(o => o.BranchManagerReadAtUtc == null) : baseQ;
        var total = await q.CountAsync(ct);

        var rows = await q
            .OrderByDescending(o => o.SalesCompletedAtUtc ?? o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new BranchNewSaleItemDto(
                o.Id,
                o.BranchId,
                o.CustomerId,
                o.Customer!.FullName,
                o.Customer.PhoneNumber,
                o.SellerId,
                o.Seller != null ? o.Seller.FullName : null,
                o.SellAmount,
                o.DailyInstallmentAmount,
                o.PrepaymentAmount,
                o.SalesCompletedAtUtc,
                o.BranchManagerReadAtUtc,
                o.BranchManagerReadAtUtc != null,
                o.OrderItems.Select(i => new BranchNewSaleLineDto(
                    i.ProductId ?? 0,
                    i.ProductName ?? "",
                    i.Quantity,
                    i.SellAmount,
                    i.SellAmount * i.Quantity)).ToList()))
            .ToListAsync(ct);

        return new BranchNewSaleListResponse(rows, total, unread, page, pageSize);
    }
}

public sealed class GetBranchNewSaleHandler : IRequestHandler<GetBranchNewSaleQuery, Result<BranchNewSaleItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetBranchNewSaleHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<BranchNewSaleItemDto>> Handle(GetBranchNewSaleQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchNewSale.Read) == false)
            return BranchNewSaleErrors.Forbidden;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: null, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        var visitBranchId = visitResult.Data!.BranchId;

        var dto = await _db.Orders.AsNoTracking()
            .Where(x => x.Id == request.OrderId
                        && x.WorkflowStatus == SalesWorkflowStatus.Sold
                        && x.BranchId == visitBranchId)
            .Select(x => new BranchNewSaleItemDto(
                x.Id,
                x.BranchId,
                x.CustomerId,
                x.Customer!.FullName,
                x.Customer.PhoneNumber,
                x.SellerId,
                x.Seller != null ? x.Seller.FullName : null,
                x.SellAmount,
                x.DailyInstallmentAmount,
                x.PrepaymentAmount,
                x.SalesCompletedAtUtc,
                x.BranchManagerReadAtUtc,
                x.BranchManagerReadAtUtc != null,
                x.OrderItems.Select(i => new BranchNewSaleLineDto(
                    i.ProductId ?? 0,
                    i.ProductName ?? "",
                    i.Quantity,
                    i.SellAmount,
                    i.SellAmount * i.Quantity)).ToList()))
            .FirstOrDefaultAsync(ct);

        return dto is null ? BranchNewSaleErrors.NotFound : dto;
    }
}

public sealed class GetBranchNewSalesUnreadCountHandler
    : IRequestHandler<GetBranchNewSalesUnreadCountQuery, Result<BranchNewSaleUnreadCountDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetBranchNewSalesUnreadCountHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<BranchNewSaleUnreadCountDto>> Handle(
        GetBranchNewSalesUnreadCountQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchNewSale.Read) == false)
            return BranchNewSaleErrors.Forbidden;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: null, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        var visitBranchId = visitResult.Data!.BranchId;

        var count = await _db.Orders.AsNoTracking()
            .CountAsync(o =>
                o.BranchId == visitBranchId
                && o.WorkflowStatus == SalesWorkflowStatus.Sold
                && o.BranchManagerReadAtUtc == null, ct);
        return new BranchNewSaleUnreadCountDto(count);
    }
}
