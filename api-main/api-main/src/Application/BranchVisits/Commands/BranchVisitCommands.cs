using System.Security.Cryptography;
using Application.BranchVisits.Common;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;
using Application.SalesRequests.Common;
using Domain.Entities.BranchVisitAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.BranchVisits.Commands;

public sealed class OpenBranchVisitCommand : IRequest<Result<BranchVisitDto>>
{
    public int BranchId { get; init; }
}

public sealed class UpsertBranchVisitRatingCommand : IRequest<Result<BranchVisitDto>>
{
    public int VisitId { get; init; }
    public int OrderListId { get; init; }
    public int Score { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class DepartBranchVisitCommand : IRequest<Result<BranchVisitDto>>
{
    public int VisitId { get; init; }
    public required string Note { get; init; }
    public int ExpectedVersion { get; init; }
    public IReadOnlyList<IFormFile>? Files { get; init; }
}

public static class BranchVisitSupport
{
    public static async Task<IReadOnlyList<BranchVisitAssignedTaskDto>> LoadAssignedTasksAsync(
        IApplicationDbContext db, int actorId, int branchId, CancellationToken ct)
    {
        try
        {
            var rows = await db.ManagementTasks.AsNoTracking()
                .Where(t =>
                    t.BranchId == branchId
                    && (t.AssigneeUserId == actorId || t.CurrentHolderUserId == actorId)
                    && t.Status != ManagementTaskStatus.Completed)
                .OrderByDescending(t => t.Id)
                .Take(50)
                .Select(t => new BranchVisitAssignedTaskDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status.ToString(),
                    t.DueAtUtc,
                    t.Priority.ToString()))
                .ToListAsync(ct);
            return rows;
        }
        catch
        {
            return [];
        }
    }

    public static async Task<List<(int UserId, string DisplayName)>> LoadMemberCandidatesAsync(
        IApplicationDbContext db, int branchId, CancellationToken ct)
    {
        // مصدر: أعضاء إدارة الفرع النشطون المعيَّنون للمدينة/الفرع — لقطة عند الفتح
        return await db.Users.AsNoTracking()
            .Where(u =>
                u.UserBranches.Any(ub => ub.BranchId == branchId)
                && u.UserRoles.Any(ur => ur.Role!.Name == SeedRoles.BranchManager.Name))
            .OrderBy(u => u.FullName)
            .Select(u => new ValueTuple<int, string>(u.Id, u.FullName))
            .ToListAsync(ct);
    }
}

public sealed class OpenBranchVisitHandler : IRequestHandler<OpenBranchVisitCommand, Result<BranchVisitDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public OpenBranchVisitHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<BranchVisitDto>> Handle(OpenBranchVisitCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchVisit.Open) == false)
            return BranchVisitErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return BranchVisitErrors.Forbidden;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0 || scope.Contains(request.BranchId) == false)
            return BranchVisitErrors.BranchScope;

        var openVisit = await _db.BranchVisits
            .Include(v => v.Ratings)
            .Include(v => v.Members)
            .Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.OpenedByUserId == actorId && v.Status == BranchVisitStatus.Open, ct);
        if (openVisit is not null)
        {
            if (openVisit.BranchId != request.BranchId)
                return BranchVisitErrors.AlreadyOpen;
            var tasksSame = await BranchVisitSupport.LoadAssignedTasksAsync(_db, actorId, request.BranchId, ct);
            return BranchVisitMapper.ToDto(openVisit, tasksSame);
        }

        var name = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "";
        var visit = new BranchVisit(request.BranchId, actorId, name, _clock.UtcNowOffset);
        var members = await BranchVisitSupport.LoadMemberCandidatesAsync(_db, request.BranchId, ct);
        visit.SetMemberSnapshot(members);
        _db.BranchVisits.Add(visit);
        await _db.SaveChangesAsync(ct);

        var tasks = await BranchVisitSupport.LoadAssignedTasksAsync(_db, actorId, request.BranchId, ct);
        return BranchVisitMapper.ToDto(visit, tasks);
    }
}

public sealed class UpsertBranchVisitRatingHandler
    : IRequestHandler<UpsertBranchVisitRatingCommand, Result<BranchVisitDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public UpsertBranchVisitRatingHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<BranchVisitDto>> Handle(UpsertBranchVisitRatingCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchVisit.Rate) == false)
            return BranchVisitErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return BranchVisitErrors.Forbidden;

        var visit = await _db.BranchVisits
            .Include(v => v.Ratings).Include(v => v.Members).Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.Id == request.VisitId, ct);
        if (visit is null)
            return BranchVisitErrors.NotFound;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0 || scope.Contains(visit.BranchId) == false)
            return BranchVisitErrors.BranchScope;
        if (visit.OpenedByUserId != actorId)
            return BranchVisitErrors.Forbidden;

        var list = await _db.OrderLists.AsNoTracking()
            .Where(l => l.Id == request.OrderListId && l.BranchId == visit.BranchId)
            .Select(l => new { l.Id, l.Name })
            .FirstOrDefaultAsync(ct);
        if (list is null)
            return BranchVisitErrors.Validation;

        try
        {
            visit.UpsertRating(list.Id, list.Name ?? $"قائمة {list.Id}", request.Score, actorId,
                _clock.UtcNowOffset, request.ExpectedVersion);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return BranchVisitErrors.Concurrency;
        }
        catch (InvalidOperationException)
        {
            return BranchVisitErrors.Validation;
        }

        await _db.SaveChangesAsync(ct);
        return BranchVisitMapper.ToDto(visit);
    }
}

