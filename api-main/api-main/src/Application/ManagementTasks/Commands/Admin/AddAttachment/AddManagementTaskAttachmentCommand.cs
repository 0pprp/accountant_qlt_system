using Application.ManagementTasks.Common;
using Microsoft.AspNetCore.Http;

namespace Application.ManagementTasks.Commands.Admin.AddAttachment;

public record AddManagementTaskAttachmentCommand : IRequest<Result<ManagementTaskAttachmentDto>>
{
    public int TaskId { get; init; }
    public required IFormFile File { get; init; }
}

public class AddManagementTaskAttachmentCommandValidator : AbstractValidator<AddManagementTaskAttachmentCommand>
{
    public AddManagementTaskAttachmentCommandValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.File)
            .NotNull().WithMessage("الملف مطلوب.")
            .Must(x => x.Length > 0).WithMessage("الملف مطلوب.")
            .Must(x => x.Length <= ManagementTaskMapper.MaxAttachmentBytes).WithMessage("حجم الملف يتجاوز 20 ميغابايت")
            .Must(x => ManagementTaskMapper.IsAllowedExtension(Path.GetExtension(x.FileName)))
            .WithMessage("صيغة الملف غير مدعومة (صور، PDF، Word، Excel).")
            .Must(x => x.FileName.Length <= 200).WithMessage("اسم الملف يجب ألا يتجاوز 200 حرف");
    }
}
