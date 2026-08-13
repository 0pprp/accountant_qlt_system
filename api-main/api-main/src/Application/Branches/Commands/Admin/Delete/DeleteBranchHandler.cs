using Application.Branches.Common;
using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Branches.Commands.Admin.Delete;

public class DeleteBranchHandler : IRequestHandler<DeleteBranchCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public DeleteBranchHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _dbContext.Branches
            .Include(x => x.Warehouses)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (branch is null)
            return BranchErrors.BranchNotFound;

        if (branch.Warehouses.Count != 0)
            return BranchErrors.BranchStillReferencedByWarehouses;

        _dbContext.Branches.Remove(branch);

        await _activityLogService.AddAsync(
            branch.Id,
            ActivityType.BranchDeleted,
            TargetEntityType.Branch,
            branch.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}