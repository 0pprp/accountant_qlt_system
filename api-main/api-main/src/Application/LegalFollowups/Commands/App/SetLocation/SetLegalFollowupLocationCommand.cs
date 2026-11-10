using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.LegalFollowups.Common;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Commands.App.SetLocation;

public sealed class SetLegalFollowupLocationCommand : IRequest<Result<LegalFollowupCaseRowDto>>
{
    public int CaseId { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int? ExpectedVersion { get; init; }
}

public sealed class SetLegalFollowupLocationHandler
    : IRequestHandler<SetLegalFollowupLocationCommand, Result<LegalFollowupCaseRowDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly LegalFollowupDataService _data;

    public SetLegalFollowupLocationHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        LegalFollowupDataService data)
    {
        _db = db;
        _current = current;
        _data = data;
    }

    public async Task<Result<LegalFollowupCaseRowDto>> Handle(
        SetLegalFollowupLocationCommand request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.LegalFollowup.FieldUpload) == false
            && _current.HasPermission(SeedPermissions.LegalFollowup.FieldComplete) == false)
            return new Error("غير مخول بتحديث موقع المتابعة.", "LegalFollowup.FieldLocationDenied");
        if (_current.UserId is not int followerId)
            return new Error("المستخدم غير معروف.", "LegalFollowup.NoActor");

        var entity = await _db.LegalFollowupCases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
        if (entity is null || entity.FollowerUserId != followerId)
            return new Error("المتابعة غير موجودة.", "LegalFollowup.NotFound");
        if (entity.Status != LegalFollowupStatus.InProgress)
            return new Error("المتابعة ليست قيد العمل.", "LegalFollowup.NotInProgress");

        try
        {
            entity.SetShopLocation(request.Latitude, request.Longitude, request.ExpectedVersion);
        }
        catch (InvalidOperationException ex)
        {
            return new Error(ex.Message, "LegalFollowup.LocationFailed");
        }

        await _db.SaveChangesAsync(cancellationToken);
        var branchId = await _data.ResolveBranchIdAsync(entity, cancellationToken);
        return new LegalFollowupCaseRowDto
        {
            Id = entity.Id,
            SalesRequestId = entity.SalesRequestId,
            CustomerId = entity.CustomerId,
            CustomerName = entity.CustomerName,
            CustomerPhone = entity.CustomerPhone,
            CustomerAddress = entity.CustomerAddress,
            FollowerUserId = entity.FollowerUserId,
            FollowerName = entity.FollowerName,
            Status = entity.Status.ToString(),
            StatusLabel = LegalFollowupStatusLabels.Arabic(entity.Status),
            Note = entity.Note,
            CompletionNote = entity.CompletionNote,
            CreatedAtUtc = entity.CreatedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            RowVersion = entity.RowVersion,
            CanCancel = false,
            BranchId = branchId,
            ShopLatitude = entity.ShopLatitude,
            ShopLongitude = entity.ShopLongitude,
        };
    }
}
