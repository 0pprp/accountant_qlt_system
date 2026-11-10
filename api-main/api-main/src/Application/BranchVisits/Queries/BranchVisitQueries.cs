using Application.BranchVisits.Commands;
using Application.BranchVisits.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.BranchVisitAggregate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.BranchVisits.Queries;

public sealed class GetActiveBranchVisitQuery : IRequest<Result<BranchVisitDto?>>;

public sealed class ListBranchVisitsQuery : IRequest<Result<BranchVisitListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public sealed class GetBranchVisitAttachmentQuery : IRequest<Result<(byte[] Bytes, string ContentType, string FileName)>>
{
    public int VisitId { get; init; }
    public int AttachmentId { get; init; }
}

public sealed class GetActiveBranchVisitHandler : IRequestHandler<GetActiveBranchVisitQuery, Result<BranchVisitDto?>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetActiveBranchVisitHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<BranchVisitDto?>> Handle(GetActiveBranchVisitQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchVisit.Read) == false
            && _current.HasPermission(SeedPermissions.BranchVisit.Open) == false)
            return BranchVisitErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return BranchVisitErrors.Forbidden;

        var visit = await _db.BranchVisits.AsNoTracking()
            .Include(v => v.Ratings).Include(v => v.Members).Include(v => v.Attachments)
            .Where(v => v.OpenedByUserId == actorId && v.Status == BranchVisitStatus.Open)
            .OrderByDescending(v => v.ArrivedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (visit is null)
            return (BranchVisitDto?)null;

        var tasks = await BranchVisitSupport.LoadAssignedTasksAsync(_db, actorId, visit.BranchId, ct);
        return BranchVisitMapper.ToDto(visit, tasks);
    }
}

public sealed class ListBranchVisitsHandler : IRequestHandler<ListBranchVisitsQuery, Result<BranchVisitListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListBranchVisitsHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<BranchVisitListResponse>> Handle(ListBranchVisitsQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchVisit.Read) == false)
            return BranchVisitErrors.Forbidden;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return new BranchVisitListResponse([], 0, page, pageSize);

        var q = _db.BranchVisits.AsNoTracking()
            .Include(v => v.Ratings).Include(v => v.Members).Include(v => v.Attachments)
            .Where(v => scope.Contains(v.BranchId));

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(v => v.ArrivedAtUtc)
            .ThenByDescending(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new BranchVisitListResponse(items.Select(v => BranchVisitMapper.ToDto(v)).ToList(), total, page, pageSize);
    }
}

public sealed class GetBranchVisitAttachmentHandler
    : IRequestHandler<GetBranchVisitAttachmentQuery, Result<(byte[] Bytes, string ContentType, string FileName)>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IWebHostEnvironment _env;

    public GetBranchVisitAttachmentHandler(
        IApplicationDbContext db, ICurrentUserService current, IWebHostEnvironment env)
    {
        _db = db;
        _current = current;
        _env = env;
    }

    public async Task<Result<(byte[] Bytes, string ContentType, string FileName)>> Handle(
        GetBranchVisitAttachmentQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchVisit.Read) == false)
            return BranchVisitErrors.Forbidden;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return BranchVisitErrors.BranchScope;

        var visit = await _db.BranchVisits.AsNoTracking()
            .Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.Id == request.VisitId, ct);
        if (visit is null || scope.Contains(visit.BranchId) == false)
            return BranchVisitErrors.NotFound;

        var att = visit.Attachments.FirstOrDefault(a => a.Id == request.AttachmentId);
        if (att is null)
            return BranchVisitErrors.NotFound;

        var full = Path.Combine(_env.WebRootPath, att.RelativePath);
        if (File.Exists(full) == false)
            return BranchVisitErrors.NotFound;

        var bytes = await File.ReadAllBytesAsync(full, ct);
        return (bytes, att.ContentType, att.OriginalFileName);
    }
}
