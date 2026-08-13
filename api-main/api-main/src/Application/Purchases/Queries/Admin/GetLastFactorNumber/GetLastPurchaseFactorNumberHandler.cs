using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Purchases.Queries.Admin.GetLastFactorNumber;

public class GetLastPurchaseFactorNumberHandler : IRequestHandler<GetLastPurchaseFactorNumberQuery,
    Result<GetLastPurchaseFactorNumberResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetLastPurchaseFactorNumberHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetLastPurchaseFactorNumberResponse>> Handle(GetLastPurchaseFactorNumberQuery request,
        CancellationToken cancellationToken)
    {
        var lastFactorNumber = await _dbContext.Purchases
            .Select(x => (int?)x.FactorNumber)
            .MaxAsync(cancellationToken) ?? 0;

        return new GetLastPurchaseFactorNumberResponse
        {
            LastFactorNumber = lastFactorNumber
        };
    }
}