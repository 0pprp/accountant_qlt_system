namespace Application.Safes.Commands.Admin.CreateCashDeliveries;

public class CreateCashDeliveriesCommandValidator : AbstractValidator<CreateCashDeliveriesCommand>
{
    public CreateCashDeliveriesCommandValidator()
    {
        RuleFor(x => x.SellerIds)
            .NotEmpty()
            .WithMessage("يجب عليك تحديد بائع واحد على الأقل.");
    }
}