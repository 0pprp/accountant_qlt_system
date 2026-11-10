using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.ComplaintAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.WrongSaleAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.DelegateHome.Queries;

public sealed record DelegateHomeSummaryDto(
    int? UnreadComplaints,
    bool UnreadComplaintsAvailable,
    int? PendingExceptions,
    bool PendingExceptionsAvailable,
    int? UnreadWrongSales,
    bool UnreadWrongSalesAvailable,
    int? ApprovedExceptions,
    int? RejectedExceptions,
    int? CancelledExceptions,
    int? TotalExceptions,
    bool ExceptionsSummaryAvailable);

public sealed class GetDelegateHomeSummaryQuery : IRequest<Result<DelegateHomeSummaryDto>>;

public sealed class GetDelegateHomeSummaryHandler
    : IRequestHandler<GetDelegateHomeSummaryQuery, Result<DelegateHomeSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetDelegateHomeSummaryHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<DelegateHomeSummaryDto>> Handle(
        GetDelegateHomeSummaryQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.DelegateHome.Read) == false)
            return new Error("لا صلاحية لرئيسية المفوض.", "DelegateHome.Forbidden");

        var scope = _current.BranchIds ?? [];
        // الفشل ليس صفرًا — عند غياب النطاق نعيد null مع Available=false
        if (scope.Count == 0)
        {
            return new DelegateHomeSummaryDto(
                null, false, null, false, null, false,
                null, null, null, null, false);
        }

        int? unreadComplaints = null;
        var complaintsOk = _current.HasPermission(SeedPermissions.Complaint.Read);
        if (complaintsOk)
        {
            unreadComplaints = await _db.ComplaintMessages.AsNoTracking()
                .CountAsync(c =>
                    c.Status == ComplaintStatus.Unread
                    && (c.BranchId == null || scope.Contains(c.BranchId.Value)), ct);
        }

        int? unreadWrong = null;
        var wrongOk = _current.HasPermission(SeedPermissions.WrongSale.Read);
        if (wrongOk)
        {
            unreadWrong = await _db.WrongSaleNotices.AsNoTracking()
                .CountAsync(n => scope.Contains(n.BranchId) && n.DelegateReadAtUtc == null, ct);
        }

        int? pending = null, approved = null, rejected = null, cancelled = null, total = null;
        var exOk = _current.HasPermission(SeedPermissions.SalesRequest.TabExceptions)
                   || _current.HasPermission(SeedPermissions.SalesRequest.ExceptionDecide);
        if (exOk)
        {
            var q = _db.SalesRequests.AsNoTracking()
                .Where(r => r.IsDeleted == false && scope.Contains(r.BranchId)
                            && r.ExceptionHoldStatus != null);
            pending = await q.CountAsync(r => r.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Pending, ct);
            approved = await q.CountAsync(r => r.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Approved, ct);
            rejected = await q.CountAsync(r => r.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Rejected, ct);
            // المصدر يعرض «ملغاة»؛ العقد الحالي للاستثناء لا يحتفظ بحالة إلغاء منفصلة
            cancelled = 0;
            total = await q.CountAsync(ct);
        }

        return new DelegateHomeSummaryDto(
            unreadComplaints, complaintsOk,
            pending, exOk,
            unreadWrong, wrongOk,
            approved, rejected, cancelled, total, exOk);
    }
}
