using Application.Common.Interfaces;
using Application.SalesRequests.Commands.Admin.Assign;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.AssignBatch;

public class AssignSalesRequestBatchHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    ISender sender)
    : IRequestHandler<AssignSalesRequestBatchCommand, Result<AssignSalesRequestBatchResponse>>
{
    public async Task<Result<AssignSalesRequestBatchResponse>> Handle(
        AssignSalesRequestBatchCommand request,
        CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        var rows = await db.SalesRequests.AsNoTracking()
            .Where(x => request.RequestIds.Contains(x.Id) && x.IsDeleted == false)
            .ToListAsync(cancellationToken);
        var unassigned = rows.Where(r => r.IsUnassigned()).Select(r => r.Id).ToHashSet();

        if (rows.Count != request.RequestIds.Count || rows.Any(r => branches.Contains(r.BranchId) == false))
            return SalesRequestErrors.NotFound;
        if (rows.Select(r => r.BranchId).Distinct().Count() > 1)
            return SalesRequestErrors.InvalidBranch;

        var items = new List<AssignSalesRequestBatchItem>(request.RequestIds.Count);
        foreach (var id in request.RequestIds)
        {
            if (unassigned.Contains(id) == false)
            {
                items.Add(new AssignSalesRequestBatchItem(
                    id, false, SalesRequestErrors.NotUnassigned.Code, SalesRequestErrors.NotUnassigned.Message));
                continue;
            }

            var result = await sender.Send(new AssignSalesRequestCommand { RequestId = id, EmployeeId = request.EmployeeId }, cancellationToken);
            items.Add(result.IsSucceeded
                ? new AssignSalesRequestBatchItem(id, true, null, null)
                : new AssignSalesRequestBatchItem(id, false, result.Error?.Code, result.Error?.Message));
        }

        return new AssignSalesRequestBatchResponse(items.Count(i => i.Succeeded), items.Count(i => i.Succeeded == false), items);
    }
}
