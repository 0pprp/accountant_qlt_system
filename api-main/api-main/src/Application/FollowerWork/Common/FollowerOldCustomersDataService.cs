using Application.Common.Interfaces;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public class FollowerOldCustomersDataService
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly FollowerSalesReviewDataService _scope;

    public FollowerOldCustomersDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        FollowerSalesReviewDataService scope)
    {
        _db = db;
        _clock = clock;
        _scope = scope;
    }

    public async Task<Result<FollowerOldCustomersResponse>> GetForAdminAsync(
        int followerId,
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        // التحقق من أن المتابع ضمن نطاق الفرع المسموح عبر تكليف قائمة
        var scope = await _scope.ResolveFollowerScopeAsync(followerId, authorizedBranches, branchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;

        var (_, followerName, _, _) = scope.Data!;
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        // القضايا مرتبطة بـ FollowerUserId مباشرة (ليست IsLegal)
        var baseQuery = _db.LegalFollowupCases.AsNoTracking()
            .Where(c => c.FollowerUserId == followerId
                        && c.Status != LegalFollowupStatus.Cancelled);

        var totalCount = await baseQuery.CountAsync(ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var rows = await baseQuery
            .OrderBy(c => c.Status == LegalFollowupStatus.Completed ? 1 : 0)
            .ThenByDescending(c => c.CompletedAtUtc ?? c.CreatedAtUtc)
            .ThenByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows.Select(c => new FollowerOldCustomerRowDto(
            c.Id,
            c.CustomerId,
            c.SalesRequestId,
            c.CustomerName,
            c.CustomerPhone,
            c.CustomerAddress,
            c.Status.ToString(),
            LegalFollowupStatusLabels.Arabic(c.Status),
            c.Note,
            c.CompletionNote,
            c.CreatedAtUtc,
            c.CompletedAtUtc,
            c.IsDone,
            LegalFollowupCaseCanCancel(c.Status),
            c.RowVersion)).ToList();

        return new FollowerOldCustomersResponse(
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            "الزبائن القديمة (قانونية) — قضايا LegalFollowupCases للمتابع؛ ليست Customer.IsLegal وحدها",
            FollowerWorkRules.TimeZoneId,
            _clock.UtcNowOffset,
            followerId,
            followerName);
    }

    private static bool LegalFollowupCaseCanCancel(LegalFollowupStatus status)
        => Domain.Entities.LegalFollowupAggregate.LegalFollowupCase.CanCancel(status);
}
