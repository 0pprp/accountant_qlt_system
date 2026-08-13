namespace Application.Branches.Commands.Admin.Update;

public record UpdateBranchCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int ProvinceId { get; set; }    
}

public record UpdateBranchDto
{
    public required string Name { get; set; }
    public int ProvinceId { get; set; }
}