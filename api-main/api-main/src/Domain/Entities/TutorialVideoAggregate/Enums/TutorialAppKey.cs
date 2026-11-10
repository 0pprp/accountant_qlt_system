namespace Domain.Entities.TutorialVideoAggregate.Enums;

public enum TutorialAppKey : byte
{
    Delegate = 1,
    BranchManager = 2,
    Follower = 3,
    /// <summary>عقد موسّع بأمان — جمهور موظف المبيعات (ليس استعارة follower).</summary>
    SalesEmployee = 4
}

public static class TutorialAppKeyLabels
{
    public static string Arabic(TutorialAppKey k) => k switch
    {
        TutorialAppKey.Delegate => "المندوب",
        TutorialAppKey.BranchManager => "إدارة الفرع",
        TutorialAppKey.Follower => "المتابع / مسؤول اللجنة",
        TutorialAppKey.SalesEmployee => "موظف المبيعات",
        _ => k.ToString()
    };

    public static string Wire(TutorialAppKey k) => k switch
    {
        TutorialAppKey.Delegate => "delegate",
        TutorialAppKey.BranchManager => "branch-manager",
        TutorialAppKey.Follower => "follower",
        TutorialAppKey.SalesEmployee => "sales-employee",
        _ => k.ToString().ToLowerInvariant()
    };

    public static bool TryParse(string? raw, out TutorialAppKey key)
    {
        key = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var s = raw.Trim().ToLowerInvariant();
        key = s switch
        {
            "delegate" or "مندوب" or "المندوب" => TutorialAppKey.Delegate,
            "branch-manager" or "branchmanager" or "إدارة الفرع" or "مدير فرع" => TutorialAppKey.BranchManager,
            "follower" or "متابع" or "المتابع" or "motaba" or "committee" or "مسؤول اللجنة"
                => TutorialAppKey.Follower,
            "sales-employee" or "salesemployee" or "موظف المبيعات" or "mandob-sales" or "employee"
                => TutorialAppKey.SalesEmployee,
            _ => default
        };
        return key != default;
    }
}
