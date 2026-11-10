using Application.LegalFollowups.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Queries.App.ListMine;

public sealed class ListMyLegalFollowupsQuery : IRequest<Result<ListLegalFollowupsResponse>>
{
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class ListMyLegalFollowupsHandler
    : IRequestHandler<ListMyLegalFollowupsQuery, Result<ListLegalFollowupsResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListMyLegalFollowupsHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<ListLegalFollowupsResponse>> Handle(
        ListMyLegalFollowupsQuery request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.LegalFollowup.FieldRead) == false)
            return new Error("غير مخول بقراءة متابعاتك القانونية.", "LegalFollowup.FieldReadDenied");
        if (_current.UserId is not int followerId)
            return new Error("المستخدم غير معروف.", "LegalFollowup.NoActor");

        LegalFollowupStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse(request.Status.Trim(), true, out LegalFollowupStatus parsed)
                || parsed == LegalFollowupStatus.Cancelled)
                return new Error("حالة غير مسموحة.", "LegalFollowup.InvalidStatus");
            status = parsed;
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = _db.LegalFollowupCases.AsNoTracking()
            .Where(c => c.FollowerUserId == followerId);
        if (status is not null)
            query = query.Where(c => c.Status == status.Value);
        else
            query = query.Where(c => c.Status != LegalFollowupStatus.Cancelled);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(c => new LegalFollowupCaseRowDto
        {
            Id = c.Id,
            SalesRequestId = c.SalesRequestId,
            CustomerId = c.CustomerId,
            CustomerName = c.CustomerName,
            CustomerPhone = c.CustomerPhone,
            CustomerAddress = c.CustomerAddress,
            FollowerUserId = c.FollowerUserId,
            FollowerName = c.FollowerName,
            Status = c.Status.ToString(),
            StatusLabel = LegalFollowupStatusLabels.Arabic(c.Status),
            Note = c.Note,
            CompletionNote = c.CompletionNote,
            CreatedAtUtc = c.CreatedAtUtc,
            CompletedAtUtc = c.CompletedAtUtc,
            RowVersion = c.RowVersion,
            CanCancel = false,
            ShopLatitude = c.ShopLatitude,
            ShopLongitude = c.ShopLongitude,
        }).ToList();

        return new ListLegalFollowupsResponse
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            BranchScope = [],
        };
    }
}
