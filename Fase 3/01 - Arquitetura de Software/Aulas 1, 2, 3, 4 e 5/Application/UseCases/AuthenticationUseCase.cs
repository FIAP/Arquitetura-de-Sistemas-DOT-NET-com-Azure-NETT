using Application.Repository;

namespace Application.UseCases;

public class AuthenticationUseCase : IAuthenticationUseCase
{
    public readonly IAuthenticationRepository _authenticationRepository;

    public AuthenticationUseCase(IAuthenticationRepository authenticationRepository)
    {
        _authenticationRepository = authenticationRepository;
    }

    public bool Authenticate(string email, string password)
    {
        return _authenticationRepository.Authenticate(email, password);

    }
}
