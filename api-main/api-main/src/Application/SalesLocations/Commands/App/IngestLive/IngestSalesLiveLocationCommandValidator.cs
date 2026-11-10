using Application.FollowerWork.Common;

namespace Application.SalesLocations.Commands.App.IngestLive;

public class IngestSalesLiveLocationCommandValidator : AbstractValidator<IngestSalesLiveLocationCommand>
{
    public IngestSalesLiveLocationCommandValidator()
    {
        RuleFor(x => x.Lat).Must(lat => FollowerLocationRules.IsValidCoordinate(lat, 0) || lat is >= -90 and <= 90);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
    }
}
