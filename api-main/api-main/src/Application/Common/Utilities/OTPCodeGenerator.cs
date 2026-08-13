namespace Application.Common.Utilities;

public static class OtpCodeGenerator
{
    public static string Generate() => Random.Shared.Next(11111, 99999).ToString();
}
