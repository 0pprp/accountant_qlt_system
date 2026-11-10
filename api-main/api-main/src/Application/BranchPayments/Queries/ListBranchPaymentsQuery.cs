using Application.BranchVisits.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.BranchPayments.Queries;

/// <summary>
/// صف تسديد شاشة إدارة الفرع — مصدر payments_page: اسم، هاتف، تاريخ، مبلغ + مجاميع.
/// قراءة فقط؛ لا إنشاء تسديد.
/// </summary>
public sealed record BranchPaymentRowDto(
    int PaymentId,
    int CustomerId,
    string CustomerName,
    string? PhoneNumber,
    DateOnly Date,
    double Amount);

public sealed record BranchPaymentsListResponse(
    int ListId,
    string? ListName,
    DateOnly? FromDate,
    DateOnly? ToDate,
    double TotalAmount,
    int PaymentCount,
    IReadOnlyList<BranchPaymentRowDto> Items);

public static class BranchPaymentErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "BranchPayment.Forbidden");
    public static readonly Error BranchScope = new("خارج نطاق فرعك.", "BranchPayment.BranchScope");
    public static readonly Error ListNotFound = new("القائمة غير موجودة.", "BranchPayment.ListNotFound");
}

public sealed class ListBranchPaymentsQuery : IRequest<Result<BranchPaymentsListResponse>>
{
    public int ListId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}

public sealed class ListBranchPaymentsHandler
    : IRequestHandler<ListBranchPaymentsQuery, Result<BranchPaymentsListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListBranchPaymentsHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<BranchPaymentsListResponse>> Handle(
        ListBranchPaymentsQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.OrderList.Read) == false
            && _current.HasPermission(SeedPermissions.ManagementRatings.Read) == false
            && _current.HasPermission(SeedPermissions.InstallmentPayment.Read) == false)
            return BranchPaymentErrors.Forbidden;

        var list = await _db.OrderLists.AsNoTracking()
            .Where(l => l.Id == request.ListId)
            .Select(l => new { l.Id, l.Name, l.BranchId })
            .FirstOrDefaultAsync(ct);
        if (list is null)
            return BranchPaymentErrors.ListNotFound;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: list.BranchId, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        var q = _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.Order!.OrderListId == list.Id && p.Order.BranchId == list.BranchId);
        if (request.FromDate is DateOnly from)
            q = q.Where(p => p.Date >= from);
        if (request.ToDate is DateOnly to)
            q = q.Where(p => p.Date <= to);

        var items = await q
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Take(500)
            .Select(p => new BranchPaymentRowDto(
                p.Id,
                p.Order!.CustomerId,
                p.Order.Customer!.FullName,
                p.Order.Customer.PhoneNumber,
                p.Date,
                p.Amount))
            .ToListAsync(ct);

        var total = items.Sum(i => i.Amount);
        return new BranchPaymentsListResponse(
            list.Id, list.Name, request.FromDate, request.ToDate, total, items.Count, items);
    }
}
