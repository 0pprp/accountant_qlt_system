using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesShifts.Queries.App.GetCurrent;

public class GetCurrentSalesShiftHandler : IRequestHandler<GetCurrentSalesShiftQuery, Result<GetCurrentSalesShiftResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurrentSalesShiftHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetCurrentSalesShiftResponse>> Handle(GetCurrentSalesShiftQuery request, CancellationToken cancellationToken)
    {
        var shift = await _dbContext.SalesShifts.AsNoTracking()
            .Where(x => x.UserId == request.UserId && x.EndedAtUtc == null)
            .Select(x => new GetCurrentSalesShiftResponse
            {
                HasActiveShift = true,
                Id = x.Id,
                UserId = x.UserId,
                BranchId = x.BranchId,
                StartedAtUtc = x.StartedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        return shift ?? new GetCurrentSalesShiftResponse { HasActiveShift = false };
    }
}
