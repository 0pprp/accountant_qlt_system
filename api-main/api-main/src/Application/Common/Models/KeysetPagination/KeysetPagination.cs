namespace Application.Common.Models.KeysetPagination;

public record KeysetPagination<T>
{
    public KeysetPagination()
    {
        Size = 20;
        Direction = PaginationDirection.Forward;
    }
    public KeysetPagination(T? lastId, int size, PaginationDirection direction)
    {
        LastId = lastId;
        Size = size;
        Direction = direction;
    }
    public T? LastId { get; set; }
    public int Size { get; set; }
    public PaginationDirection Direction { get; set; }
}

public record KeysetPagination : KeysetPagination<int?>
{
    public KeysetPagination() { }
    public KeysetPagination(int? lastId, int size, PaginationDirection direction) : base(lastId, size, direction)
    {
    }
}
