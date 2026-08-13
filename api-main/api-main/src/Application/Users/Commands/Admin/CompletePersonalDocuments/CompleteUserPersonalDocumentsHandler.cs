using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.CompletePersonalDocuments;

public class CompleteUserPersonalDocumentsHandler : IRequestHandler<CompleteUserPersonalDocumentsCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public CompleteUserPersonalDocumentsHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CompleteUserPersonalDocumentsCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.UserBranches)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound;

        if (user.CreationStep == UserCreationStep.PersonalDocuments)
            user.CreationStep = UserCreationStep.SalaryDetail;

        foreach (var userBranch in user.UserBranches)
        {
            await _activityLogService.AddAsync(
                userBranch.BranchId,
                ActivityType.UserPersonalDocumentsCompleted,
                TargetEntityType.User,
                request.UserId);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}