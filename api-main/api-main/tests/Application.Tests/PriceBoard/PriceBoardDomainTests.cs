using Domain.Entities.PriceBoardAggregate;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.PriceBoard;

public class PriceBoardDomainTests
{
    [Fact]
    public void Create_stores_decimal_amounts_and_province()
    {
        var e = new PriceBoardEntry(
            "مكيف 1 طن", 850_000m, 12_000m, 50_000m,
            3, "النجف", 9, "مراقب", DateTimeOffset.UtcNow);
        Assert.Equal(850_000m, e.SalePrice);
        Assert.Equal(3, e.TargetProvinceId);
        Assert.Equal(0, e.ConcurrencyStamp);
    }

    [Fact]
    public void Rejects_negative_amounts()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new PriceBoardEntry("x", -1m, 0, 0, 1, "بغداد", 1, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Update_requires_matching_version()
    {
        var e = new PriceBoardEntry("a", 1m, 1m, 1m, 1, "ب", 1, null, DateTimeOffset.UtcNow);
        e.Update("b", 2m, 2m, 2m, 1, "ب", 2, "م", 0, DateTimeOffset.UtcNow);
        Assert.Equal(1, e.ConcurrencyStamp);
        Assert.Throws<InvalidOperationException>(() =>
            e.Update("c", 3m, 3m, 3m, 1, "ب", 2, "م", 0, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Permissions_outside_all_and_monitor_has_write()
    {
        Assert.DoesNotContain(SeedPermissions.All, p => p.Name == SeedPermissions.PriceBoard.Create);
        var monitor = Application.Common.SeedData.RoleDefaultMatrix.SeedPermissionsFor(
            Application.Common.SeedData.Roles.Monitor.Name);
        Assert.Contains(SeedPermissions.PriceBoard.Create, monitor);
        Assert.Contains(SeedPermissions.PriceBoard.Delete, monitor);
        var director = Application.Common.SeedData.RoleDefaultMatrix.SeedPermissionsFor(
            Application.Common.SeedData.Roles.Director.Name);
        Assert.DoesNotContain(SeedPermissions.PriceBoard.Create, director);
    }

    [Fact]
    public void Does_not_touch_order_or_product_permissions()
    {
        Assert.NotEqual(SeedPermissions.PriceBoard.Update, SeedPermissions.Product.Update);
        Assert.NotEqual(SeedPermissions.PriceBoard.Update, SeedPermissions.Order.Update);
    }
}
