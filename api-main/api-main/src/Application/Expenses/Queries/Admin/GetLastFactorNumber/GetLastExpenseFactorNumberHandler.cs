using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Expenses.Queries.Admin.GetLastFactorNumber;

public class GetLastExpenseFactorNumberHandler : IRequestHandler<GetLastExpenseFactorNumberQuery,
    Result<GetLastExpenseFactorNumberResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetLastExpenseFactorNumberHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetLastExpenseFactorNumberResponse>> Handle(GetLastExpenseFactorNumberQuery request,
        CancellationToken cancellationToken)
    {
        var lastFactorNumber = await _dbContext.Expenses
            .Select(x => (int?)x.FactorNumber)
            .MaxAsync(cancellationToken) ?? 0;

        return new GetLastExpenseFactorNumberResponse
        {
            LastFactorNumber = lastFactorNumber
        };
    }
}