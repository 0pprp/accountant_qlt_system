using Application.Common.Interfaces;

namespace Application.Users.Commands.Common.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IAuthService _authService;

    public RefreshTokenHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var signInResponse = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (signInResponse.IsFailed)
            return signInResponse.Error!;

        return new RefreshTokenResponse
        {
            AccessToken = signInResponse.Data!.AccessToken,
            RefreshToken = signInResponse.Data.RefreshToken,
            Permissions = signInResponse.Data.Permissions,
            Branches = signInResponse.Data.Branches,
            Roles = signInResponse.Data.Roles
        };
    }
}
