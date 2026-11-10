using Domain.Entities.TutorialVideoAggregate.Enums;

namespace Application.TutorialVideos.Common;

public sealed record TutorialVideoDto(
    int Id,
    string AppKey,
    string AppKeyLabel,
    string QuestionAr,
    string VideoUrl,
    bool IsEnabled,
    int SortOrder,
    string? UpdatedByName,
    DateTimeOffset UpdatedAtUtc,
    int ExpectedVersion);

public sealed record TutorialVideoListResponse(
    IReadOnlyList<TutorialVideoDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record TutorialPublicDto(
    int Id,
    string AppKey,
    string QuestionAr,
    string VideoUrl,
    int SortOrder);

public sealed record TutorialAppOptionDto(string AppKey, string Label);

public static class TutorialErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "Tutorial.Forbidden");
    public static readonly Error NotFound = new("الفيديو غير موجود.", "Tutorial.NotFound");
    public static readonly Error Validation = new("بيانات غير صالحة.", "Tutorial.Validation");
    public static readonly Error Concurrency = new("تم تعديل السجل من جهة أخرى.", "Tutorial.Concurrency");
    public static readonly Error InvalidAppKey = new("جمهور التطبيق غير صالح.", "Tutorial.InvalidAppKey");
    public static readonly Error UrlNotAllowed = new(
        "رابط الفيديو يجب أن يكون https لمضيف مدعوم (YouTube / Vimeo).",
        "Tutorial.UrlNotAllowed");
    public static readonly Error AudienceDenied = new(
        "الفيديو خارج نطاق تطبيقك.",
        "Tutorial.AudienceDenied");
}

public static class TutorialMapper
{
    public static TutorialVideoDto ToAdmin(Domain.Entities.TutorialVideoAggregate.TutorialVideo v) =>
        new(v.Id, TutorialAppKeyLabels.Wire(v.AppKey), TutorialAppKeyLabels.Arabic(v.AppKey),
            v.QuestionAr, v.VideoUrl, v.IsEnabled, v.SortOrder, v.UpdatedByName, v.UpdatedAtUtc, v.ConcurrencyStamp);

    public static TutorialPublicDto ToPublic(Domain.Entities.TutorialVideoAggregate.TutorialVideo v) =>
        new(v.Id, TutorialAppKeyLabels.Wire(v.AppKey), v.QuestionAr, v.VideoUrl, v.SortOrder);
}
