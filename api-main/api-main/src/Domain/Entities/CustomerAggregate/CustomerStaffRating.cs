using Domain.Common;

namespace Domain.Entities.CustomerAggregate;

/// <summary>
/// تقييم موظف على مستوى الزبون (ليس تقييم يوم القائمة).
/// دور واحد لكل صف: مندوب / مسؤول لجنة ميدانية / موظف مبيعات.
/// </summary>
public class CustomerStaffRating : Entity
{
    public const string RoleDelegate = "Delegate";
    public const string RoleCommitteeField = "CommitteeField";
    public const string RoleSalesEmployee = "SalesEmployee";
    public const int MaxNoteLength = 2000;
    public const int MaxNameLength = 200;

    /// <summary>الدرجة التي تحتها تصير الملاحظة إجبارية — قاعدة النظام الميداني.</summary>
    public const int NoteRequiredBelowScore = 7;

    /// <summary>الدرجة التي عندها فما دون يُعدّ المبيع خاطئاً.</summary>
    public const int WrongSaleMaxScore = 6;

    private CustomerStaffRating()
    {
    }

    public CustomerStaffRating(
        int customerId,
        string role,
        int score,
        string? note,
        int ratedByUserId,
        string? ratedByName,
        DateTimeOffset ratedAtUtc,
        int? orderId = null)
    {
        if (customerId <= 0)
            throw new InvalidOperationException("CustomerId مطلوب");
        if (!IsValidRole(role))
            throw new InvalidOperationException("دور التقييم غير معروف");
        if (score is < 1 or > 10)
            throw new InvalidOperationException("التقييم يجب أن يكون من 1 إلى 10");

        CustomerId = customerId;
        Role = role;
        Apply(score, note, ratedByUserId, ratedByName, ratedAtUtc, orderId);
    }

    public int CustomerId { get; private set; }
    public string Role { get; private set; } = RoleCommitteeField;
    public int Score { get; private set; }
    public string? Note { get; private set; }
    public int RatedByUserId { get; private set; }
    public string? RatedByName { get; private set; }
    public DateTimeOffset RatedAtUtc { get; private set; }
    public int? OrderId { get; private set; }

    /// <summary>حقول استمارة التقييم — قيمة المخزون المقدّرة وقت التقييم.</summary>
    public double? ShopStockEstimatedValue { get; private set; }

    /// <summary>الإيراد اليومي المقدّر وقت التقييم.</summary>
    public double? EstimatedDailyRevenue { get; private set; }

    /// <summary>المبلغ المرتبط بلحظة التقييم — مبلغ البيع للمندوب، وسقف البيع للمبيعات.</summary>
    public double? ContextAmount { get; private set; }

    /// <summary>اللحظة التي التُقط فيها التقييم — مصادقة أو بيع أو أول تسديد.</summary>
    public string? Occasion { get; private set; }

    /// <summary>مبيع خاطئ — الدرجة ستة فأقل، تُحوَّل في النظام الميداني إلى صندوق الاستثناءات.</summary>
    public bool IsWrongSale => Score <= WrongSaleMaxScore;

    public const string OccasionAuthentication = "Authentication";
    public const string OccasionSale = "Sale";
    public const string OccasionFirstCollection = "FirstCollection";

    public void SetFormDetails(
        double? shopStockEstimatedValue,
        double? estimatedDailyRevenue,
        double? contextAmount,
        string? occasion)
    {
        ShopStockEstimatedValue = shopStockEstimatedValue;
        EstimatedDailyRevenue = estimatedDailyRevenue;
        ContextAmount = contextAmount;
        Occasion = occasion;
    }

    public void Apply(
        int score,
        string? note,
        int ratedByUserId,
        string? ratedByName,
        DateTimeOffset ratedAtUtc,
        int? orderId = null)
    {
        if (score is < 1 or > 10)
            throw new InvalidOperationException("التقييم يجب أن يكون من 1 إلى 10");
        if (ratedByUserId <= 0)
            throw new InvalidOperationException("RatedByUserId مطلوب");
        if (score < NoteRequiredBelowScore && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("الملاحظة إجبارية عندما يكون التقييم أقل من 7");

        Score = score;
        Note = NormalizeNote(note);
        RatedByUserId = ratedByUserId;
        RatedByName = NormalizeName(ratedByName);
        RatedAtUtc = ratedAtUtc;
        OrderId = orderId is > 0 ? orderId : null;
    }

    public static bool IsValidRole(string? role) =>
        string.Equals(role, RoleDelegate, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, RoleCommitteeField, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, RoleSalesEmployee, StringComparison.OrdinalIgnoreCase);

    public static string NormalizeRole(string role) =>
        string.Equals(role, RoleDelegate, StringComparison.OrdinalIgnoreCase) ? RoleDelegate
        : string.Equals(role, RoleSalesEmployee, StringComparison.OrdinalIgnoreCase) ? RoleSalesEmployee
        : RoleCommitteeField;

    private static string? NormalizeNote(string? note)
    {
        var t = (note ?? string.Empty).Trim();
        if (t.Length == 0)
            return null;
        return t.Length > MaxNoteLength ? t[..MaxNoteLength] : t;
    }

    private static string? NormalizeName(string? name)
    {
        var t = (name ?? string.Empty).Trim();
        if (t.Length == 0)
            return null;
        return t.Length > MaxNameLength ? t[..MaxNameLength] : t;
    }
}
