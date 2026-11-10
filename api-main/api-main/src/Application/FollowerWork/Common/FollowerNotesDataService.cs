using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public class FollowerNotesDataService
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly FollowerSalesReviewDataService _scope;

    public FollowerNotesDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        FollowerSalesReviewDataService scope)
    {
        _db = db;
        _clock = clock;
        _scope = scope;
    }

    public async Task<Result<FollowerNotesResponse>> GetNotesAsync(
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

        // زبائن على قوائم التكليف (عبر طلبات معتمدة) + ملاحظات مكتوبة مباشرة على القائمة
        var customerIdsOnLists = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderListId != null && listIds.Contains(o.OrderListId.Value))
            .Select(o => o.CustomerId)
            .Distinct()
            .ToListAsync(ct);

        var query = _db.CustomerNotes.AsNoTracking()
            .Where(n =>
                (n.OrderListId != null && listIds.Contains(n.OrderListId.Value))
                || customerIdsOnLists.Contains(n.CustomerId));

        var totalCount = await query.CountAsync(ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var noteRows = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .ThenByDescending(n => n.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.CustomerId,
                n.OrderListId,
                n.NoteText,
                n.AuthorName,
                n.AuthorUserId,
                n.CreatedAtUtc
            })
            .ToListAsync(ct);

        var cids = noteRows.Select(n => n.CustomerId).Distinct().ToList();
        var lids = noteRows.Where(n => n.OrderListId != null).Select(n => n.OrderListId!.Value).Distinct().ToList();

        var customers = await _db.Customers.AsNoTracking()
            .Where(c => cids.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber })
            .ToDictionaryAsync(c => c.Id, ct);

        var lists = await _db.OrderLists.AsNoTracking()
            .Where(l => lids.Contains(l.Id) || listIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        var items = noteRows.Select(n =>
        {
            customers.TryGetValue(n.CustomerId, out var cust);
            string? listName = null;
            if (n.OrderListId is int lid)
                lists.TryGetValue(lid, out listName);
            return new FollowerNoteRowDto(
                n.Id,
                n.CustomerId,
                cust?.FullName ?? $"زبون {n.CustomerId}",
                cust?.PhoneNumber,
                n.OrderListId,
                listName,
                n.NoteText,
                n.AuthorName,
                n.AuthorUserId,
                n.CreatedAtUtc,
                "customer");
        }).ToList();

        return new FollowerNotesResponse(
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

    public async Task<Result<FollowerNoteRowDto>> AddNoteForMotabaAsync(
        int motabaUserId,
        string? motabaName,
        int customerId,
        int listId,
        string noteText,
        CancellationToken ct)
    {
        var text = (noteText ?? "").Trim();
        if (string.IsNullOrWhiteSpace(text))
            return new Error("نص الملاحظة مطلوب.", "FollowerNotes.Empty");
        if (text.Length > CustomerNote.MaxTextLength)
            return new Error($"الحد الأقصى للملاحظة {CustomerNote.MaxTextLength} حرف.", "FollowerNotes.TooLong");

        var list = await _db.OrderLists.AsNoTracking()
            .Where(l => l.Id == listId && l.MotabaId == motabaUserId)
            .Select(l => new { l.Id, l.Name, l.BranchId })
            .FirstOrDefaultAsync(ct);
        if (list is null)
            return FollowerWorkErrors.FollowerBranchMismatch;

        var customerExists = await _db.Customers.AsNoTracking().AnyAsync(c => c.Id == customerId, ct);
        if (customerExists == false)
            return new Error("الزبون غير موجود.", "FollowerNotes.CustomerNotFound");

        var onList = await _db.Orders.AsNoTracking()
            .AnyAsync(o => o.CustomerId == customerId && o.OrderListId == listId, ct);
        if (onList == false)
            return new Error("الزبون خارج نطاق قائمة المتابع.", "FollowerNotes.CustomerOutOfScope");

        var note = new CustomerNote(
            customerId,
            listId,
            list.BranchId,
            text,
            motabaUserId,
            string.IsNullOrWhiteSpace(motabaName) ? "متابع" : motabaName.Trim(),
            _clock.UtcNowOffset);
        _db.CustomerNotes.Add(note);
        await _db.SaveChangesAsync(ct);

        return new FollowerNoteRowDto(
            note.Id,
            customerId,
            (await _db.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => c.FullName).FirstAsync(ct)),
            await _db.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => c.PhoneNumber).FirstAsync(ct),
            listId,
            list.Name,
            note.NoteText,
            note.AuthorName,
            note.AuthorUserId,
            note.CreatedAtUtc,
            "customer");
    }

    /// <summary>
    /// HasNote للمتدنية: أي ملاحظة CustomerNotes على الزبون (مطابقة LoadLatestNotesAsync).
    /// </summary>
    public async Task<Dictionary<int, DateTimeOffset?>> LoadLatestNoteTimesAsync(
        IReadOnlyList<int> customerIds,
        CancellationToken ct)
    {
        var map = new Dictionary<int, DateTimeOffset?>();
        if (customerIds.Count == 0)
            return map;

        var rows = await _db.CustomerNotes.AsNoTracking()
            .Where(n => customerIds.Contains(n.CustomerId))
            .GroupBy(n => n.CustomerId)
            .Select(g => new { CustomerId = g.Key, At = g.Max(x => (DateTimeOffset?)x.CreatedAtUtc) })
            .ToListAsync(ct);

        foreach (var r in rows)
            map[r.CustomerId] = r.At;
        return map;
    }
}
