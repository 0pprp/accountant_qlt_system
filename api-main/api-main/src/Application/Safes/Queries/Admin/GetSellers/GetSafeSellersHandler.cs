using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Safes.Common;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetSellers;

public class GetSafeSellersHandler : IRequestHandler<GetSafeSellersQuery, Result<PaginatedList<GetSafeSellersResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSafeSellersHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaginatedList<GetSafeSellersResponse>>> Handle(GetSafeSellersQuery request,
        CancellationToken cancellationToken)
    {
        var cashHolderIds = SafeCashHolderQuery.UserIdsForSafe(
            _dbContext.OrderLists,
            _dbContext.Orders,
            _dbContext.Safes,
            request.SafeId);

        var sellers = await _dbContext.Users.AsNoTracking()
            .Where(x => cashHolderIds.Contains(x.Id))
            .When(request.Filter.SearchTerm is not null, x => x.FullName.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetSafeSellersResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                RoleName = x.UserRoles.Select(ur => ur.Role!.DisplayName).FirstOrDefault(),
                OrderListName = _dbContext.OrderLists
                    .Where(ol => _dbContext.Safes.Any(s => s.Id == request.SafeId && s.BranchId == ol.BranchId)
                                 && (ol.MandobId == x.Id || ol.MotabaId == x.Id))
                    .OrderBy(ol => ol.MandobId == x.Id ? 0 : 1)
                    .Select(ol => ol.Name)
                    .FirstOrDefault(),
                DeliveredCashAmount = x.SellerCashDeliveryTransactions
                    .Where(s => s.Transaction!.Status == TransactionStatus.Approved)
                    .Sum(s => s.Transaction!.Amount),
                UndeliveredCashAmount = x.UndeliveredCashAmount,
                LastCashDeliveryDate = x.SellerCashDeliveryTransactions.Max(s => (DateTimeOffset?)s.Transaction!.CreatedAt!.Value),
                LastCashDeliveryDescription = x.SellerCashDeliveryTransactions
                    .OrderByDescending(s => s.Transaction!.CreatedAt)
                    .Select(s => s.Description)
                    .FirstOrDefault()
            })
            .OrderByDescending(x => x.LastCashDeliveryDate)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return sellers;
    }
}