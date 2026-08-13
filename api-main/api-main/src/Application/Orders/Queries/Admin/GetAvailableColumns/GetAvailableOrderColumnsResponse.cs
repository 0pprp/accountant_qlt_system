namespace Application.Orders.Queries.Admin.GetAvailableColumns;

public record GetAvailableOrderColumnsResponse
{
    public required string Key { get; init; }
    public required string DisplayName { get; init; }
    public required string DataType { get; init; }
}

