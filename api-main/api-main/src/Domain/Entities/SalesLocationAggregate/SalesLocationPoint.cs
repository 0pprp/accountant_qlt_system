using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.SalesLocationAggregate.Enums;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.SalesLocationAggregate;

/// <summary>
/// نقطة موقع ميدانية. ActorKind يُثبَّت من مسار السيرفر (مبيعات vs متابع).
/// RecordedAtUtc = وقت التقاط الجهاز؛ ReceivedAtUtc = استلام السيرفر.
/// </summary>
public class SalesLocationPoint : Entity
{
    public const int MaxClientEventIdLength = 64;

    private SalesLocationPoint()
    {
    }

    public SalesLocationPoint(
        int userId,
        int? branchId,
        double latitude,
        double longitude,
        DateTimeOffset recordedAtUtc,
        DateTimeOffset receivedAtUtc,
        bool isLive,
        SalesLocationActorKind actorKind,
        double? accuracyMeters = null,
        int? shiftId = null,
        string? clientEventId = null)
    {
        UserId = userId;
        BranchId = branchId;
        Latitude = latitude;
        Longitude = longitude;
        RecordedAtUtc = recordedAtUtc;
        ReceivedAtUtc = receivedAtUtc;
        IsLive = isLive;
        ActorKind = actorKind;
        AccuracyMeters = accuracyMeters;
        ShiftId = shiftId;
        ClientEventId = string.IsNullOrWhiteSpace(clientEventId)
            ? null
            : clientEventId.Trim();
    }

    public int UserId { get; set; }
    public int? BranchId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>وقت التقاط الجهاز (أساس آخر موقع والمسار).</summary>
    public DateTimeOffset RecordedAtUtc { get; set; }

    /// <summary>وقت استلام السيرفر.</summary>
    public DateTimeOffset ReceivedAtUtc { get; set; }

    public bool IsLive { get; set; }

    /// <summary>مشتق سيرفريًا — لا يُقبل من العميل.</summary>
    public SalesLocationActorKind ActorKind { get; set; }

    /// <summary>دقة بالأمتار — null إن لم تُرسل؛ لا تُعامل كصفر.</summary>
    public double? AccuracyMeters { get; set; }

    public int? ShiftId { get; set; }

    /// <summary>معرّف حدث العميل لمنع التكرار (فريد لكل مستخدم).</summary>
    public string? ClientEventId { get; set; }

    public User? User { get; set; }
    public Branch? Branch { get; set; }
}
