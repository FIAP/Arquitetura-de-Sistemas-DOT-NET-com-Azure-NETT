using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using FluentValidation;
using TaskManagement.Functions.Interfaces;
using TaskManagement.Functions.Repositories;
using TaskManagement.Functions.Services;
using TaskManagement.Functions.Mapping;
using TaskManagement.Functions.Validators;
using TaskManagement.Functions.Middleware;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseMiddleware<ExceptionHandlingMiddleware>();
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Configuração do Serilog
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(logger);
        });

        // Repositórios e Serviços
        services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
        services.AddScoped<ITaskService, TaskService>();

        // AutoMapper
        services.AddAutoMapper(typeof(TaskMappingProfile));

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

        // Configurações adicionais
        services.AddHttpClient();

        // Middleware personalizado
        services.AddScoped<ExceptionHandlingMiddleware>();
    })
    .Build();

host.Run();
