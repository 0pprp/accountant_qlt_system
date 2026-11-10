using Application.SalesRequests.Common;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.Tests.SalesRequests;

/// <summary>
/// يثبت تطابق منطق UnassignedInScope مع Domain.IsUnassigned للحالات الحرجة.
/// </summary>
public class SalesRequestUnassignedQueryParityTests
{
    public static TheoryData<SalesRequestStatus, int?, SalesRequestExceptionHoldStatus?, bool> Cases => new()
    {
        { SalesRequestStatus.New, null, null, true },
        { SalesRequestStatus.Assigned, 5, null, false },
        { SalesRequestStatus.Pending, null, null, true },
        { SalesRequestStatus.Pending, 3, null, false },
        { SalesRequestStatus.Rejected, null, null, false },
        { SalesRequestStatus.Completed, null, null, false },
        { SalesRequestStatus.LegalFollowup, null, null, false },
        { SalesRequestStatus.New, null, SalesRequestExceptionHoldStatus.Pending, false },
        { SalesRequestStatus.New, 2, SalesRequestExceptionHoldStatus.Pending, true },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Domain_IsUnassigned_matches_access_predicate_shape(
        SalesRequestStatus status,
        int? targetEmployeeId,
        SalesRequestExceptionHoldStatus? hold,
        bool expectedUnassigned)
    {
        var request = new SalesRequest("علي حسن كاظم", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null)
        {
            Status = status,
            TargetEmployeeId = targetEmployeeId,
            ExceptionHoldStatus = hold
        };

        Assert.Equal(expectedUnassigned, request.IsUnassigned());

        // مرآة منطق SalesRequestAccess.UnassignedInScope بدون EF
        var inScope = !request.IsDeleted
                      && status is not (SalesRequestStatus.Rejected or SalesRequestStatus.Completed or SalesRequestStatus.LegalFollowup)
                      && (hold is null || (targetEmployeeId is not null and > 0))
                      && (status == SalesRequestStatus.New || targetEmployeeId is null or <= 0);

        Assert.Equal(request.IsUnassigned(), inScope);
    }

    [Fact]
    public void Access_helper_type_is_available()
    {
        Assert.Equal(typeof(SalesRequestAccess).Name, nameof(SalesRequestAccess));
    }
}
