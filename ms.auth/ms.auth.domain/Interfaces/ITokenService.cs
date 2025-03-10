using ms.user.domain.Entities;

namespace ms.auth.domain.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(Guid id, string username, Roles role);
    }
}
