using Application.Attachments.Common;
using Application.Branches.Common;
using Application.Common.Interfaces;
using Application.Permissions.Common;
using Application.Users.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.Admin.GetById;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<GetUserByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetUserByIdResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .Select(x => new GetUserByIdResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                MotherName = x.MotherName,
                Username = x.Username,
                NationalCode = x.NationalCode,
                BirthDate = x.BirthDate,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                CreationStep = x.CreationStep,
                SalaryDetail = x.SalaryDetail,
                Attachments = x.Attachments.Select(a => new GetAttachmentDto
                {
                    Id = a.Id,
                    OriginalFileName = a.OriginalFileName,
                    RelativePath = a.RelativePath,
                    FileSizeInByte = a.FileSizeInByte,
                    Type = a.Type
                }).ToList(),
                Branches = x.UserBranches.Select(ub => new GetBranchDto
                {
                    Id = ub.BranchId,
                    Name = ub.Branch!.Name
                }).ToList(),
                Roles = x.UserRoles.Select(ur => new GetRoleDto
                {
                    Id = ur.RoleId,
                    Name = ur.Role!.Name
                }).ToList(),
                Permissions = x.UserRoles.SelectMany(ur => ur.Role!.RolePermissions).Select(rp => new GetPermissionDto
                {
                    Id = rp.PermissionId,
                    Name = rp.Permission!.Name
                }).ToList(),
                CustomPermissions = x.UserPermissions.Select(up => new GetPermissionDto
                {
                    Id = up.PermissionId,
                    Name = up.Permission!.Name
                }).ToList()
            })
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound;

        return user;
    }
}