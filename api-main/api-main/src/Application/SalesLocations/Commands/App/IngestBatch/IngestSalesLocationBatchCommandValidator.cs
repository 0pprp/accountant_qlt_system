namespace Application.SalesLocations.Commands.App.IngestBatch;

public class IngestSalesLocationBatchCommandValidator : AbstractValidator<IngestSalesLocationBatchCommand>
{
    public IngestSalesLocationBatchCommandValidator()
    {
        RuleFor(x => x.Points)
            .NotEmpty();

        RuleForEach(x => x.Points).ChildRules(point =>
        {
            point.RuleFor(p => p.Lat)
                .InclusiveBetween(-90, 90);

            point.RuleFor(p => p.Lng)
                .InclusiveBetween(-180, 180);
        });
    }
}
