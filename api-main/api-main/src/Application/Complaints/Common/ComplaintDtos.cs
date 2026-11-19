using Domain.Entities.ComplaintAggregate;

namespace Application.Complaints.Common;

public sealed record ComplaintDto(
    int Id,
    string SourceApp,
    int SenderUserId,
    string SenderDisplayName,
    string SenderRole,
    int? BranchId,
    string? BranchName,
    string Subject,
    string Body,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc,
    bool IsUnread,
    int ExpectedVersion);

public sealed record ComplaintListResponse(
    IReadOnlyList<ComplaintDto> Items,
    int TotalCount,
    int UnreadCount,
    int Page,
    int PageSize);

public static class ComplaintErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "Complaint.Forbidden");
    public static readonly Error NotFound = new("الشكوى غير موجودة.", "Complaint.NotFound");
    public static readonly Error Validation = new("بيانات الشكوى غير صالحة.", "Complaint.Validation");
    public static readonly Error Concurrency = new("تم تعديل السجل من جهة أخرى.", "Complaint.Concurrency");
    public static readonly Error InvalidBranch = new("رقم الفرع غير صالح.", "Complaint.InvalidBranch");
    public static readonly Error BranchScope = new("خارج نطاق فرعك.", "Complaint.BranchScope");
}

public sealed record MarkAllComplaintsReadResponse(int Updated);

public static class ComplaintScope
{
    public const int MaxSearchLength = 100;

    public static IQueryable<ComplaintMessage> Apply(
        IQueryable<ComplaintMessage> source,
        IReadOnlyCollection<int> scope,
        int? branchId)
    {
        var ids = scope.ToList();
        if (ids.Count == 0)
            return source.Where(_ => false);
        if (branchId is int bid)
        {
            if (ids.Contains(bid) == false)
                return source.Where(_ => false);
            return source.Where(c => c.BranchId == bid);
        }
        return source.Where(c => c.BranchId == null || ids.Contains(c.BranchId.Value));
    }

    public static bool Contains(IReadOnlyCollection<int> scope, int? branchId) =>
        scope.Count > 0 && (branchId is not int bid || scope.Contains(bid));
}

public static class ComplaintMapper
{
    public static ComplaintDto ToDto(ComplaintMessage c) =>
        new(c.Id, c.SourceApp, c.SenderUserId, c.SenderDisplayName, c.SenderRole,
            c.BranchId, c.BranchName, c.Subject, c.Body, c.Status.ToString(),
            c.CreatedAtUtc, c.ReadAtUtc, c.Status == ComplaintStatus.Unread, c.ConcurrencyStamp);
}
