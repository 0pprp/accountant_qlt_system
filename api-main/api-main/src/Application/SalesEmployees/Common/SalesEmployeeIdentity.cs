namespace Application.SalesEmployees.Common;

/// <summary>
/// توافق مع دفعة 14/15 — المصدر الحقيقي هو <see cref="SalesWorkforce"/> (تكليف صريح).
/// لا تستخدم role=Mandob كأهلية.
/// </summary>
public static class SalesEmployeeIdentity
{
    public const string ShiftActive = SalesWorkforce.ShiftActive;
    public const string ShiftNoShift = SalesWorkforce.ShiftNoShift;

    public static HashSet<string> AllowedShiftStates => SalesWorkforce.AllowedShiftStates;
    public static HashSet<string> AllowedSorts => SalesWorkforce.AllowedSorts;

    public static IReadOnlyList<int> VisibleBranches(
        IEnumerable<int> employeeBranchIds,
        IReadOnlyList<int> viewerScope)
        => SalesWorkforce.VisibleBranches(employeeBranchIds, viewerScope);
}
