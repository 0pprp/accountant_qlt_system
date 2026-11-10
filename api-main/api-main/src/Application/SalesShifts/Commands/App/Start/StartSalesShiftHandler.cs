using Application.Common.Interfaces;
using Application.SalesEmployees.Common;
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
        var isBlocked = await _dbContext.SalesEmployeeAppBlocks.AsNoTracking()
            .AnyAsync(x => x.EmployeeUserId == request.UserId && x.IsBlocked, cancellationToken);
        if (isBlocked)
            return SalesShiftErrors.AppBlocked;

        var hasActive = await _dbContext.SalesShifts
            .AnyAsync(x => x.UserId == request.UserId && x.EndedAtUtc == null, cancellationToken);
        if (hasActive)
            return SalesShiftErrors.ActiveShiftAlreadyExists;

        var branchId = _currentUserService.BranchIds?.FirstOrDefault();
        if (branchId is null or 0)
            return SalesShiftErrors.BranchNotFound;

        var now = _dateTimeProvider.UtcNowOffset;
        // دوام تطبيق المبيعات يتطلب تكليف مبيعات ساري على الفرع — متابع اللجنة يستخدم نفس الكيان لكن عبر صلاحيات مختلفة؛
        // إن وُجد تكليف مبيعات نتحقق منه؛ وإلا نسمح لـ Motaba (مسار اللجنة) بدون تكليف مبيعات.
        var hasSalesAssignment = await SalesWorkforce.HasActiveAssignmentAsync(
            _dbContext, request.UserId, branchId.Value, now, cancellationToken);
        if (!hasSalesAssignment)
        {
            var isMotaba = await _dbContext.UserRoles.AsNoTracking()
                .AnyAsync(
                    ur => ur.UserId == request.UserId
                          && ur.Role != null
                          && ur.Role.Name == Application.Common.SeedData.Roles.Motaba.Name,
                    cancellationToken);
            if (!isMotaba)
                return SalesShiftErrors.SalesAssignmentRequired;
        }

        var shift = new SalesShift(request.UserId, branchId.Value, now);
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
