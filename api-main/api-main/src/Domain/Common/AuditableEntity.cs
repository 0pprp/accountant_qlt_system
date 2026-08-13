namespace Domain.Common;

public abstract class BaseEntity { }

public abstract class Entity<T> : BaseEntity
{
    public T Id { get; set; }
}

public abstract class Entity : Entity<int> { }

public abstract class AuditableEntity<T> : Entity<T>
{
    public DateTimeOffset? CreatedAt { get; set; }
    public T CreatedBy { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
    public T LastUpdatedBy { get; set; }
}

public abstract class AuditableEntity : AuditableEntity<int> { }