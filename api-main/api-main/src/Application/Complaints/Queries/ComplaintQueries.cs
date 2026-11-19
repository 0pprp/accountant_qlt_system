using Application.Common.Interfaces;
using Application.Complaints.Common;
using Application.SalesRequests.Common;
using Domain.Entities.ComplaintAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Complaints.Queries;

public sealed class ListComplaintsQuery : IRequest<Result<ComplaintListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool UnreadOnly { get; init; }
    public string? Search { get; init; }
    public int? BranchId { get; init; }
}

public sealed class ListComplaintsHandler : IRequestHandler<ListComplaintsQuery, Result<ComplaintListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListComplaintsHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<ComplaintListResponse>> Handle(ListComplaintsQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Complaint.Read) == false)
            return ComplaintErrors.Forbidden;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var scope = _current.BranchIds ?? [];

        if (scope.Count == 0)
            return new ComplaintListResponse([], 0, 0, page, pageSize);

        var q = ComplaintScope.Apply(_db.ComplaintMessages.AsNoTracking(), scope, request.BranchId);

        var unread = await q.CountAsync(c => c.Status == ComplaintStatus.Unread, ct);
        if (request.UnreadOnly)
            q = q.Where(c => c.Status == ComplaintStatus.Unread);

        var term = (request.Search ?? "").Trim().ToLower();
        if (term.Length > ComplaintScope.MaxSearchLength)
            term = term[..ComplaintScope.MaxSearchLength];
        if (term.Length > 0)
            q = q.Where(c =>
                c.Subject.ToLower().Contains(term)
                || c.Body.ToLower().Contains(term)
                || c.SenderDisplayName.ToLower().Contains(term));

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(c => c.CreatedAtUtc)
            .ThenByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new ComplaintListResponse(items.Select(ComplaintMapper.ToDto).ToList(), total, unread, page, pageSize);
    }
}
