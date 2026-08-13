using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.UpdateSalaryDetail;

public class UpdateUserSalaryDetailHandler : IRequestHandler<UpdateUserSalaryDetailCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public UpdateUserSalaryDetailHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateUserSalaryDetailCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.UserBranches)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound;

        user.SalaryDetail.Update(request.Type, request.Amount, request.SaleSharePercent, request.InstallmentSharePercent);

        if (user.CreationStep == UserCreationStep.SalaryDetail)
            user.CreationStep = UserCreationStep.Permissions;

        foreach (var userBranch in user.UserBranches)
        {
            await _activityLogService.AddAsync(
                userBranch.BranchId,
                ActivityType.UserSalaryDetailUpdated,
                TargetEntityType.User,
                request.UserId);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}