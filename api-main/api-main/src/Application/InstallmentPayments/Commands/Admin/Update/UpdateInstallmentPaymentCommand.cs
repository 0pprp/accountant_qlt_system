namespace Application.InstallmentPayments.Commands.Admin.Update;

public record UpdateInstallmentPaymentCommand : IRequest<Result>
{
    public int Id { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
}

public record UpdateInstallmentPaymentDto
{
    public double Amount { get; set; }
    public string? Description { get; set; }
}