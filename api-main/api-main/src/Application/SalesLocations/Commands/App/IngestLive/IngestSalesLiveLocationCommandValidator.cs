namespace Application.SalesLocations.Commands.App.IngestLive;

public class IngestSalesLiveLocationCommandValidator : AbstractValidator<IngestSalesLiveLocationCommand>
{
    public IngestSalesLiveLocationCommandValidator()
    {
        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Lng)
            .InclusiveBetween(-180, 180);
    }
}
