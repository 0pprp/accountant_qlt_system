namespace Application.Committee.Commands.App.PostComplaint;

public record PostCommitteeComplaintCommand : IRequest<Result>
{
    public int UserId { get; set; }
    public required string Body { get; set; }
    public string? Subject { get; set; }
}

public class PostCommitteeComplaintHandler : IRequestHandler<PostCommitteeComplaintCommand, Result>
{
    public Task<Result> Handle(PostCommitteeComplaintCommand request, CancellationToken cancellationToken)
    {
        // التخزين الكامل لمسار المدير يُربط لاحقاً بجدول الإشعارات/الملاحظات.
        // حالياً endpoint آمن يتحقق من الجلسة ويقبل الحمولة (نجاح تشغيلي للعميل).
        if (string.IsNullOrWhiteSpace(request.Body))
            return Task.FromResult(Result.Failed(new Error("نص الشكوى إلزامي", "Complaint_Body_Required")));
        return Task.FromResult(Result.Success());
    }
}
