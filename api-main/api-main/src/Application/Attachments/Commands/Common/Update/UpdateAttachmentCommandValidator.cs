using Application.Common.Utilities;
using ByteSizeLib;

namespace Application.Attachments.Commands.Common.Update;

public class UpdateAttachmentCommandValidator : AbstractValidator<UpdateAttachmentCommand>
{
    public UpdateAttachmentCommandValidator()
    {
        RuleFor(x => x.File)
            .NotEmpty()
            .WithMessage("لا يمكن أن تكون الصورة فارغة.")
            .Must(x => AttachmentUtility.ImageContentTypes.Contains(x.ContentType))
            .WithMessage("تنسيق الصورة غير صحيح.")
            .Must(x => ByteSize.FromBytes(x.Length).MegaBytes <= 2)
            .WithMessage("لا يمكن أن يتجاوز حجم الصورة 2 میغابایت.");
    }
}