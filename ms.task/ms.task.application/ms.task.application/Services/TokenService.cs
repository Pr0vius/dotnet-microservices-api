using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ms.task.application.Services
{
    public class TokenService
    {
        public ClaimsPrincipal? DecodeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("El token no puede ser nulo o vacío.");

            var splitToken = token.StartsWith("Bearer ") ? token.Substring(7) : token;
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(splitToken))
                return null;

            var jwtToken = handler.ReadJwtToken(splitToken);
            var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "JWT");

            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}

