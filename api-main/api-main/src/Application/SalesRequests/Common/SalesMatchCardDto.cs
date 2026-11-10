using Domain.Services.SalesMatching;

namespace Application.SalesRequests.Common;

public record SalesMatchCardDto
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public required string State { get; init; }
    public int? Count { get; init; }
}

public record SalesMatchSummaryItemDto
{
    public int RequestId { get; init; }
    public required List<SalesMatchCardDto> Cards { get; init; }
}

public static class SalesMatchCardLabels
{
    public static string For(string key) => key switch
    {
        SalesMatchCriteria.TripleName => "تشابه الاسم الثلاثي",
        SalesMatchCriteria.Phone => "تشابه رقم الهاتف",
        SalesMatchCriteria.FatherGrandfather => "تشابه اسم الأب والجد",
        SalesMatchCriteria.SonFather => "تطابق ابن مع أب",
        _ => key
    };

    public static string StateName(SalesMatchCardState state) => state switch
    {
        SalesMatchCardState.Loading => "loading",
        SalesMatchCardState.Results => "results",
        SalesMatchCardState.NoResults => "noResults",
        SalesMatchCardState.InsufficientData => "insufficientData",
        SalesMatchCardState.Failed => "failed",
        _ => "failed"
    };
}
