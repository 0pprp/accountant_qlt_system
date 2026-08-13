using Application.Branches.Common;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.WarehouseAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Branches.Commands.Admin.Create;

public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly AdminData _adminData;
    private readonly IActivityLogService _activityLogService;

    public CreateBranchHandler(IApplicationDbContext dbContext, AdminData adminData, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _adminData = adminData;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var isBranchExist = await _dbContext.Branches.AnyAsync(x => x.Name == request.Name, cancellationToken);
        if (isBranchExist)
            return BranchErrors.BranchAlreadyExist;

        var superAdmin = await _dbContext.Users.FirstAsync(x => x.Username == _adminData.Username, cancellationToken);

        var branch = new Branch(request.Name, request.ProvinceId)
        {
            Warehouses = [new Warehouse(request.Name)],
            UserBranches = [new UserBranch
            {
                UserId = superAdmin.Id
            }]
        };
        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            branch.Id,
            ActivityType.BranchCreated,
            TargetEntityType.Branch,
            branch.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}