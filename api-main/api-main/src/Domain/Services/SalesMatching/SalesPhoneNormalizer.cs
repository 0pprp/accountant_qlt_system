namespace Domain.Services.SalesMatching;

/// <summary>
/// تطبيع الهاتف للمطابقة فقط — منقول من قواعد SalesHaider دون الاعتماد على خدماته.
/// </summary>
public static class SalesPhoneNormalizer
{
    public static string ForMatch(string? value)
    {
        var digits = DigitsOnly(value);
        if (digits.Length == 0)
            return string.Empty;

        if (digits.StartsWith("00964", StringComparison.Ordinal) && digits.Length >= 15)
            digits = digits[5..];
        else if (digits.StartsWith("964", StringComparison.Ordinal) && digits.Length >= 13)
            digits = digits[3..];

        if (digits.StartsWith('0') && digits.Length == 11)
            digits = digits[1..];

        if (digits.Length == 10 && digits.StartsWith('7'))
            return digits;

        return string.Empty;
    }

    public static bool Matches(string? a, string? b)
    {
        var left = ForMatch(a);
        var right = ForMatch(b);
        return left.Length == 10
               && right.Length == 10
               && string.Equals(left, right, StringComparison.Ordinal);
    }

    public static bool HasEnoughPhoneData(string? value) => ForMatch(value).Length == 10;

    public static string DigitsOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        if (trimmed.Contains('E', StringComparison.OrdinalIgnoreCase) && trimmed.Contains('.'))
        {
            if (double.TryParse(
                    trimmed,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var number)
                && number is >= 7000000000d and <= 7999999999d)
            {
                return ((long)number).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        return new string(trimmed.Where(char.IsDigit).ToArray());
    }
}
