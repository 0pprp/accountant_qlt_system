using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.Admin.FilterOptions;

public record SalesRequestFilterOption(int Id, string Name);
public record SalesRequestFilterOptions(List<SalesRequestFilterOption> Employees, List<SalesRequestFilterOption> Provinces);
public record GetSalesRequestFilterOptionsQuery(string Tab, int? BranchId) : IRequest<Result<SalesRequestFilterOptions>>;
public class GetSalesRequestFilterOptionsHandler(IApplicationDbContext db, ICurrentUserService user)
    : IRequestHandler<GetSalesRequestFilterOptionsQuery, Result<SalesRequestFilterOptions>>
{
    public async Task<Result<SalesRequestFilterOptions>> Handle(GetSalesRequestFilterOptionsQuery request, CancellationToken ct)
    {
        var scope = SalesRequestAdminQuery.ApplyTab(db, user, SalesRequestAccess.AuthorizedBranches(user), request.Tab);
        if (scope.IsFailed || scope.Data is null) return scope.Error!;
        if (request.BranchId is int id && SalesRequestAccess.CanAccessBranch(user, id) == false)
            return SalesRequestErrors.BranchForbidden;
        var query = scope.Data.Query;
        if (request.BranchId is int branch) query = query.Where(x => x.BranchId == branch);
        var employees = await query.Where(x => x.TargetEmployeeId != null && x.TargetEmployee != null)
            .Select(x => new { Id = x.TargetEmployeeId!.Value, Name = x.TargetEmployee!.FullName })
            .Distinct().OrderBy(x => x.Name).Select(x => new SalesRequestFilterOption(x.Id, x.Name)).ToListAsync(ct);
        var provinces = await query.Where(x => x.ProvinceId != null && x.Province != null)
            .Select(x => new { Id = x.ProvinceId!.Value, Name = x.Province!.Name })
            .Distinct().OrderBy(x => x.Name).Select(x => new SalesRequestFilterOption(x.Id, x.Name)).ToListAsync(ct);
        return new SalesRequestFilterOptions(employees, provinces);
    }
}
