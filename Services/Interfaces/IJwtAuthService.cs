using Common.Models.InputDTOs;
using Common.Models.JWT;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Services.Interfaces
{
    public interface IJwtAuthService
    {
        JwtAuthResult GenerateTokens(LoginUserWithRolesDto userDto);
    }
}
