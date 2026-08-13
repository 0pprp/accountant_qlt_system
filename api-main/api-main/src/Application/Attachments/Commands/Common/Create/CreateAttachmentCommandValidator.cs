using Application.Common.Utilities;
using ByteSizeLib;

namespace Application.Attachments.Commands.Common.Create;

public class CreateAttachmentCommandValidator : AbstractValidator<CreateAttachmentCommand>
{
    public CreateAttachmentCommandValidator()
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