using Application.Branches.Common;
using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Branches.Commands.Admin.Update;

public class UpdateBranchHandler : IRequestHandler<UpdateBranchCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public UpdateBranchHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _dbContext.Branches.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (branch is null)
            return BranchErrors.BranchNotFound;

        branch.Update(request.Name, request.ProvinceId);

        await _activityLogService.AddAsync(
            request.Id,
            ActivityType.BranchUpdated,
            TargetEntityType.Branch,
            request.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}