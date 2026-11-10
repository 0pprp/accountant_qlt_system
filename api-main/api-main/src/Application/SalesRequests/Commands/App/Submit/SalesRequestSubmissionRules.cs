using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Common.SeedData;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Commands.App.Submit;

public static class SalesRequestSubmissionRules
{
    public static string? Permission(SalesRequestSourceType source) => source switch
    {
        SalesRequestSourceType.EmployeeSubmitted => Application.Common.SeedData.Permissions.SalesRequest.SubmitEmployee,
        SalesRequestSourceType.Delegate => Application.Common.SeedData.Permissions.SalesRequest.SubmitDelegate,
        SalesRequestSourceType.Follower => Application.Common.SeedData.Permissions.SalesRequest.SubmitFollower,
        _ => null
    };

    /// <summary>أرقام عربية/فارسية → لاتينية، وإزالة المسافات ثم الشكل النهائي.</summary>
    public static string NormalizePhone(string? value)
    {
        var mapped = (value ?? "").Trim().Select(c =>
            c >= '٠' && c <= '٩' ? (char)('0' + c - '٠') :
            c >= '۰' && c <= '۹' ? (char)('0' + c - '۰') : c);
        return new string(mapped.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public static bool ValidPhone(string? value) => Regex.IsMatch(NormalizePhone(value), @"^07[0-9]{9}$");

    public static string Fingerprint(SubmitSalesRequestCommand request) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
            request.Source, request.BranchId, Name = request.CustomerName.Trim(),
            Phone = NormalizePhone(request.CustomerPhone), Address = request.Address.Trim(),
            Product = request.RequestedProduct.Trim(), Note = request.Note?.Trim() ?? ""
        }))));
}
