using Application.Common.Interfaces;

namespace Application.Customers.Commands.App.Create;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateCustomerCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون الأسم الثلاثي فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز الأسم الثلاثي 50 حرف");

        RuleFor(x => x.MotherName)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون أسم الأم فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز أسم الأم 50 حرف");

        RuleFor(x => x.NationalCode)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون رقم الهوية فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز رقم الهوية 50 حرف");

        RuleFor(x => x.BirthDate)
            .Must(BeAtLeast18YearsOld)
            .WithMessage("يجب ألا يقل عمر العميل عن 18 عامًا.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون رقم الهاتف فارغين.")
            .Length(11)
            .WithMessage("رقم الهاتف غير صحيح.");

        RuleFor(x => x.WhatsAppPhoneNumber)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون رقم الهاتف (وتساب) فارغين.")
            .Length(11)
            .WithMessage("رقم الهاتف (وتساب) غير صحيح.");

        RuleFor(x => x.Business.Name)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون أسم المتجر فارغين.")
            .MaximumLength(100)
            .WithMessage("يجب ألا يتجاوز أسم المتجر 100 حرف");

        RuleFor(x => x.Business.Address)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون عنوان المتجر فارغين.")
            .MaximumLength(300)
            .WithMessage("يجب ألا يتجاوز عنوان المتجر 300 حرف");

        RuleFor(x => x.Business.NearestKnownLocation)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون أقرب نقطة دالة فارغين.")
            .MaximumLength(200)
            .WithMessage("يجب ألا يتجاوز أقرب نقطة دالة 200 حرف");
    }

    private bool BeAtLeast18YearsOld(DateOnly birthDate)
    {
        var today = _dateTimeProvider.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
            age--;

        return age >= 18;
    }
}