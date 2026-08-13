namespace Application.Users.Commands.Admin.Create;

public record CreateUserCommand : IRequest<Result<CreateUserResponse>>
{
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public int RoleId { get; set; }
    public required List<int> BranchIds { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
}