public sealed class DepartBranchVisitHandler : IRequestHandler<DepartBranchVisitCommand, Result<BranchVisitDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IFileManager _files;

    public DepartBranchVisitHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock, IFileManager files)
    {
        _db = db;
        _current = current;
        _clock = clock;
        _files = files;
    }

    public async Task<Result<BranchVisitDto>> Handle(DepartBranchVisitCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchVisit.Depart) == false)
            return BranchVisitErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return BranchVisitErrors.Forbidden;

        var visit = await _db.BranchVisits
            .Include(v => v.Ratings).Include(v => v.Members).Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.Id == request.VisitId, ct);
        if (visit is null)
            return BranchVisitErrors.NotFound;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0 || scope.Contains(visit.BranchId) == false)
            return BranchVisitErrors.BranchScope;

        if (visit.Status == BranchVisitStatus.Departed && visit.OpenedByUserId == actorId)
            return BranchVisitMapper.ToDto(visit);

        var files = (request.Files ?? [])
            .Where(f => f is { Length: > 0 })
            .Take(BranchVisitFileRules.MaxFilesPerDepart)
            .ToList();

        var savedTemps = new List<string>();
        try
        {
            foreach (var file in files)
            {
                if (file.Length > BranchVisitFileRules.MaxBytes)
                    return BranchVisitErrors.AttachmentInvalid;
                var ext = Path.GetExtension(file.FileName ?? "");
                if (BranchVisitFileRules.AllowedExtensions.Contains(ext) == false)
                    return BranchVisitErrors.AttachmentInvalid;

                await using var stream = file.OpenReadStream();
                var header = new byte[16];
                var read = await stream.ReadAsync(header.AsMemory(0, header.Length), ct);
                if (BranchVisitFileRules.TryDetectKind(header.AsSpan(0, read), ext, out var detectedType) == false)
                    return BranchVisitErrors.AttachmentInvalid;
                stream.Position = 0;

                string sha;
                using (var shaAlg = SHA256.Create())
                {
                    var hash = await shaAlg.ComputeHashAsync(stream, ct);
                    sha = Convert.ToHexString(hash).ToLowerInvariant();
                }
                stream.Position = 0;

                if (visit.Attachments.Any(a => a.ContentSha256 == sha))
                    continue;

                var saved = await _files.SaveFileAsync(
                    file,
                    Path.Combine(AttachmentPath.Root, AttachmentPath.BranchVisitsSubfolder(visit.Id)),
                    ct);
                savedTemps.Add(saved.RelativePath);
                visit.AddAttachment(
                    Path.GetFileName(file.FileName) ?? saved.Name,
                    saved.Name,
                    saved.RelativePath,
                    detectedType,
                    saved.Extension,
                    file.Length,
                    sha,
                    actorId,
                    _clock.UtcNowOffset);
            }

            var cutoff = _clock.UtcNowOffset.AddDays(-DelegateFollowupRules.MaxDaysListActivity);
            var required = await (
                from l in _db.OrderLists.AsNoTracking()
                where l.BranchId == visit.BranchId
                join o in _db.Orders.AsNoTracking() on l.Id equals o.OrderListId
                join p in _db.InstallmentPayments.AsNoTracking() on o.Id equals p.OrderId
                where p.Date >= DateOnly.FromDateTime(cutoff.UtcDateTime)
                select l.Id).Distinct().ToListAsync(ct);

            try
            {
                visit.Depart(request.Note, required, actorId, _clock.UtcNowOffset, request.ExpectedVersion);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
            {
                CleanupTemps(savedTemps);
                return BranchVisitErrors.Concurrency;
            }
            catch (InvalidOperationException)
            {
                CleanupTemps(savedTemps);
                return BranchVisitErrors.Validation;
            }

            await _db.SaveChangesAsync(ct);
            return BranchVisitMapper.ToDto(visit);
        }
        catch
        {
            CleanupTemps(savedTemps);
            return BranchVisitErrors.AttachmentFailed;
        }
    }

    private void CleanupTemps(List<string> paths)
    {
        foreach (var p in paths)
        {
            try { _files.Delete(p); } catch { /* best-effort */ }
        }
    }
}
