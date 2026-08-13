using Application.Attachments.Common;
using Application.Branches.Common;
using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.AttachmentAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.App.GetProfile;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, Result<GetUserProfileResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserProfileHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetUserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .Select(x => new GetUserProfileResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                Branches = x.UserBranches.Select(ub => new GetBranchDto
                {
                    Id = ub.BranchId,
                    Name = ub.Branch!.Name
                }).ToList(),
                ProfilePicture = x.Attachments
                    .Select(a => new GetAttachmentDto
                    {
                        Id = a.Id,
                        OriginalFileName = a.OriginalFileName,
                        RelativePath = a.RelativePath,
                        FileSizeInByte = a.FileSizeInByte,
                        Type = a.Type
                    })
                    .FirstOrDefault(a => a.Type == AttachmentType.ProfilePicture),
                PhoneNumber = x.PhoneNumber,
                CreatedAt = x.CreatedAt!.Value,
                Address = x.Address,
                OrderList = x.OrderListAsMandob != null
                    ? new OrderListDto
                    {
                        Id = x.OrderListAsMandob!.Id,
                        Name = x.OrderListAsMandob.Name
                    }
                    : null
            })
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound;

        return user;
    }
}