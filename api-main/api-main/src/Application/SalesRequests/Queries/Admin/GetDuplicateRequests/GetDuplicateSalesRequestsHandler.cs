using Application.Common.Interfaces;
using Application.Common.Models;
using Application.SalesRequests.Common;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesMatching;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.Admin.GetDuplicateRequests;

public class GetDuplicateSalesRequestsHandler
    : IRequestHandler<GetDuplicateSalesRequestsQuery, Result<PaginatedList<GetDuplicateSalesRequestItem>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDuplicateSalesRequestsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<GetDuplicateSalesRequestItem>>> Handle(
        GetDuplicateSalesRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var self = await _db.SalesRequests.AsNoTracking()
            .Where(x => x.Id == request.RequestId && !x.IsDeleted)
            .Select(x => new { x.Id, x.CustomerName, x.CustomerPhone, x.BranchId })
            .FirstOrDefaultAsync(cancellationToken);

        if (self is null || !branches.Contains(self.BranchId))
            return SalesRequestErrors.NotFound;

        var candidates = await SalesMatchCandidateSelector.LoadPeerRequestCandidatesAsync(
            _db, branches, self.Id, self.CustomerName, self.CustomerPhone, cancellationToken);
        var hitCandidateIds = candidates
            .Where(p =>
                SalesPhoneNormalizer.Matches(self.CustomerPhone, p.Phone)
                || SalesRequestNameSimilarity.IsTripleNameMatch(self.CustomerName, p.Name)
                || SalesRequestNameSimilarity.IsFatherOrGrandfatherMatch(self.CustomerName, p.Name)
                || SalesRequestNameSimilarity.IsSonFatherMatch(self.CustomerName, p.Name))
            .Select(p => p.Id)
            .Distinct()
            .ToList();

        if (hitCandidateIds.Count == 0)
        {
            return new PaginatedList<GetDuplicateSalesRequestItem>(
                [], 0, request.Pagination.PageIndex, request.Pagination.PageSize);
        }

        var peers = await _db.SalesRequests.AsNoTracking()
            .Where(x => hitCandidateIds.Contains(x.Id) && !x.IsDeleted && branches.Contains(x.BranchId))
            .Select(x => new
            {
                x.Id,
                x.SourceType,
                x.Status,
                x.CreatedAt,
                AssignedName = x.TargetEmployee != null ? x.TargetEmployee.FullName : null
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var hits = peers;

        var total = hits.Count;
        var page = hits
            .Skip((request.Pagination.PageIndex - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .Select(p => new GetDuplicateSalesRequestItem
            {
                Id = p.Id,
                SourceType = p.SourceType,
                SourceTypeLabel = SalesRequestSourceLabels.For(p.SourceType),
                StatusLabel = StatusLabel(p.Status),
                CreatedAt = p.CreatedAt,
                AssignedEmployeeName = p.AssignedName
            })
            .ToList();

        return new PaginatedList<GetDuplicateSalesRequestItem>(
            page, total, request.Pagination.PageIndex, request.Pagination.PageSize);
    }

    private static string StatusLabel(SalesRequestStatus status) => status switch
    {
        SalesRequestStatus.New => "جديد",
        SalesRequestStatus.Assigned => "مسند",
        SalesRequestStatus.Viewed => "تمت المشاهدة",
        SalesRequestStatus.Returned => "مُعاد",
        SalesRequestStatus.PreparedForSale => "مُجهّز للبيع",
        SalesRequestStatus.InProgress => "قيد التنفيذ",
        SalesRequestStatus.Inspected => "تم الفحص",
        SalesRequestStatus.Pending => "معلّق",
        SalesRequestStatus.Rejected => "مرفوض",
        SalesRequestStatus.Completed => "مكتمل",
        SalesRequestStatus.LegalFollowup => "متابعة قانونية",
        SalesRequestStatus.ConvertedToSale => "محوّل لمسودة بيع",
        _ => status.ToString()
    };
}
