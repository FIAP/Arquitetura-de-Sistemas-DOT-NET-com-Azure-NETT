using Application.Repository;
using Microsoft.Extensions.DependencyInjection;
using Infra.Repository;

namespace Application;

public static class InfraBootstrapper
{
    public static void Register(IServiceCollection services)
    {        
        services.AddTransient<IAuthenticationRepository, AuthenticationRepository>();
    }
}
