namespace Application.Common.Models;

public record Pagination
{
    public Pagination()
    {
        PageIndex = 1;
        PageSize = 10;
    }
    public Pagination(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
};