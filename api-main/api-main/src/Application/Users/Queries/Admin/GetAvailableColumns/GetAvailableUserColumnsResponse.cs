namespace Application.Users.Queries.Admin.GetAvailableColumns;

public record GetAvailableUserColumnsResponse
{
    public required string Key { get; init; }
    public required string DisplayName { get; init; }
    public required string DataType { get; init; }
}
