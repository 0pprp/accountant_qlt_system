using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
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
        var sellers = await _dbContext.Users.AsNoTracking()
            .Where(x => x.OrderListAsMandob!.Branch!.Safe!.Id == request.SafeId)
            .When(request.Filter.SearchTerm is not null, x => x.FullName.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetSafeSellersResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                OrderListName = x.OrderListAsMandob != null ? x.OrderListAsMandob.Name : null,
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