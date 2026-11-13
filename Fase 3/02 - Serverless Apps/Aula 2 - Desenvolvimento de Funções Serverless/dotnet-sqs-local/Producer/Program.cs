using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Producer.Models;
using Producer.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configuração
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// AWS SQS Client configurado para LocalStack
builder.Services.AddSingleton<IAmazonSQS>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    
    var config = new AmazonSQSConfig();
    config.ServiceURL = configuration["AWS:ServiceURL"] ?? "http://localhost:4566";
    config.UseHttp = true;

    // Usar credenciais básicas para LocalStack
    var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");

    return new AmazonSQSClient(credentials, config);
});

// Serviços
builder.Services.AddScoped<SqsProducerService>();

var host = builder.Build();

// Executar o producer
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var producerService = host.Services.GetRequiredService<SqsProducerService>();

logger.LogInformation("=== PRODUCER SQS INICIADO ===");
logger.LogInformation("Pressione qualquer tecla para começar a enviar mensagens, ou 'q' para sair...");

var random = new Random();
var nomes = new[] { "João Silva", "Maria Santos", "Pedro Oliveira", "Ana Costa", "Carlos Ferreira" };
var emails = new[] { "joao@email.com", "maria@email.com", "pedro@email.com", "ana@email.com", "carlos@email.com" };
var produtos = new[] { "Notebook", "Mouse", "Teclado", "Monitor", "Headset", "Webcam", "Mesa", "Cadeira" };
var precos = new[] { 2500.00m, 50.00m, 120.00m, 800.00m, 200.00m, 150.00m, 300.00m, 450.00m };

while (true)
{
    var key = Console.ReadKey(true);
    
    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
    {
        logger.LogInformation("Finalizando producer...");
        break;
    }

    try
    {
        // Gerar pedido aleatório
        var pedido = new PedidoMessage
        {
            NomeCliente = nomes[random.Next(nomes.Length)],
            EmailCliente = emails[random.Next(emails.Length)],
            DataPedido = DateTime.UtcNow
        };

        // Adicionar itens aleatórios
        var numeroItens = random.Next(1, 4);
        for (int i = 0; i < numeroItens; i++)
        {
            var produtoIndex = random.Next(produtos.Length);
            var item = new ItemPedido
            {
                Nome = produtos[produtoIndex],
                Quantidade = random.Next(1, 3),
                PrecoUnitario = precos[produtoIndex]
            };
            pedido.Itens.Add(item);
        }

        pedido.ValorTotal = pedido.Itens.Sum(i => i.Subtotal);

        // Enviar pedido
        var sucesso = await producerService.EnviarPedidoAsync(pedido);
        
        if (sucesso)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            logger.LogInformation("✓ Pedido enviado: {Pedido}", pedido);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            logger.LogError("✗ Falha ao enviar pedido");
            Console.ResetColor();
        }

        // Mostrar estatísticas da fila
        var count = await producerService.GetQueueMessageCountAsync();
        if (count >= 0)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            logger.LogInformation("📊 Mensagens na fila: {Count}", count);
            Console.ResetColor();
        }

        logger.LogInformation("Pressione qualquer tecla para enviar outro pedido, ou 'q' para sair...");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        logger.LogError(ex, "Erro ao processar pedido");
        Console.ResetColor();
    }
}
