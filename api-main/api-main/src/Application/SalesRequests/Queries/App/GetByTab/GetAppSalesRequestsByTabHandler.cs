using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.App.GetByTab;

public class GetAppSalesRequestsByTabHandler
    : IRequestHandler<GetAppSalesRequestsByTabQuery, Result<List<SalesRequestAppItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAppSalesRequestsByTabHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<SalesRequestAppItemDto>>> Handle(
        GetAppSalesRequestsByTabQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null or <= 0)
            return SalesRequestErrors.NotOwner;

        var statuses = SalesRequestTabMap.StatusesForTab(request.Tab);
        if (statuses.Count == 0)
            return SalesRequestErrors.InvalidTab;

        // هوية القارئ من الجلسة فقط — لا يُثق بـ employeeId من العميل
        var rows = await _db.SalesRequests.AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.TargetEmployeeId == userId
                        && statuses.Contains(x.Status))
            .OrderByDescending(x => x.StatusChangedAtUtc ?? x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.CustomerName,
                x.CustomerPhone,
                x.Address, x.RequestedProduct, x.SubmissionNote,
                x.Status,
                x.SourceType,
                x.BranchId,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                x.TargetEmployeeId,
                x.LinkedOrderId,
                x.CreatedAt,
                x.StatusChangedAtUtc,
                x.ViewedAtUtc,
                x.LastStatusNote
            })
            .ToListAsync(cancellationToken);

        return rows.Select(x => new SalesRequestAppItemDto
        {
            RequestId = x.Id,
            CustomerName = x.CustomerName,
            CustomerPhone = x.CustomerPhone,
            Address = x.Address,
            RequestedProduct = x.RequestedProduct,
            SubmissionNote = x.SubmissionNote,
            Status = x.Status,
            StatusLabel = SalesRequestStatusLabels.For(x.Status),
            TabKey = SalesRequestTabMap.DisplayTabForStatus(x.Status),
            SourceType = x.SourceType,
            SourceTypeLabel = SalesRequestSourceLabels.For(x.SourceType),
            BranchId = x.BranchId,
            BranchName = x.BranchName,
            TargetEmployeeId = x.TargetEmployeeId,
            LinkedOrderId = x.LinkedOrderId,
            CreatedAt = x.CreatedAt,
            StatusChangedAtUtc = x.StatusChangedAtUtc,
            ViewedAtUtc = x.ViewedAtUtc,
            LastStatusNote = x.LastStatusNote,
            IsReturned = x.Status == Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.Returned
        }).ToList();
    }
}
