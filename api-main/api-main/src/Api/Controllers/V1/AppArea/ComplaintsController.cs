using Application.Complaints.Commands;
using Application.Complaints.Common;

namespace Api.Controllers.V1.AppArea;

/// <summary>
/// إرسال شكوى من التطبيقات — المصدر يُشتق على السيرفر (sales-employee عند تكليف نشط).
/// ليست AppNote.
/// </summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/Complaints")]
public class ComplaintsController : ApiController
{
    private readonly IMediator _mediator;

    public ComplaintsController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.Complaint.Submit)]
    [HttpPost]
    public async Task<ActionResult<ComplaintDto>> Submit([FromBody] SubmitBody? body)
    {
        body ??= new SubmitBody();
        return (await _mediator.Send(new SubmitComplaintCommand
        {
            Body = body.Body ?? "",
            Subject = body.Subject,
            // يُتجاهل لصالح الاشتقاق من الجلسة+التكليف في الـhandler.
            SourceApp = "app"
        }, CancellationToken)).ToHttpResponse();
    }

    public sealed class SubmitBody
    {
        public string? Body { get; set; }
        public string? Subject { get; set; }
    }
}
