using dotnet_grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection(); // Adicione esta linha

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService(); // Mapeia o serviço de reflexão
}

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<InvestmentService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
