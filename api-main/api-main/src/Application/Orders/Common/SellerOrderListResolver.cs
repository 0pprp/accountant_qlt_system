using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Common;

public static class SellerOrderListResolver
{
    public static async Task<Result<int>> ResolveOrderListIdForSellerAsync(
        IApplicationDbContext dbContext,
        string? role,
        int sellerUserId,
        int? requestedOrderListId,
        CancellationToken cancellationToken)
    {
        if (role == Application.Common.SeedData.Roles.Mandob.Name)
        {
            if (requestedOrderListId is not null)
                return OrderErrors.MandobCanNotSetOrderListId;

            var mandobOrderList = await dbContext.OrderLists
                .Where(x => x.MandobId == sellerUserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (mandobOrderList is null)
                return OrderErrors.YouDoNotHaveAnyOrderList;

            return mandobOrderList.Id;
        }

        if (role == Application.Common.SeedData.Roles.Motaba.Name)
        {
            if (requestedOrderListId is null)
                return OrderErrors.OrderListIdIsRequired;

            var motabaOrderList = await dbContext.OrderLists
                .Where(x => x.Id == requestedOrderListId && x.MotabaId == sellerUserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (motabaOrderList is null)
                return OrderErrors.TheSellerDoesNotBelongToThisOrderList;

            return motabaOrderList.Id;
        }

        return OrderErrors.YouDoNotHavePermissionForThisAction;
    }
}