using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public class FollowerDelegateNotesDataService
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly FollowerSalesReviewDataService _scope;

    public FollowerDelegateNotesDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        FollowerSalesReviewDataService scope)
    {
        _db = db;
        _clock = clock;
        _scope = scope;
    }

    public async Task<Result<FollowerDelegateNotesResponse>> GetAsync(
        int followerId,
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var scope = await _scope.ResolveFollowerScopeAsync(followerId, authorizedBranches, branchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;

        var (_, followerName, listIds, _) = scope.Data!;
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        // ملاحظات كتبها المتابع أو على قوائم تكليفه الحالي — ليست CustomerNotes
        var query = _db.FollowerDelegateNotes.AsNoTracking()
            .Where(n => n.CreatedByUserId == followerId || listIds.Contains(n.OrderListId));

        var totalCount = await query.CountAsync(ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var rows = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .ThenByDescending(n => n.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.OrderListId,
                n.MandobUserId,
                n.NoteText,
                n.CreatedByUserId,
                n.CreatedByName,
                n.CreatedByRole,
                n.CreatedAtUtc
            })
            .ToListAsync(ct);

        var lids = rows.Select(r => r.OrderListId).Distinct().ToList();
        var mandobIds = rows.Select(r => r.MandobUserId).Distinct().ToList();

        var lists = await _db.OrderLists.AsNoTracking()
            .Where(l => lids.Contains(l.Id))
            .Select(l => new { l.Id, l.Name })
            .ToDictionaryAsync(l => l.Id, ct);

        var users = await _db.Users.AsNoTracking()
            .Where(u => mandobIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, ct);

        var items = rows.Select(n =>
        {
            lists.TryGetValue(n.OrderListId, out var list);
            users.TryGetValue(n.MandobUserId, out var mandob);
            return new FollowerDelegateNoteRowDto(
                n.Id,
                n.OrderListId,
                list?.Name,
                n.MandobUserId,
                mandob?.FullName,
                n.NoteText,
                n.CreatedByUserId,
                n.CreatedByName,
                n.CreatedByRole,
                n.CreatedAtUtc,
                "follower-delegate-note");
        }).ToList();

        return new FollowerDelegateNotesResponse(
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            FollowerWorkRules.TimeZoneId,
            _clock.UtcNowOffset,
            followerId,
            followerName);
    }

    public async Task<Result<FollowerDelegateNoteRowDto>> AddForMotabaAsync(
        int motabaUserId,
        string? motabaName,
        int listId,
        string noteText,
        CancellationToken ct)
    {
        var text = (noteText ?? "").Trim();
        if (string.IsNullOrWhiteSpace(text))
            return new Error("نص الملاحظة مطلوب.", "FollowerDelegateNotes.Empty");
        if (text.Length > FollowerDelegateNote.MaxTextLength)
            return new Error($"الحد الأقصى للملاحظة {FollowerDelegateNote.MaxTextLength} حرف.", "FollowerDelegateNotes.TooLong");

        var list = await _db.OrderLists.AsNoTracking()
            .Where(l => l.Id == listId && l.MotabaId == motabaUserId)
            .Select(l => new { l.Id, l.Name, l.MandobId, MandobName = l.Mandob != null ? l.Mandob.FullName : null })
            .FirstOrDefaultAsync(ct);
        if (list is null)
            return FollowerWorkErrors.DelegateNotesListOutOfScope;

        var note = new FollowerDelegateNote(
            list.Id,
            list.MandobId,
            text,
            motabaUserId,
            string.IsNullOrWhiteSpace(motabaName) ? "متابع" : motabaName.Trim(),
            _clock.UtcNowOffset);
        _db.FollowerDelegateNotes.Add(note);
        await _db.SaveChangesAsync(ct);

        return new FollowerDelegateNoteRowDto(
            note.Id,
            note.OrderListId,
            list.Name,
            note.MandobUserId,
            list.MandobName,
            note.NoteText,
            note.CreatedByUserId,
            note.CreatedByName,
            note.CreatedByRole,
            note.CreatedAtUtc,
            "follower-delegate-note");
    }
}
