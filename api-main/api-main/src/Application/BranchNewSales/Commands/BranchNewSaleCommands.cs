using Application.BranchNewSales.Common;
using Application.BranchVisits.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.BranchNewSales.Commands;

public sealed class MarkBranchNewSaleReadCommand : IRequest<Result>
{
    public int OrderId { get; init; }
}

public sealed class MarkAllBranchNewSalesReadCommand : IRequest<Result<int>>
{
}

public sealed class MarkBranchNewSaleReadHandler : IRequestHandler<MarkBranchNewSaleReadCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public MarkBranchNewSaleReadHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result> Handle(MarkBranchNewSaleReadCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchNewSale.MarkRead) == false)
            return BranchNewSaleErrors.Forbidden;

        var order = await _db.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId && o.WorkflowStatus == SalesWorkflowStatus.Sold, ct);
        if (order is null)
            return BranchNewSaleErrors.NotFound;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: order.BranchId, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        // idempotent: already read = success without changing Approval/Workflow
        order.MarkBranchManagerRead(_clock.UtcNowOffset);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed class MarkAllBranchNewSalesReadHandler : IRequestHandler<MarkAllBranchNewSalesReadCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public MarkAllBranchNewSalesReadHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(MarkAllBranchNewSalesReadCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchNewSale.MarkRead) == false)
            return BranchNewSaleErrors.Forbidden;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: null, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        var visitBranchId = visitResult.Data!.BranchId;
        var at = _clock.UtcNowOffset;
        var rows = await _db.Orders
            .Where(o =>
                o.BranchId == visitBranchId
                && o.WorkflowStatus == SalesWorkflowStatus.Sold
                && o.BranchManagerReadAtUtc == null)
            .ToListAsync(ct);

        var n = 0;
        foreach (var o in rows)
        {
            if (o.MarkBranchManagerRead(at))
                n++;
        }

        await _db.SaveChangesAsync(ct);
        return n;
    }
}
