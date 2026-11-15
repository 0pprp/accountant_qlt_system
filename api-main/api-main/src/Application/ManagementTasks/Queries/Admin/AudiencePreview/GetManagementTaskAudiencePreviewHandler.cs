using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Queries.Admin.AudiencePreview;

public class GetManagementTaskAudiencePreviewHandler
    : IRequestHandler<GetManagementTaskAudiencePreviewQuery, Result<ManagementTaskAudiencePreviewDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetManagementTaskAudiencePreviewHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ManagementTaskAudiencePreviewDto>> Handle(
        GetManagementTaskAudiencePreviewQuery request,
        CancellationToken cancellationToken)
    {
        var allowed = _currentUser.HasPermission(SeedPermissions.ManagementTask.Create)
                      || _currentUser.HasPermission(SeedPermissions.ManagementTask.Assign);
        if (allowed == false)
            return ManagementTaskErrors.ActionForbidden;

        if (Enum.IsDefined(request.Audience) == false || request.Audience == ManagementTaskAudience.User)
            return ManagementTaskErrors.InvalidAudience;

        var branchError = ManagementTaskAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        var (recipients, error) = await ManagementTaskAudienceResolver.ResolveAsync(
            _db,
            request.Audience,
            request.BranchId,
            null,
            _clock.UtcNowOffset,
            cancellationToken);

        if (error is not null)
        {
            if (error.Value.Code == ManagementTaskErrors.AudienceEmpty.Code)
                return new ManagementTaskAudiencePreviewDto(request.Audience, 0, []);

            return error.Value;
        }

        var list = recipients ?? [];
        var sample = list.Take(ManagementTaskAudienceResolver.PreviewSampleSize).ToList();
        return new ManagementTaskAudiencePreviewDto(request.Audience, list.Count, sample);
    }
}
