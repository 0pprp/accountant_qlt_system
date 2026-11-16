using Application.Common.Interfaces;
using Domain.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Common;

public static class CustomerStaffRatingUpsert
{
    public static async Task UpsertAsync(
        IApplicationDbContext db,
        int customerId,
        string role,
        int score,
        string? note,
        int ratedByUserId,
        string? ratedByName,
        DateTimeOffset ratedAtUtc,
        int? orderId,
        CancellationToken ct)
    {
        var normalizedRole = CustomerStaffRating.NormalizeRole(role);
        var existing = await db.CustomerStaffRatings
            .FirstOrDefaultAsync(
                x => x.CustomerId == customerId && x.Role == normalizedRole,
                ct);

        if (existing is null)
        {
            db.CustomerStaffRatings.Add(new CustomerStaffRating(
                customerId,
                normalizedRole,
                score,
                note,
                ratedByUserId,
                ratedByName,
                ratedAtUtc,
                orderId));
            return;
        }

        existing.Apply(score, note, ratedByUserId, ratedByName, ratedAtUtc, orderId);
    }
}
