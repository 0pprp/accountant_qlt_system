namespace Application.Branches.Commands.Admin.Create;

public record CreateBranchCommand : IRequest<Result>
{
    public required string Name { get; set; }
    public int ProvinceId { get; set; }
}