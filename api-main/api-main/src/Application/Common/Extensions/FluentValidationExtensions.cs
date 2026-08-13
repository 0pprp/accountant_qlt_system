namespace Application.Common.Extensions;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, double> MustBeDivisibleBy1000<T>(this IRuleBuilder<T, double> ruleBuilder)
    {
        return ruleBuilder
            .Must(amount => Math.Abs(amount % 1000) < 0.01)
            .WithMessage("يجب أن يكون المبلغ قابلاً للقسمة على 1000.");
    }
}

