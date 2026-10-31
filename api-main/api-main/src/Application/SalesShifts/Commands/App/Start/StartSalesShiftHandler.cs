using Application.Common.Interfaces;
using Application.SalesShifts.Common;
using Domain.Entities.SalesShiftAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesShifts.Commands.App.Start;

public class StartSalesShiftHandler : IRequestHandler<StartSalesShiftCommand, Result<StartSalesShiftResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StartSalesShiftHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<StartSalesShiftResponse>> Handle(StartSalesShiftCommand request, CancellationToken cancellationToken)
    {
        var hasActive = await _dbContext.SalesShifts
            .AnyAsync(x => x.UserId == request.UserId && x.EndedAtUtc == null, cancellationToken);
        if (hasActive)
            return SalesShiftErrors.ActiveShiftAlreadyExists;

        var branchId = _currentUserService.BranchIds?.FirstOrDefault();
        if (branchId is null or 0)
            return SalesShiftErrors.BranchNotFound;

        var shift = new SalesShift(request.UserId, branchId.Value, _dateTimeProvider.UtcNowOffset);
        _dbContext.SalesShifts.Add(shift);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new StartSalesShiftResponse
        {
            Id = shift.Id,
            UserId = shift.UserId,
            BranchId = shift.BranchId,
            StartedAtUtc = shift.StartedAtUtc,
            HasActiveShift = true
        };
    }
}
