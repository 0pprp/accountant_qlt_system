using System.Text.Json;
using Application.Common.Interfaces;
using Application.SalesRequests.Commands.Admin.DeleteSoldSale;
using Application.SalesRequests.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Queries.Admin.DeletedSales;

public record GetDeletedSalesQuery : IRequest<Result<GetDeletedSalesResponse>>
{
    public int? BranchId { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

public sealed record DeletedSaleDto(
    int RequestId,
    int? OrderId,
    string CustomerName,
    string? CustomerPhone,
    int BranchId,
    string? BranchName,
    DateTimeOffset? DeletedAtUtc,
    string? DeletedByName,
    string? Reason,
    string? SaleDate,
    double? SellAmount,
    double? PrepaymentAmount,
    double? PaidTotal,
    int? PaymentsCount,
    string? Items);

public sealed record GetDeletedSalesResponse(IReadOnlyList<DeletedSaleDto> Items, int TotalCount, int Page, int PageSize);

public class GetDeletedSalesHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetDeletedSalesQuery, Result<GetDeletedSalesResponse>>
{
    public async Task<Result<GetDeletedSalesResponse>> Handle(GetDeletedSalesQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.HasPermission(SeedPermissions.SalesRequest.DeletedSalesRead) == false)
            return SalesRequestErrors.ActionForbidden;

        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        if (request.BranchId is int b && branches.Contains(b) == false)
            return SalesRequestErrors.BranchForbidden;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = db.SalesRequests.AsNoTracking()
            .Where(x => x.IsDeleted && x.Status == SalesRequestStatus.Completed && branches.Contains(x.BranchId));
        if (request.BranchId is int branchId)
            query = query.Where(x => x.BranchId == branchId);
        if (string.IsNullOrWhiteSpace(request.Search) == false)
        {
            var term = request.Search.Trim();
            query = query.Where(x => x.CustomerName.Contains(term) || (x.CustomerPhone != null && x.CustomerPhone.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.DeletedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.CustomerName,
                x.CustomerPhone,
                x.BranchId,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                x.DeletedAtUtc,
                x.DeletedByUserId,
                x.DeleteReason
            })
            .ToListAsync(cancellationToken);

        var deleterIds = rows.Where(r => r.DeletedByUserId != null).Select(r => r.DeletedByUserId!.Value).Distinct().ToList();
        var deleters = await db.Users.AsNoTracking()
            .Where(u => deleterIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var rowBranches = rows.Select(r => r.BranchId).Distinct().ToList();
        var marker = $"\"kind\":\"{DeletedSoldSaleLog.Marker}\"";
        var logs = await db.ActivityLogs.AsNoTracking()
            .Where(l => l.ActivityType == ActivityType.OrderDeleted
                        && l.TargetEntityType == TargetEntityType.Order
                        && rowBranches.Contains(l.BranchId)
                        && l.Description.Contains(marker))
            .Select(l => l.Description)
            .ToListAsync(cancellationToken);
        var snapshots = logs
            .Select(Parse)
            .Where(s => s is not null)
            .GroupBy(s => s!.RequestId)
            .ToDictionary(g => g.Key, g => g.First()!);

        var items = rows.Select(r =>
        {
            snapshots.TryGetValue(r.Id, out var snap);
            return new DeletedSaleDto(
                r.Id,
                snap?.OrderId,
                r.CustomerName,
                r.CustomerPhone,
                r.BranchId,
                r.BranchName,
                r.DeletedAtUtc,
                r.DeletedByUserId is int id && deleters.TryGetValue(id, out var name) ? name : null,
                r.DeleteReason,
                snap?.SaleDate,
                snap?.SellAmount,
                snap?.PrepaymentAmount,
                snap?.PaidTotal,
                snap?.PaymentsCount,
                snap?.Items);
        }).ToList();

        return new GetDeletedSalesResponse(items, total, page, pageSize);
    }

    private sealed record Snapshot(int RequestId, int? OrderId, string? SaleDate, double? SellAmount,
        double? PrepaymentAmount, double? PaidTotal, int? PaymentsCount, string? Items);

    private static Snapshot? Parse(string description)
    {
        var start = description.IndexOf('{');
        if (start < 0)
            return null;
        try
        {
            using var doc = JsonDocument.Parse(description[start..]);
            var root = doc.RootElement;
            if (root.TryGetProperty("r", out var r) == false)
                return null;
            return new Snapshot(
                r.GetInt32(),
                root.TryGetProperty("o", out var o) ? o.GetInt32() : null,
                root.TryGetProperty("d", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : null,
                root.TryGetProperty("s", out var s) ? s.GetDouble() : null,
                root.TryGetProperty("pp", out var pp) ? pp.GetDouble() : null,
                root.TryGetProperty("paid", out var paid) ? paid.GetDouble() : null,
                root.TryGetProperty("n", out var n) ? n.GetInt32() : null,
                root.TryGetProperty("items", out var items) ? items.GetString() : null);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
