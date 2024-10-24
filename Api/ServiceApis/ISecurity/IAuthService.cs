using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServiceApis.ISecurity
{
    public interface IAuthService
    {
        JwtSecurityToken CreateToken(List<Claim> authClaims);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
    }
}
