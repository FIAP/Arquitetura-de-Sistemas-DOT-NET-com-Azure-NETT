using Microsoft.Extensions.DependencyInjection;

public static class ServiceConfiguration
{
    public static ServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        return serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Registrar serviços no contêiner
        services.AddTransient<IRepository<Acao>, Repository<Acao>>();
        services.AddTransient<AcaoRetornoService>();
        services.AddTransient<AcaoDescricaoService>();
    }
}
