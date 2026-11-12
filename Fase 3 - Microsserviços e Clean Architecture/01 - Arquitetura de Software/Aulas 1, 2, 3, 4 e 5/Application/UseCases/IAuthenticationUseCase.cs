namespace Application.UseCases;

public interface IAuthenticationUseCase
{
    bool Authenticate(string email, string password);
}
