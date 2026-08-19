using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.SafeAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Common;

public static class SafeCashHolderQuery
{
    public static IQueryable<int> UserIdsForSafe(
        IQueryable<OrderList> orderLists,
        IQueryable<Order> orders,
        IQueryable<Safe> safes,
        int safeId)
    {
        var branchIds = safes
            .Where(x => x.Id == safeId && x.BranchId != null)
            .Select(x => x.BranchId!.Value);

        var mandobIds = orderLists
            .Where(x => branchIds.Contains(x.BranchId))
            .Select(x => x.MandobId);

        var motabaIds = orderLists
            .Where(x => branchIds.Contains(x.BranchId))
            .Select(x => x.MotabaId);

        var sellerIds = orders
            .Where(x => branchIds.Contains(x.BranchId) && x.SellerId != null)
            .Select(x => x.SellerId!.Value);

        return mandobIds.Union(motabaIds).Union(sellerIds);
    }

    public static IQueryable<int> UserIdsForBranch(
        IQueryable<OrderList> orderLists,
        IQueryable<Order> orders,
        int branchId)
    {
        var mandobIds = orderLists
            .Where(x => x.BranchId == branchId)
            .Select(x => x.MandobId);

        var motabaIds = orderLists
            .Where(x => x.BranchId == branchId)
            .Select(x => x.MotabaId);

        var sellerIds = orders
            .Where(x => x.BranchId == branchId && x.SellerId != null)
            .Select(x => x.SellerId!.Value);

        return mandobIds.Union(motabaIds).Union(sellerIds);
    }

    public static async Task<bool> BelongsToBranchAsync(
        IApplicationDbContext dbContext,
        int userId,
        int branchId,
        CancellationToken cancellationToken)
    {
        var belongsViaList = await dbContext.OrderLists.AnyAsync(
            x => x.BranchId == branchId && (x.MandobId == userId || x.MotabaId == userId),
            cancellationToken);
        if (belongsViaList)
            return true;

        return await dbContext.Orders.AnyAsync(
            x => x.BranchId == branchId && x.SellerId == userId,
            cancellationToken);
    }
}
