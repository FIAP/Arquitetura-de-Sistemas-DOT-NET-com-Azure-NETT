using Application.UseCases;
using Application.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicantionBootstrapper
{
    public static void Register(IServiceCollection services)
    {        
        services.AddTransient<IAuthenticationUseCase, AuthenticationUseCase>();
    }
}
