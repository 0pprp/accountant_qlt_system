using Application.Common.Interfaces;
using Application.Complaints.Common;
using Application.SalesEmployees.Common;
using Application.SalesRequests.Common;
using Domain.Entities.ComplaintAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Complaints.Commands;

public sealed class SubmitComplaintCommand : IRequest<Result<ComplaintDto>>
{
    public required string Body { get; init; }
    public string? Subject { get; init; }
    /// <summary>
    /// "committee" من مسار اللجنة الموثوق؛ "app" من تطبيقات الميدان — يُشتق المصدر على السيرفر.
    /// لا يُقبل sales-employee من جسم العميل مباشرة.
    /// </summary>
    public string SourceApp { get; init; } = "committee";
}

public sealed class MarkComplaintReadCommand : IRequest<Result<ComplaintDto>>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class SubmitComplaintHandler : IRequestHandler<SubmitComplaintCommand, Result<ComplaintDto>>
{
    public const string SourceCommittee = "committee";
    public const string SourceSalesEmployee = "sales-employee";

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public SubmitComplaintHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<ComplaintDto>> Handle(SubmitComplaintCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Complaint.Submit) == false)
            return ComplaintErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return ComplaintErrors.Forbidden;

        var source = await ResolveSourceAppAsync(request.SourceApp, actorId, ct);
        if (source is null)
            return ComplaintErrors.Forbidden;

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId)
            .Select(u => new { u.FullName })
            .FirstOrDefaultAsync(ct);
        var branchIds = _current.BranchIds ?? [];
        int? branchId = branchIds.Count == 1 ? branchIds[0] : branchIds.FirstOrDefault();
        if (branchIds.Count == 0) branchId = null;
        string? branchName = null;
        if (branchId is int bid and > 0)
            branchName = await _db.Branches.AsNoTracking().Where(b => b.Id == bid).Select(b => b.Name).FirstOrDefaultAsync(ct);

        ComplaintMessage entity;
        try
        {
            entity = new ComplaintMessage(
                source,
                actorId,
                user?.FullName ?? "",
                _current.Role ?? "",
                branchId is > 0 ? branchId : null,
                branchName,
                request.Subject ?? "شكوى",
                request.Body,
                _clock.UtcNowOffset);
        }
        catch (InvalidOperationException)
        {
            return ComplaintErrors.Validation;
        }

        _db.ComplaintMessages.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ComplaintMapper.ToDto(entity);
    }

    private async Task<string?> ResolveSourceAppAsync(string? requested, int actorId, CancellationToken ct)
    {
        var raw = (requested ?? "").Trim().ToLowerInvariant();
        // مسار اللجنة الداخلي الموثوق يبقى committee.
        if (raw is SourceCommittee or "follower")
            return SourceCommittee;

        // مسار التطبيق: موظف مبيعات مكلّف فقط → sales-employee (ليس AppNote وليس مندوب جباية).
        if (raw is "app" or SourceSalesEmployee or "")
        {
            var now = _clock.UtcNowOffset;
            if (await SalesWorkforce.HasAnyActiveAssignmentAsync(_db, actorId, now, ct))
                return SourceSalesEmployee;
            return null;
        }

        return null;
    }
}

public sealed class MarkComplaintReadHandler : IRequestHandler<MarkComplaintReadCommand, Result<ComplaintDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public MarkComplaintReadHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<ComplaintDto>> Handle(MarkComplaintReadCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Complaint.MarkRead) == false)
            return ComplaintErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return ComplaintErrors.Forbidden;

        var entity = await _db.ComplaintMessages.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return ComplaintErrors.NotFound;

        var scope = _current.BranchIds ?? [];
        if (scope.Count > 0 && entity.BranchId is int bid && scope.Contains(bid) == false)
            return ComplaintErrors.BranchScope;

        try
        {
            entity.MarkRead(actorId, _clock.UtcNowOffset, request.ExpectedVersion);
        }
        catch (InvalidOperationException)
        {
            return ComplaintErrors.Concurrency;
        }

        await _db.SaveChangesAsync(ct);
        return ComplaintMapper.ToDto(entity);
    }
}
