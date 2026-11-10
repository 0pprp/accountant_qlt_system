using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Queries.App.GetByTab;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.App.GetById;

public class GetAppSalesRequestByIdHandler
    : IRequestHandler<GetAppSalesRequestByIdQuery, Result<SalesRequestAppItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAppSalesRequestByIdHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<SalesRequestAppItemDto>> Handle(
        GetAppSalesRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null or <= 0)
            return SalesRequestErrors.NotOwner;

        var x = await _db.SalesRequests.AsNoTracking()
            .Where(r => r.Id == request.RequestId && !r.IsDeleted)
            .Select(r => new
            {
                r.Id,
                r.CustomerName,
                r.CustomerPhone,
                r.Address, r.RequestedProduct, r.SubmissionNote,
                r.Status,
                r.SourceType,
                r.BranchId,
                BranchName = r.Branch != null ? r.Branch.Name : null,
                r.TargetEmployeeId,
                r.LinkedOrderId,
                r.CreatedAt,
                r.StatusChangedAtUtc,
                r.ViewedAtUtc,
                r.LastStatusNote
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (x is null)
            return SalesRequestErrors.NotFound;

        // لا تكشف طلب فرع/ملكية محجوبة — NotFound لا NotOwner لتفادي التسريب
        if (x.TargetEmployeeId != userId)
            return SalesRequestErrors.NotFound;

        return new SalesRequestAppItemDto
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
        };
    }
}
