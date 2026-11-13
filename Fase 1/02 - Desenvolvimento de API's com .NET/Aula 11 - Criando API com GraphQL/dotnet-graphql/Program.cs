using dotnet_graphql.GraphQL;
using dotnet_graphql.Services;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region [Registry]
builder.Services.AddSingleton<InvestmentService>();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<InvestmentQuery>()
    .AddMutationType<InvestmentMutation>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UsePlayground(new PlaygroundOptions
    {
        Path = "/graphql/playground"
    });
}
app.MapGet("/health", () => Results.Ok("API is running"));

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
});

app.Run();
