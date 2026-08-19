using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Common;

public static class OrderCashHolder
{
    public static async Task<Result<User>> GetAsync(
        IApplicationDbContext dbContext,
        int? sellerId,
        CancellationToken cancellationToken)
    {
        if (sellerId is null)
            return UserErrors.UserNotFound;

        var seller = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == sellerId, cancellationToken);
        if (seller is null)
            return UserErrors.UserNotFound;

        return seller;
    }
}
