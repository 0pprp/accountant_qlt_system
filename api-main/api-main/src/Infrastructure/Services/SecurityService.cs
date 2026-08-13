using Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;
public class SecurityService : ISecurityService
{
    public string GetSha256Hash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(inputBytes);
        StringBuilder sb = new();

        foreach (var hashByte in hashBytes)
        {
            sb.Append(hashByte.ToString("x2"));
        }

        return sb.ToString();
    }
    public string CreateRandomString()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}
