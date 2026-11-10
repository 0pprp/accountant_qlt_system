using Domain.Entities.BranchVisitAggregate;

namespace Application.BranchVisits.Common;

public sealed record BranchVisitRatingDto(int OrderListId, string ListName, int Score, DateTimeOffset RatedAtUtc);

public sealed record BranchVisitMemberDto(int UserId, string DisplayName);

public sealed record BranchVisitAssignedTaskDto(
    int Id,
    string Title,
    string? Description,
    string Status,
    DateTimeOffset? DueAtUtc,
    string? Priority);

public sealed record BranchVisitAttachmentDto(
    int Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UploadedAtUtc);

public sealed record BranchVisitDto(
    int Id,
    int BranchId,
    int OpenedByUserId,
    string OpenedByName,
    string Status,
    DateTimeOffset ArrivedAtUtc,
    DateTimeOffset? DepartedAtUtc,
    string? DepartNote,
    int ExpectedVersion,
    IReadOnlyList<BranchVisitRatingDto> Ratings,
    IReadOnlyList<BranchVisitMemberDto> Members,
    IReadOnlyList<BranchVisitAssignedTaskDto> AssignedTasks,
    IReadOnlyList<BranchVisitAttachmentDto> Attachments);

public sealed record BranchVisitListResponse(
    IReadOnlyList<BranchVisitDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public static class BranchVisitErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "BranchVisit.Forbidden");
    public static readonly Error NotFound = new("الزيارة غير موجودة.", "BranchVisit.NotFound");
    public static readonly Error BranchScope = new("خارج نطاق فرعك.", "BranchVisit.BranchScope");
    public static readonly Error AlreadyOpen = new("لديك زيارة مفتوحة بالفعل.", "BranchVisit.AlreadyOpen");
    public static readonly Error Validation = new("بيانات الزيارة غير صالحة.", "BranchVisit.Validation");
    public static readonly Error Concurrency = new("تم تعديل الزيارة من جهة أخرى.", "BranchVisit.Concurrency");
    public static readonly Error AttachmentInvalid = new("مرفق غير مقبول.", "BranchVisit.AttachmentInvalid");
    public static readonly Error AttachmentFailed = new("تعذر حفظ المرفق — لم تُغلق الزيارة.", "BranchVisit.AttachmentFailed");
}

public static class BranchVisitMapper
{
    public static BranchVisitDto ToDto(
        BranchVisit v,
        IReadOnlyList<BranchVisitAssignedTaskDto>? assignedTasks = null) =>
        new(v.Id, v.BranchId, v.OpenedByUserId, v.OpenedByName, v.Status.ToString(),
            v.ArrivedAtUtc, v.DepartedAtUtc, v.DepartNote, v.ConcurrencyStamp,
            v.Ratings.Select(r => new BranchVisitRatingDto(r.OrderListId, r.ListName, r.Score, r.RatedAtUtc)).ToList(),
            v.Members.Select(m => new BranchVisitMemberDto(m.UserId, m.DisplayName)).ToList(),
            assignedTasks ?? [],
            v.Attachments.Select(a => new BranchVisitAttachmentDto(
                a.Id, a.OriginalFileName, a.ContentType, a.SizeBytes, a.UploadedAtUtc)).ToList());
}
