using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Application.Common.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field?.GetCustomAttribute<DisplayAttribute>() is { Name: { } name })
            return name;

        return value.ToString();
    }
}
