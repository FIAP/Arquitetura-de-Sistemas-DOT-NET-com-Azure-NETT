namespace Application.Repository;

public interface IAuthenticationRepository
{
    bool Authenticate(string email, string password);
}

