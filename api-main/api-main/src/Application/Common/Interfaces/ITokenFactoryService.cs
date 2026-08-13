using Application.Common.Models.TokenFactory;

namespace Application.Common.Interfaces;

public interface ITokenFactoryService
{
    public string CreateUserJwt(CreateJwtDto createJwtDto);
    public string CreateRefreshToken();
}