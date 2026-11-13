using Application.Repository;

namespace Infra.Repository;

public class AuthenticationRepository : IAuthenticationRepository
{
    bool IAuthenticationRepository.Authenticate(string email, string password) => true;
}
