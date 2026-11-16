namespace Application.Users.Commands.Admin.Update;

public record UpdateUserCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public string? MotherName { get; set; }
    public required string Username { get; set; }
    public string? NationalCode { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int RoleId { get; set; }
    public required List<int> BranchIds { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}

public record UpdateUserDto
{
    public required string FullName { get; set; }
    public string? MotherName { get; set; }
    public required string Username { get; set; }
    public string? NationalCode { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int RoleId { get; set; }
    public required List<int> BranchIds { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}
