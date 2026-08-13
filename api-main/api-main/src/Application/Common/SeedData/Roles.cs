namespace Application.Common.SeedData;

public static class Roles
{
    public static readonly List<RoleName> All = [];

    static Roles()
    {
        All.Add(Admin);
        All.Add(ChiefAccountant);
        All.Add(MainAccountant);
        All.Add(BranchAccountant);
        All.Add(BranchManager);
        All.Add(Ceo);
        All.Add(Mandob);
        All.Add(Motaba);
    }
    
    public static readonly RoleName Admin = new(nameof(Admin), "المدیر");
    public static readonly RoleName ChiefAccountant = new(nameof(ChiefAccountant), "مسئول المحاسبین");
    public static readonly RoleName MainAccountant = new(nameof(MainAccountant), "المحاسب الرئیسی");
    public static readonly RoleName BranchAccountant = new(nameof(BranchAccountant), "المحاسب الفرعی");
    public static readonly RoleName BranchManager = new(nameof(BranchManager), "مدیر الفرع");
    public static readonly RoleName Ceo = new(nameof(Ceo), "المدیر التنفیذی");
    public static readonly RoleName Mandob = new(nameof(Mandob), "المندوب");
    public static readonly RoleName Motaba = new(nameof(Motaba), "المتابع");
}

public record RoleName(string Name, string DisplayName);