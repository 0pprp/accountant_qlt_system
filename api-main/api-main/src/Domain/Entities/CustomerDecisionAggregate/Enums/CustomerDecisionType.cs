namespace Domain.Entities.CustomerDecisionAggregate.Enums;

/// <summary>أنواع قرار إدارة الفرع — مطابقة المصدر CustomerWeekDecisions.DecisionType.</summary>
public enum CustomerDecisionType : byte
{
    Continue = 1,       // متواصل
    FakeSale = 2,       // وهمي
    LegalRequest = 3,   // طلب قانونية
    Legal = 4,          // قانونية (بعد اعتماد المراقب)
    WaiverRequest = 5,  // طلب تنازل
    Waiver = 6          // تنازل (بعد تطبيق المحاسب)
}

public static class CustomerDecisionTypeLabels
{
    public static string Arabic(CustomerDecisionType t) => t switch
    {
        CustomerDecisionType.Continue => "متواصل",
        CustomerDecisionType.FakeSale => "وهمي",
        CustomerDecisionType.LegalRequest => "طلب قانونية",
        CustomerDecisionType.Legal => "قانونية",
        CustomerDecisionType.WaiverRequest => "طلب تنازل",
        CustomerDecisionType.Waiver => "تنازل",
        _ => t.ToString()
    };

    public static bool TryParseArabic(string? raw, out CustomerDecisionType type)
    {
        type = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var s = raw.Trim();
        type = s switch
        {
            "متواصل" => CustomerDecisionType.Continue,
            "وهمي" => CustomerDecisionType.FakeSale,
            "طلب قانونية" or "قانونية-طلب" => CustomerDecisionType.LegalRequest,
            "قانونية" => CustomerDecisionType.Legal,
            "طلب تنازل" => CustomerDecisionType.WaiverRequest,
            "تنازل" => CustomerDecisionType.Waiver,
            _ => default
        };
        return type != default || s is "متواصل";
    }
}
