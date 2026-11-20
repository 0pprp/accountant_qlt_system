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
        // قوالب لوحة الإدارة الموحدة — لا تُربط بـ ChiefAccountant تخمينًا
        All.Add(Director);
        All.Add(Monitor);
        All.Add(SalesManager);
        All.Add(ArchiveOfficer);
        All.Add(FieldPromoter);
    }
    
    public static readonly RoleName Admin = new(nameof(Admin), "المدیر");
    public static readonly RoleName ChiefAccountant = new(nameof(ChiefAccountant), "مسئول المحاسبین");
    public static readonly RoleName MainAccountant = new(nameof(MainAccountant), "المحاسب الرئیسی");
    public static readonly RoleName BranchAccountant = new(nameof(BranchAccountant), "المحاسب الفرعی");
    public static readonly RoleName BranchManager = new(nameof(BranchManager), "مدیر الفرع");
    public static readonly RoleName Ceo = new(nameof(Ceo), "المدیر التنفیذی");
    public static readonly RoleName Mandob = new(nameof(Mandob), "المندوب");
    public static readonly RoleName Motaba = new(nameof(Motaba), "المتابع");

    /// <summary>مروّج ميداني: يرسل طلبات بيع ويثبّت المحلات ويُتتبَّع موقعه أثناء الدوام.</summary>
    public static readonly RoleName FieldPromoter = new(nameof(FieldPromoter), "مروّج ميداني");

    /// <summary>قالب افتراضي — مصدر: FE_Company UserType «المدير». لا يمنح ManagePermissions.</summary>
    public static readonly RoleName Director = new(nameof(Director), "المدير");

    /// <summary>قالب افتراضي — مصدر: FE_Company UserType «المراقب». لا يمنح ManagePermissions.</summary>
    public static readonly RoleName Monitor = new(nameof(Monitor), "المراقب");

    /// <summary>قالب افتراضي — مصدر: FE_Company UserType «مدير مبيعات». لا يمنح ManagePermissions.</summary>
    public static readonly RoleName SalesManager = new(nameof(SalesManager), "مدير مبيعات");

    /// <summary>نهاية سلسلة عهدة الورق — يستلم ويؤرشف ويسترجع الناقص.</summary>
    public static readonly RoleName ArchiveOfficer = new(nameof(ArchiveOfficer), "مسؤول الأرشيف");
}

public record RoleName(string Name, string DisplayName);