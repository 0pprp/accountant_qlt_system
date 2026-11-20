using Domain.Common;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.FieldPromoterAggregate;

/// <summary>بيانات المروّج الميداني الخاصة به: الإيقاف والهدف اليومي.</summary>
public class FieldPromoterProfile : Entity
{
    public const int MaxReasonLength = 300;

    private FieldPromoterProfile()
    {
    }

    public FieldPromoterProfile(int userId, DateTimeOffset createdAtUtc)
    {
        UserId = userId;
        CreatedAtUtc = createdAtUtc;
    }

    public int UserId { get; private set; }
    public bool IsSuspended { get; private set; }
    public string? SuspendReason { get; private set; }

    /// <summary>هدف دقائق العمل اليومي الخاص به؛ null يعني الافتراضي العام.</summary>
    public int? DailyWorkMinutesTarget { get; set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public User? User { get; set; }

    public void Suspend(string? reason)
    {
        IsSuspended = true;
        SuspendReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    public void Activate()
    {
        IsSuspended = false;
        SuspendReason = null;
    }
}

/// <summary>محل ثبّته مروّج على الخريطة مع درجة اهتمام صاحبه.</summary>
public class FieldPromoterPin : Entity
{
    public const int MaxNameLength = 150;
    public const int MaxNoteLength = 500;

    private FieldPromoterPin()
    {
    }

    public FieldPromoterPin(int userId, double latitude, double longitude, string name, byte interest,
        string? phone, string? note, double? agentLatitude, double? agentLongitude, DateTimeOffset createdAtUtc)
    {
        UserId = userId;
        Latitude = latitude;
        Longitude = longitude;
        Name = name;
        Interest = interest;
        Phone = phone;
        Note = note;
        AgentLatitude = agentLatitude;
        AgentLongitude = agentLongitude;
        CreatedAtUtc = createdAtUtc;
    }

    public int UserId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string Name { get; private set; } = "";

    /// <summary>1 غير مهتم إطلاقاً … 5 مهتم جداً.</summary>
    public byte Interest { get; private set; }

    public string? Phone { get; private set; }
    public string? Note { get; private set; }
    public double? AgentLatitude { get; private set; }
    public double? AgentLongitude { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public User? User { get; set; }

    public void Update(string name, byte interest, string? phone, string? note, DateTimeOffset atUtc)
    {
        Name = name;
        Interest = interest;
        Phone = phone;
        Note = note;
        UpdatedAtUtc = atUtc;
    }

    public void Delete(DateTimeOffset atUtc)
    {
        IsDeleted = true;
        DeletedAtUtc = atUtc;
    }
}

/// <summary>صورة المحل وموقعه المرفقان بطلب بيع أرسله مروّج. خاص: لا يُعرض إلا للمخوّلين.</summary>
public class FieldPromoterOrderShop : Entity
{
    public const int MaxPhotoBytes = 2 * 1024 * 1024;

    private FieldPromoterOrderShop()
    {
    }

    public FieldPromoterOrderShop(int salesRequestId, double? latitude, double? longitude,
        byte[]? photo, string? photoContentType)
    {
        SalesRequestId = salesRequestId;
        Latitude = latitude;
        Longitude = longitude;
        Photo = photo;
        PhotoContentType = photoContentType;
    }

    public int SalesRequestId { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public byte[]? Photo { get; private set; }
    public string? PhotoContentType { get; private set; }
}

/// <summary>تنبيه وقوف طويل: المروّج بقي ضمن نصف قطر صغير مدة طويلة أثناء الدوام.</summary>
public class FieldPromoterAlert : Entity
{
    private FieldPromoterAlert()
    {
    }

    public FieldPromoterAlert(int userId, int shiftId, double latitude, double longitude,
        DateTimeOffset sinceUtc, DateTimeOffset raisedAtUtc)
    {
        UserId = userId;
        ShiftId = shiftId;
        Latitude = latitude;
        Longitude = longitude;
        SinceUtc = sinceUtc;
        LastSeenAtUtc = raisedAtUtc;
        RaisedAtUtc = raisedAtUtc;
    }

    public int UserId { get; private set; }
    public int ShiftId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTimeOffset SinceUtc { get; private set; }
    public DateTimeOffset LastSeenAtUtc { get; private set; }
    public DateTimeOffset RaisedAtUtc { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public DateTimeOffset? SeenAtUtc { get; private set; }
    public int? SeenByUserId { get; private set; }

    public User? User { get; set; }

    public void Touch(DateTimeOffset atUtc) => LastSeenAtUtc = atUtc;

    public void Resolve(DateTimeOffset atUtc) => ResolvedAtUtc ??= atUtc;

    public void MarkSeen(int userId, DateTimeOffset atUtc)
    {
        if (SeenAtUtc is not null)
            return;
        SeenAtUtc = atUtc;
        SeenByUserId = userId;
    }
}

/// <summary>يوم عمل مجمَّد بعد انتهائه: دقائق العمل مقابل الهدف.</summary>
public class FieldPromoterDay : Entity
{
    private FieldPromoterDay()
    {
    }

    public FieldPromoterDay(int userId, DateOnly day, int workMinutes, int targetMinutes, int pins, int orders,
        FieldPromoterDayStatus status)
    {
        UserId = userId;
        Day = day;
        WorkMinutes = workMinutes;
        TargetMinutes = targetMinutes;
        Pins = pins;
        Orders = orders;
        Status = status;
    }

    public int UserId { get; private set; }
    public DateOnly Day { get; private set; }
    public int WorkMinutes { get; private set; }
    public int TargetMinutes { get; private set; }
    public int Pins { get; private set; }
    public int Orders { get; private set; }
    public FieldPromoterDayStatus Status { get; private set; }

    public User? User { get; set; }
}

public enum FieldPromoterDayStatus : byte
{
    NoTarget = 0,
    Met = 1,
    Missed = 2,
    Off = 3
}

/// <summary>إعدادات المروّجين العامة — صف واحد.</summary>
public class FieldPromoterSettings : Entity
{
    public const int DefaultStationaryRadiusMeters = 150;
    public const int DefaultStationaryMinutes = 60;
    public const int MaxSupportLength = 20;

    public int DefaultDailyWorkMinutes { get; set; }
    public int StationaryRadiusMeters { get; set; } = DefaultStationaryRadiusMeters;
    public int StationaryMinutes { get; set; } = DefaultStationaryMinutes;
    public string? SupportPhone { get; set; }
    public string? SupportWhatsapp { get; set; }
    public DateOnly? LastFrozenDay { get; set; }
    public DateOnly? LastRetentionRunDay { get; set; }
}
