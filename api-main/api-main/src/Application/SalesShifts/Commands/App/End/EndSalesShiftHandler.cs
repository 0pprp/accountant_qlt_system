using Application.Common.Interfaces;
using Application.SalesShifts.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesShifts.Commands.App.End;

public class EndSalesShiftHandler : IRequestHandler<EndSalesShiftCommand, Result<EndSalesShiftResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EndSalesShiftHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<EndSalesShiftResponse>> Handle(EndSalesShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _dbContext.SalesShifts
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.EndedAtUtc == null, cancellationToken);
        if (shift is null)
            return SalesShiftErrors.ActiveShiftNotFound;

        var endedAt = _dateTimeProvider.UtcNowOffset;
        shift.End(endedAt);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new EndSalesShiftResponse
        {
            Id = shift.Id,
            UserId = shift.UserId,
            BranchId = shift.BranchId,
            StartedAtUtc = shift.StartedAtUtc,
            EndedAtUtc = endedAt,
            HasActiveShift = false
        };
    }
}
