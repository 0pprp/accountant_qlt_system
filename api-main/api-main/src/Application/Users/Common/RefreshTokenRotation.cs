using Application.Common.Interfaces;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Common;

/// <summary>
/// تدوير مفتاح التجديد دون قفل الجهاز: المفتاح الجديد صف مستقل، والقديم يبقى صالحاً لمهلة قصيرة
/// فإن ضاع رد التجديد على شبكة ضعيفة يجدّد الهاتف بالقديم ولا يُطرد. لا يمدّ عمر الجلسة.
/// </summary>
public static class RefreshTokenRotation
{
    public static readonly TimeSpan Grace = TimeSpan.FromHours(24);

    public static async Task RotateAsync(
        IApplicationDbContext db,
        int userId,
        string oldTokenHash,
        string newTokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await db.UserLogins
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var used = rows.Where(x => x.RefreshToken == oldTokenHash).ToList();
        if (used.Count == 0)
            return;

        var sessionEnd = rows
            .Where(x => x.RefreshTokenExpiresAt >= now)
            .Select(x => x.RefreshTokenExpiresAt)
            .DefaultIfEmpty(used.Max(x => x.RefreshTokenExpiresAt))
            .Max();
        var graceEnd = now + Grace;

        foreach (var row in used)
        {
            if (row.RefreshTokenExpiresAt > graceEnd)
                row.RefreshTokenExpiresAt = graceEnd;
        }

        db.UserLogins.Add(new UserLogin(newTokenHash, sessionEnd, userId));
        db.UserLogins.RemoveRange(rows.Where(x => x.RefreshTokenExpiresAt < now));

        await db.SaveChangesAsync(cancellationToken);
    }
}
