using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;

namespace Application.Tests.Committee;

public class CustomerGpsAndStaffRatingTests
{
    [Fact]
    public void Business_SetGps_stores_coordinates()
    {
        var business = new Business("محل", "عنوان", "علامة");
        Assert.False(business.HasGps);

        business.SetGps(33.3, 44.4);

        Assert.True(business.HasGps);
        Assert.Equal(33.3, business.GpsLatitude);
        Assert.Equal(44.4, business.GpsLongitude);
    }

    [Fact]
    public void Business_SetGps_rejects_out_of_range()
    {
        var business = new Business("محل", "عنوان", "علامة");
        Assert.Throws<ArgumentOutOfRangeException>(() => business.SetGps(100, 0));
    }

    [Fact]
    public void Order_SetLocation_assigns_value()
    {
        var order = new Order();
        var location = new Location(33.31, 44.41);
        order.SetLocation(location);
        Assert.Equal(33.31, order.Location!.Latitude);
        Assert.Equal(44.41, order.Location.Longitude);
    }

    [Fact]
    public void CustomerStaffRating_Apply_updates_latest_values()
    {
        var rating = new CustomerStaffRating(
            7,
            CustomerStaffRating.RoleCommitteeField,
            8,
            "أول",
            3,
            "متابع",
            DateTimeOffset.UtcNow,
            11);

        rating.Apply(5, "ثاني", 9, "متابع 2", DateTimeOffset.UtcNow.AddMinutes(1), 12);

        Assert.Equal(5, rating.Score);
        Assert.Equal("ثاني", rating.Note);
        Assert.Equal(9, rating.RatedByUserId);
        Assert.Equal(12, rating.OrderId);
        Assert.Equal(CustomerStaffRating.RoleCommitteeField, rating.Role);
    }
}
