namespace Application.Customers.Queries.App.GetPaginated;

public record GetPaginatedCustomersResponse
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string BusinessName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}