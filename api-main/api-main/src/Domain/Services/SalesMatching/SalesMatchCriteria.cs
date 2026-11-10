namespace Domain.Services.SalesMatching;

public static class SalesMatchCriteria
{
    public const string TripleName = "tripleName";
    public const string Phone = "phone";
    public const string FatherGrandfather = "fatherGrandfather";
    public const string SonFather = "sonFather";

    public static readonly string[] All =
    [
        TripleName,
        Phone,
        FatherGrandfather,
        SonFather
    ];
}

public enum SalesMatchCardState
{
    Loading = 0,
    Results = 1,
    NoResults = 2,
    InsufficientData = 3,
    Failed = 4,
}
