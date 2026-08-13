using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetTransactions;

public class GetSafeTransactionsHandler : IRequestHandler<GetSafeTransactionsQuery, Result<PaginatedList<GetSafeTransactionsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSafeTransactionsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaginatedList<GetSafeTransactionsResponse>>> Handle(GetSafeTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var transactions = await _dbContext.Transactions.AsNoTracking()
            .Where(x => x.SafeId == request.SafeId)
            .Where(x => x.Type == TransactionType.SellerPayment || x.Type == TransactionType.SafeTransfer)
            .Select(x => new GetSafeTransactionsResponse
            {
                Id = x.Id,
                Source = x.SafeTransferTransaction != null
                    ? x.SafeTransferTransaction.SourceSafe!.Name
                    : x.SellerCashDeliveryTransaction!.Seller!.FullName,
                Destination = x.SafeTransferTransaction != null
                    ? x.SafeTransferTransaction.DestinationSafe!.Name
                    : x.Safe!.Name,
                Amount = x.Amount,
                Type = x.Type,
                Status = x.Status,
                StatusDescription = x.StatusDescription,
                Direction = x.Direction,
                CreatedAt = x.CreatedAt!.Value
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return transactions;
    }
}