using Domain.Entities.UserAggregate.Enums;

namespace Application.Common.Extensions;

public static class UserCreationStepExtensions
{
    public static string ToArabicString(this UserCreationStep creationStep)
    {
        return creationStep switch
        {
            UserCreationStep.PersonalDocuments => "المستندات الشخصية",
            UserCreationStep.SalaryDetail => "تفاصيل الراتب",
            UserCreationStep.Permissions => "الصلاحيات",
            UserCreationStep.Completed => "مكتمل",
            _ => throw new ArgumentOutOfRangeException(nameof(creationStep), creationStep, null)
        };
    }
}
