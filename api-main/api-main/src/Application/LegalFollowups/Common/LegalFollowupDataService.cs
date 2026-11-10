using Application.Common.Interfaces;
using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.LegalFollowups.Common;

public class LegalFollowupDataService
{
    private readonly IApplicationDbContext _db;

    public LegalFollowupDataService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ListLegalFollowupsResponse> ListAsync(
        IReadOnlyList<int> branchScope,
        LegalFollowupStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var requestBranchPairs = await _db.SalesRequests.AsNoTracking()
            .Where(r => branchScope.Contains(r.BranchId) && !r.IsDeleted)
            .Select(r => new { r.Id, r.BranchId })
            .ToListAsync(ct);
        var requestIds = requestBranchPairs.Select(x => x.Id).ToHashSet();
        var requestBranch = requestBranchPairs.ToDictionary(x => x.Id, x => x.BranchId);

        var followerBranchRows = await _db.OrderLists.AsNoTracking()
            .Where(l => branchScope.Contains(l.BranchId) && l.MotabaId > 0)
            .Select(l => new { l.MotabaId, l.BranchId })
            .ToListAsync(ct);
        var followerIdsInScope = followerBranchRows.Select(x => x.MotabaId).Distinct().ToHashSet();
        var followerPrimaryBranch = followerBranchRows
            .GroupBy(x => x.MotabaId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.BranchId).First());

        var query = _db.LegalFollowupCases.AsNoTracking()
            .Where(c =>
                (c.SalesRequestId != null && requestIds.Contains(c.SalesRequestId.Value))
                || (c.SalesRequestId == null && followerIdsInScope.Contains(c.FollowerUserId)));

        if (status is not null)
            query = query.Where(c => c.Status == status.Value);
        else
            query = query.Where(c => c.Status != LegalFollowupStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.CustomerName.Contains(term)
                || (c.CustomerPhone != null && c.CustomerPhone.Contains(term))
                || (c.FollowerName != null && c.FollowerName.Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .ThenByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows.Select(c =>
        {
            int? branchId = null;
            if (c.SalesRequestId is int srId && requestBranch.TryGetValue(srId, out var rb))
                branchId = rb;
            else if (followerPrimaryBranch.TryGetValue(c.FollowerUserId, out var fb))
                branchId = fb;
            return MapRow(c, branchId);
        }).ToList();

        return new ListLegalFollowupsResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            BranchScope = branchScope,
        };
    }

    public async Task<LegalFollowupCaseRowDto?> GetAsync(
        int caseId,
        IReadOnlyList<int> branchScope,
        CancellationToken ct)
    {
        var entity = await _db.LegalFollowupCases.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == caseId, ct);
        if (entity is null)
            return null;

        var branchId = await ResolveBranchIdAsync(entity, ct);
        if (branchId is null || branchScope.Contains(branchId.Value) == false)
            return null;

        return MapRow(entity, branchId);
    }

    public async Task<IReadOnlyList<LegalFollowupFollowerOptionDto>> ListFollowersAsync(
        IReadOnlyList<int> branchScope,
        int? branchId,
        CancellationToken ct)
    {
        var scope = branchId is > 0 && branchScope.Contains(branchId.Value)
            ? (IReadOnlyList<int>)[branchId.Value]
            : branchScope;

        var pairs = await _db.OrderLists.AsNoTracking()
            .Where(l => scope.Contains(l.BranchId) && l.MotabaId > 0)
            .Select(l => new { l.MotabaId, l.BranchId })
            .ToListAsync(ct);

        var ids = pairs.Select(p => p.MotabaId).Distinct().ToList();
        var names = ids.Count == 0
            ? new Dictionary<int, string?>()
            : await _db.Users.AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        return pairs
            .GroupBy(p => p.MotabaId)
            .Select(g =>
            {
                names.TryGetValue(g.Key, out var name);
                return new LegalFollowupFollowerOptionDto
                {
                    FollowerUserId = g.Key,
                    FollowerName = name ?? $"#{g.Key}",
                    BranchId = g.Select(x => x.BranchId).First(),
                };
            })
            .OrderBy(x => x.FollowerName)
            .ToList();
    }

    public async Task<int?> ResolveBranchIdAsync(LegalFollowupCase entity, CancellationToken ct)
    {
        if (entity.SalesRequestId is int srId)
        {
            return await _db.SalesRequests.AsNoTracking()
                .Where(r => r.Id == srId)
                .Select(r => (int?)r.BranchId)
                .FirstOrDefaultAsync(ct);
        }

        return await _db.OrderLists.AsNoTracking()
            .Where(l => l.MotabaId == entity.FollowerUserId)
            .Select(l => (int?)l.BranchId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<LegalFollowupDocumentDto>> ListDocumentsAsync(int caseId, CancellationToken ct)
    {
        var rows = await _db.LegalFollowupCaseDocuments.AsNoTracking()
            .Where(d => d.CaseId == caseId)
            .OrderBy(d => d.Id)
            .ToListAsync(ct);
        return rows.Select(MapDoc).ToList();
    }

    public static LegalFollowupDocumentDto MapDoc(LegalFollowupCaseDocument d) => new()
    {
        Id = d.Id,
        DocumentType = d.DocumentType,
        TypeLabel = LegalFollowupCompletionRules.TypeLabel(d.DocumentType),
        FileName = d.FileName,
        ContentType = d.ContentType,
        SizeBytes = d.SizeBytes,
        UploadedAtUtc = d.UploadedAtUtc,
    };

    private static LegalFollowupCaseRowDto MapRow(LegalFollowupCase c, int? branchId) => new()
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
        CanCancel = LegalFollowupCase.CanCancel(c.Status),
        BranchId = branchId,
        ShopLatitude = c.ShopLatitude,
        ShopLongitude = c.ShopLongitude,
    };
}
