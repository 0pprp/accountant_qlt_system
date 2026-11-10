namespace Application.ManagementRatings.Common;

public enum ManagementRatingsScope
{
    Branch = 1,
    List = 2,
    Follower = 3,
    Employee = 4,
}

public static class ManagementRatingsScopeParser
{
    public static bool TryParse(string? value, out ManagementRatingsScope scope)
    {
        scope = ManagementRatingsScope.Branch;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        switch (value.Trim().ToLowerInvariant())
        {
            case "branch":
                scope = ManagementRatingsScope.Branch;
                return true;
            case "list":
            case "delegate":
                scope = ManagementRatingsScope.List;
                return true;
            case "follower":
                scope = ManagementRatingsScope.Follower;
                return true;
            case "employee":
            case "salesemployee":
                scope = ManagementRatingsScope.Employee;
                return true;
            default:
                return false;
        }
    }

    public static string ToApiKey(ManagementRatingsScope scope) => scope switch
    {
        ManagementRatingsScope.List => "list",
        ManagementRatingsScope.Follower => "follower",
        ManagementRatingsScope.Employee => "employee",
        _ => "branch",
    };
}

public static class ManagementRatingsLevelParser
{
    public static bool TryParse(string? raw, out string normalizedArabic)
    {
        normalizedArabic = "";
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        switch (raw.Trim().ToLowerInvariant())
        {
            case "excellent":
            case "ممتاز":
                normalizedArabic = "ممتاز";
                return true;
            case "good":
            case "جيد":
                normalizedArabic = "جيد";
                return true;
            case "weak":
            case "ضعيف":
                normalizedArabic = "ضعيف";
                return true;
            case "rejected":
            case "lagging":
            case "متلكئ":
            case "مرفوض":
                normalizedArabic = "متلكئ";
                return true;
            case "bad":
            case "سيء":
                normalizedArabic = "سيء";
                return true;
            default:
                return false;
        }
    }
}
