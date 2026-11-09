using Amazon.SQS;
using Consumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
builder.Services.AddScoped<SqsConsumerService>();

var host = builder.Build();

// Executar o consumer
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var consumerService = host.Services.GetRequiredService<SqsConsumerService>();

logger.LogInformation("=== CONSUMER SQS INICIADO ===");
logger.LogInformation("Aguardando mensagens... Pressione Ctrl+C para parar.");

// Token de cancelamento para parar o consumer
using var cancellationTokenSource = new CancellationTokenSource();

// Configurar handler para Ctrl+C
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    logger.LogInformation("Parando consumer...");
    cancellationTokenSource.Cancel();
};

var totalMensagensProcessadas = 0;

try
{
    while (!cancellationTokenSource.Token.IsCancellationRequested)
    {
        try
        {
            // Verificar estatísticas da fila periodicamente
            await consumerService.GetQueueMessageCountAsync();

            // Receber mensagens da fila
            var mensagens = await consumerService.ReceberMensagensAsync();

            if (mensagens.Count == 0)
            {
                // Não há mensagens, aguardar um pouco antes de tentar novamente
                logger.LogInformation("⏳ Nenhuma mensagem disponível. Aguardando...");
                continue;
            }

            // Processar cada mensagem recebida
            foreach (var mensagem in mensagens)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                logger.LogInformation("📨 Nova mensagem recebida - MessageId: {MessageId}", mensagem.MessageId);

                // Processar o pedido
                var processadoComSucesso = await consumerService.ProcessarPedidoAsync(mensagem);

                if (processadoComSucesso)
                {
                    // Se processado com sucesso, deletar da fila
                    var deletado = await consumerService.DeletarMensagemAsync(mensagem);
                    
                    if (deletado)
                    {
                        totalMensagensProcessadas++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        logger.LogInformation("✅ Mensagem processada e removida da fila. Total processadas: {Total}", 
                            totalMensagensProcessadas);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        logger.LogWarning("⚠️  Mensagem processada mas não removida da fila");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.LogError("❌ Falha ao processar mensagem. Mensagem permanecerá na fila para reprocessamento.");
                    Console.ResetColor();
                }

                Console.WriteLine(new string('-', 80));
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            logger.LogError(ex, "Erro no loop principal do consumer");
            Console.ResetColor();
            
            // Aguardar antes de tentar novamente
            await Task.Delay(5000, cancellationTokenSource.Token);
        }
    }
}
catch (OperationCanceledException)
{
    // Consumer foi cancelado pelo usuário
}
finally
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    logger.LogInformation("Consumer finalizado. Total de mensagens processadas: {Total}", totalMensagensProcessadas);
    Console.ResetColor();
}
