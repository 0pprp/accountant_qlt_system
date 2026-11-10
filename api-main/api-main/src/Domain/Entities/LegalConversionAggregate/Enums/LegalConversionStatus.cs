namespace Domain.Entities.LegalConversionAggregate.Enums;

public enum LegalConversionStatus : byte
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public static class LegalConversionStatusLabels
{
    public static string Arabic(LegalConversionStatus s) => s switch
    {
        LegalConversionStatus.Pending => "قيد المراجعة",
        LegalConversionStatus.Approved => "معتمد",
        LegalConversionStatus.Rejected => "مرفوض",
        _ => s.ToString()
    };
}
