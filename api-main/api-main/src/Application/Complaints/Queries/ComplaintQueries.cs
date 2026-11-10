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
    public int PageSize { get; init; } = 50;
    public bool UnreadOnly { get; init; }
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

        var q = _db.ComplaintMessages.AsNoTracking().AsQueryable();
        if (scope.Count > 0)
            q = q.Where(c => c.BranchId == null || scope.Contains(c.BranchId.Value));
        else if (scope.Count == 0 && _current.HasPermission(SeedPermissions.Complaint.Read))
        {
            // مفوض/مدير بلا فروع: لا بيانات (مطابقة نمط النطاق)
            return new ComplaintListResponse([], 0, 0, page, pageSize);
        }

        var unread = await q.CountAsync(c => c.Status == ComplaintStatus.Unread, ct);
        if (request.UnreadOnly)
            q = q.Where(c => c.Status == ComplaintStatus.Unread);

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
