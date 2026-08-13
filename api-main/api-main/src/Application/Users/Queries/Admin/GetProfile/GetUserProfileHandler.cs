using Application.Attachments.Common;
using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.AttachmentAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.Admin.GetProfile;

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
                Roles = x.UserRoles.Select(ur => ur.Role!.DisplayName).ToList()
            })
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound;

        return user;
    }
}