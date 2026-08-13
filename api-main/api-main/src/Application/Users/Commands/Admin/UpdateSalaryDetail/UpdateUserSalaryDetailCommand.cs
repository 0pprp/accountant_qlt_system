using Domain.Entities.UserAggregate.Enums;

namespace Application.Users.Commands.Admin.UpdateSalaryDetail;

public record UpdateUserSalaryDetailCommand : IRequest<Result>
{
    public int UserId { get; set; }
    public SalaryType Type { get; set; }
    public double Amount { get; set; }
    public double? SaleSharePercent { get; set; }
    public double? InstallmentSharePercent { get; set; }
}

public record UpdateUserSalaryDetailDto
{
    public SalaryType Type { get; set; }
    public double Amount { get; set; }
    public double? SaleSharePercent { get; set; }
    public double? InstallmentSharePercent { get; set; }
}