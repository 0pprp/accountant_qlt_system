using Application.Common.Models.SignIn;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    public Task<SignInResponse> SignInAsync(SignInRequest user, CancellationToken cancellationToken = default);
    public Task<Result<SignInResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    public Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}