using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Waivers.Common;

public static class WaiverScope
{
    public const int DecisionNoteMaxLength = 500;

    public static bool CanSeeAllBranches(ICurrentUserService current)
        => (current.BranchIds ?? []).Count == 0
           || current.HasPermission(SeedPermissions.Waiver.Apply);

    public static bool InScope(ICurrentUserService current, int branchId)
        => CanSeeAllBranches(current) || (current.BranchIds ?? []).Contains(branchId);

    public static string? NormalizeNote(string? note)
    {
        var trimmed = note?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
