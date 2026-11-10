using Application.Common.Interfaces;
using Application.SalesEmployees.Common;
using Domain.Entities.AnnouncementAggregate.Enums;
using Domain.Entities.TutorialVideoAggregate.Enums;

namespace Application.Common.Content;

/// <summary>
/// يربط جمهور المحتوى بهوية الجلسة — لا يُؤخذ appKey من العميل كمصدر ثقة وحده.
/// تكليف SalesEmployeeAssignment يسبق تصنيف Mandob كمندوب جباية.
/// </summary>
public static class ContentAppAudience
{
    public static async Task<AnnouncementAppKey?> ResolveAnnouncementAsync(
        IApplicationDbContext db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        CancellationToken ct)
    {
        if (current.UserId is not int userId)
            return null;

        // موظف مبيعات مكلّف: لا جمهور إعلان في العقد الحالي (Delegate|Follower فقط).
        if (await SalesWorkforce.HasAnyActiveAssignmentAsync(db, userId, clock.UtcNowOffset, ct))
            return null;

        var role = current.Role ?? "";
        if (IsMotaba(role))
            return AnnouncementAppKey.Follower;
        if (IsMandob(role))
            return AnnouncementAppKey.Delegate;
        return null;
    }

    public static async Task<TutorialAppKey?> ResolveTutorialAsync(
        IApplicationDbContext db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        CancellationToken ct)
    {
        if (current.UserId is not int userId)
            return null;

        if (await SalesWorkforce.HasAnyActiveAssignmentAsync(db, userId, clock.UtcNowOffset, ct))
            return TutorialAppKey.SalesEmployee;

        var role = current.Role ?? "";
        if (IsMotaba(role))
            return TutorialAppKey.Follower;
        if (role.Equals("BranchManager", StringComparison.OrdinalIgnoreCase))
            return TutorialAppKey.BranchManager;
        if (IsMandob(role))
            return TutorialAppKey.Delegate;
        return null;
    }

    public static bool AllowsAnnouncement(AnnouncementAppKey allowed, AnnouncementAppKey requested) =>
        allowed == requested;

    public static bool AllowsTutorial(TutorialAppKey allowed, TutorialAppKey requested) =>
        allowed == requested;

    private static bool IsMotaba(string role) =>
        role.Equals("Motaba", StringComparison.OrdinalIgnoreCase)
        || role.Equals("Follower", StringComparison.OrdinalIgnoreCase);

    private static bool IsMandob(string role) =>
        role.Equals("Mandob", StringComparison.OrdinalIgnoreCase)
        || role.Equals("Delegate", StringComparison.OrdinalIgnoreCase);
}
