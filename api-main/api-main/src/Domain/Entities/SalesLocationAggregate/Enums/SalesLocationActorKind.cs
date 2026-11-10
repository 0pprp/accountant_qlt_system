namespace Domain.Entities.SalesLocationAggregate.Enums;

/// <summary>
/// نوع الفاعل عند الالتقاط — يُشتق من مسار السيرفر لا من العميل.
/// </summary>
public enum SalesLocationActorKind : byte
{
    Unknown = 0,
    SalesEmployee = 1,
    Follower = 2,
}
