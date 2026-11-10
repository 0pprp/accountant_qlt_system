using Application.FollowerWork.Common;

namespace Application.Committee.Queries.App.GetDecliningCustomers;

/// <summary>
/// تطبيق المتابع — نفس قاعدة DirectorFollowerOpsDeclineRules / FollowerDeclineRules
/// (ليست أيام التسديد الفائتة). يلتقط لقطة اليوم ثم يقارن بلقطات مخزّنة خلال 7 أيام.
/// </summary>
public class GetCommitteeDecliningCustomersHandler
    : IRequestHandler<GetCommitteeDecliningCustomersQuery, Result<List<CommitteeDecliningCustomerDto>>>
{
    private readonly FollowerDecliningDataService _declining;

    public GetCommitteeDecliningCustomersHandler(FollowerDecliningDataService declining)
    {
        _declining = declining;
    }

    public async Task<Result<List<CommitteeDecliningCustomerDto>>> Handle(
        GetCommitteeDecliningCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _declining.GetDecliningForMotabaAsync(request.UserId, cancellationToken);
        if (result.IsFailed)
            return result.Error ?? FollowerWorkErrors.FollowerNotFound;

        var data = result.Data!;
        return data.Items.Select(r => new CommitteeDecliningCustomerDto
        {
            CustomerId = r.CustomerId,
            ListId = r.ListId,
            ListName = r.ListName ?? "",
            FullName = r.CustomerName,
            Phone = r.Phone,
            FromLabel = r.FromLabel,
            ToLabel = r.ToLabel,
            FromScore = r.FromScore,
            ToScore = r.ToScore,
            DaysBetween = r.DaysBetween,
            PriorCaptureDate = r.PriorCaptureDate,
            CurrentCaptureDate = r.CurrentCaptureDate,
            // حقول توافق قديمة — ليست تعريف المتدنية
            MissedDays = r.DaysBetween,
            RiskLabel = r.ToLabel
        }).ToList();
    }
}
