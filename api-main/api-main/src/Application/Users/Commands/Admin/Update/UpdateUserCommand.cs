namespace Application.Users.Commands.Admin.Update;

public record UpdateUserCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string Username { get; set; }
    public required string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public int RoleId { get; set; }
    public required List<int> BranchIds { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
}

public record UpdateUserDto
{
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string Username { get; set; }
    public required string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public int RoleId { get; set; }
    public required List<int> BranchIds { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
}