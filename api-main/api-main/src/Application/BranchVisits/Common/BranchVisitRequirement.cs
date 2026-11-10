using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.BranchVisitAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.BranchVisits.Common;

/// <summary>
/// شرط الزيارة النشطة لأعمال إدارة الفرع (BM).
/// مستقل عن صلاحية BranchVisit.Read — نقص القراءة لا يمنح تجاوزًا.
///
/// مستثنى من هذا الشرط (عقد العمل):
/// - قراءات oversight ولوحات الإدارة العامة.
/// - تقارير المفوض وتاريخ الزيارات (ListBranchVisits / history mode).
/// - أوامر فتح/تقييم/مغادرة الزيارة نفسها.
/// </summary>
public static class BranchVisitRequirement
{
    public static readonly Error VisitRequired = new(
        "افتح زيارة نشطة للفرع قبل هذا العمل.",
        "BranchVisit.VisitRequired");

    public static readonly Error VisitBranchMismatch = new(
        "الزيارة المفتوحة ليست لفرع العمل المحدد.",
        "BranchVisit.VisitBranchMismatch");

    /// <summary>
    /// يشترط زيارة Open للفاعل الحالي. إن مُمرِّر <paramref name="requiredBranchId"/>
    /// يجب أن تطابق الزيارة ذلك الفرع (زيارة A لا تفتح أعمال B).
    /// </summary>
    public static async Task<Result<BranchVisit>> RequireOpenAsync(
        IApplicationDbContext db,
        ICurrentUserService current,
        int? requiredBranchId,
        CancellationToken ct)
    {
        if (current.UserId is not int actorId)
            return BranchVisitErrors.Forbidden;

        var visit = await db.BranchVisits
            .Where(v => v.OpenedByUserId == actorId && v.Status == BranchVisitStatus.Open)
            .OrderByDescending(v => v.ArrivedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (visit is null)
            return VisitRequired;

        var scope = current.BranchIds ?? [];
        if (scope.Count == 0 || scope.Contains(visit.BranchId) == false)
            return BranchVisitErrors.BranchScope;

        if (requiredBranchId is int bid && visit.BranchId != bid)
            return VisitBranchMismatch;

        return visit;
    }
}
