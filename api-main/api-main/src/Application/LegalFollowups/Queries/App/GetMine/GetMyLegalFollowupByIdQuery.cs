using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.LegalFollowups.Common;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Queries.App.GetMine;

public sealed class GetMyLegalFollowupByIdQuery : IRequest<Result<LegalFollowupCaseRowDto>>
{
    public int CaseId { get; init; }
}

public sealed class GetMyLegalFollowupByIdHandler
    : IRequestHandler<GetMyLegalFollowupByIdQuery, Result<LegalFollowupCaseRowDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly LegalFollowupDataService _data;

    public GetMyLegalFollowupByIdHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        LegalFollowupDataService data)
    {
        _db = db;
        _current = current;
        _data = data;
    }

    public async Task<Result<LegalFollowupCaseRowDto>> Handle(
        GetMyLegalFollowupByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.LegalFollowup.FieldRead) == false)
            return new Error("غير مخول بقراءة متابعاتك القانونية.", "LegalFollowup.FieldReadDenied");
        if (_current.UserId is not int followerId)
            return new Error("المستخدم غير معروف.", "LegalFollowup.NoActor");

        var entity = await _db.LegalFollowupCases.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CaseId && c.FollowerUserId == followerId, cancellationToken);
        if (entity is null)
            return new Error("المتابعة غير موجودة.", "LegalFollowup.NotFound");

        var docs = await _data.ListDocumentsAsync(entity.Id, cancellationToken);
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
            Documents = docs,
        };
    }
}
