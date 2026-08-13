namespace Domain.Common;

public interface ISoftDelete
{
    public int? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}