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
       services.AddTransient<IInvestimentoService<Acao>,   AcaoService>();
       services.AddTransient<IInvestimentoService<Titulo>, TituloService>();
       services.AddTransient<IInvestimentoService<Imovel>, ImovelService>();
    }
}
