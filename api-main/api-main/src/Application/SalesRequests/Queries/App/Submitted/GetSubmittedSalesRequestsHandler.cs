using Application.SalesRequests.Queries.App.GetByTab;
using Application.Common.Interfaces;
using Application.SalesRequests.Commands.App.Submit;
using Application.SalesRequests.Common;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.App.Submitted;

/// <summary>Source inbox, never the employee assignment inbox. No client-supplied author id.</summary>
public class GetSubmittedSalesRequestsHandler(IApplicationDbContext db, ICurrentUserService user)
    : IRequestHandler<GetSubmittedSalesRequestsQuery, Result<SubmittedSalesRequestsPage>>
{
    public async Task<Result<SubmittedSalesRequestsPage>> Handle(GetSubmittedSalesRequestsQuery request, CancellationToken ct)
    {
        var permission = SalesRequestSubmissionRules.Permission(request.Source);
        if (user.UserId is not int userId || userId <= 0 || permission is null || !user.HasPermission(permission))
            return SalesRequestErrors.TabForbidden;
        if (request.Tab is not ("all" or "inProgress" or "rejected" or "completed") || request.Page < 1 || request.Page > 10000)
            return SalesRequestErrors.InvalidTab;
        if (user.BranchIds is not { Count: > 0 }) return SalesRequestErrors.BranchForbidden;

        var query = db.SalesRequests.AsNoTracking().Where(x => x.CreatedByUserId == userId && x.SourceType == request.Source
            && user.BranchIds.Contains(x.BranchId)
            && db.Users.Any(u => u.Id == userId && u.UserBranches.Any(b => b.BranchId == x.BranchId)));
        query = request.Tab switch {
            "rejected" => query.Where(x => x.Status == SalesRequestStatus.Rejected || (x.IsDeleted && x.Status != SalesRequestStatus.Completed)),
            "completed" => query.Where(x => !x.IsDeleted && x.Status == SalesRequestStatus.Completed),
            "inProgress" => query.Where(x => !x.IsDeleted && x.Status != SalesRequestStatus.Rejected && x.Status != SalesRequestStatus.Completed),
            _ => query.Where(x => !x.IsDeleted || x.Status != SalesRequestStatus.Completed)
        };
        const int size = 30;
        var rows = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((request.Page - 1) * size).Take(size + 1)
            .Select(x => new { x.Id, x.CustomerName, x.CustomerPhone, x.Address, x.RequestedProduct,
                x.SubmissionNote, x.Status, Employee = x.TargetEmployee == null ? null : x.TargetEmployee.FullName,
                x.RejectReason, x.DeleteReason, x.IsDeleted, x.LastStatusNote, x.CreatedAt, Branch = x.Branch == null ? null : x.Branch.Name })
            .ToListAsync(ct);
        return new SubmittedSalesRequestsPage(rows.Take(size).Select(x => new SubmittedSalesRequestRow(
            x.Id, x.CustomerName, x.CustomerPhone, x.Address, x.RequestedProduct, x.SubmissionNote,
            SalesRequestStatusLabels.For(x.IsDeleted ? SalesRequestStatus.Rejected : x.Status), x.Employee,
            string.IsNullOrWhiteSpace(x.RejectReason) ? x.DeleteReason : x.RejectReason,
            x.Status == SalesRequestStatus.Pending ? x.LastStatusNote : null, x.CreatedAt, x.Branch)).ToList(),
            request.Page, rows.Count > size);
    }
}

