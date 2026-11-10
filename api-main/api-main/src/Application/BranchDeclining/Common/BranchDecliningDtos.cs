namespace Application.BranchDeclining.Common;

public sealed record BranchDecliningRowDto(
    int CustomerId,
    string CustomerName,
    string? PhoneNumber,
    int BranchId,
    string FromLabel,
    string ToLabel,
    int ToScore,
    DateOnly PriorExcellentDate,
    DateOnly CurrentCaptureDate,
    int DaysBetween);

public sealed record BranchDecliningResponse(
    IReadOnlyList<BranchDecliningRowDto> Items,
    int TotalCount,
    bool HistoryInsufficient,
    int Page,
    int PageSize);

public static class BranchDecliningErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "BranchDeclining.Forbidden");
    public static readonly Error BranchScope = new("خارج نطاق فرعك.", "BranchDeclining.BranchScope");
}
