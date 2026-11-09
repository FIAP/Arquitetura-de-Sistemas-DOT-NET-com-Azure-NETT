using Amazon.SQS;
using Amazon.SQS.Model;
using Consumer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Consumer.Services;

public class SqsConsumerService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqsConsumerService> _logger;
    private string? _queueUrl;

    public SqsConsumerService(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<SqsConsumerService> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<string> GetQueueUrlAsync()
    {
        if (_queueUrl != null)
            return _queueUrl;

        var queueName = _configuration["SQS:QueueName"] ?? "mensagens-exemplo";
        
        try
        {
            var response = await _sqsClient.GetQueueUrlAsync(queueName);
            _queueUrl = response.QueueUrl;
            _logger.LogInformation("Conectado à fila: {QueueUrl}", _queueUrl);
            return _queueUrl;
        }
        catch (QueueDoesNotExistException)
        {
            _logger.LogError("Fila não encontrada: {QueueName}. Execute o Producer primeiro para criar a fila.", queueName);
            throw;
        }
    }

    public async Task<List<Message>> ReceberMensagensAsync()
    {
        try
        {
            var queueUrl = await GetQueueUrlAsync();
            var maxMessages = int.Parse(_configuration["SQS:MaxMessages"] ?? "10");
            var waitTimeSeconds = int.Parse(_configuration["SQS:WaitTimeSeconds"] ?? "20");

            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = maxMessages,
                WaitTimeSeconds = waitTimeSeconds,
                AttributeNames = ["All"],
                MessageAttributeNames = ["All"]
            };

            var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);
            
            if (response.Messages.Count > 0)
            {
                _logger.LogInformation("📨 Recebidas {Count} mensagens da fila", response.Messages.Count);
            }
            
            return response.Messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao receber mensagens da fila");
            return new List<Message>();
        }
    }

    public async Task<bool> ProcessarPedidoAsync(Message message)
    {
        try
        {
            // Deserializar a mensagem
            var pedido = JsonSerializer.Deserialize<PedidoMessage>(message.Body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (pedido == null)
            {
                _logger.LogWarning("Mensagem com formato inválido recebida. MessageId: {MessageId}", message.MessageId);
                return false;
            }

            _logger.LogInformation("🔄 Processando pedido: {Pedido}", pedido);

            // Simular processamento do pedido
            await SimularProcessamentoPedido(pedido);

            // Log dos atributos da mensagem
            if (message.MessageAttributes.Count > 0)
            {
                _logger.LogInformation("📋 Atributos da mensagem:");
                foreach (var attr in message.MessageAttributes)
                {
                    _logger.LogInformation("  - {Key}: {Value}", attr.Key, attr.Value.StringValue);
                }
            }

            _logger.LogInformation("✅ Pedido processado com sucesso: {PedidoId}", pedido.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pedido. MessageId: {MessageId}", message.MessageId);
            return false;
        }
    }

    private async Task SimularProcessamentoPedido(PedidoMessage pedido)
    {
        // Simular validação do pedido
        _logger.LogInformation("🔍 Validando pedido {PedidoId}...", pedido.Id);
        await Task.Delay(500);

        // Simular verificação de estoque
        _logger.LogInformation("📦 Verificando estoque para {Count} itens...", pedido.Itens.Count);
        await Task.Delay(800);

        // Simular processamento de pagamento
        _logger.LogInformation("💳 Processando pagamento de R$ {Valor}...", pedido.ValorTotal);
        await Task.Delay(1000);

        // Simular envio de email
        _logger.LogInformation("📧 Enviando confirmação para {Email}...", pedido.EmailCliente);
        await Task.Delay(300);

        var delaySeconds = int.Parse(_configuration["SQS:ProcessingDelaySeconds"] ?? "2");
        if (delaySeconds > 0)
        {
            await Task.Delay(delaySeconds * 1000);
        }
    }

    public async Task<bool> DeletarMensagemAsync(Message message)
    {
        try
        {
            var queueUrl = await GetQueueUrlAsync();

            await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
            {
                QueueUrl = queueUrl,
                ReceiptHandle = message.ReceiptHandle
            });

            _logger.LogInformation("🗑️  Mensagem deletada da fila. MessageId: {MessageId}", message.MessageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar mensagem. MessageId: {MessageId}", message.MessageId);
            return false;
        }
    }

    public async Task<int> GetQueueMessageCountAsync()
    {
        try
        {
            var queueUrl = await GetQueueUrlAsync();

            var response = await _sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest
            {
                QueueUrl = queueUrl,
                AttributeNames = ["ApproximateNumberOfMessages", "ApproximateNumberOfMessagesNotVisible"]
            });

            var visibleCount = 0;
            var processingCount = 0;

            if (response.Attributes.TryGetValue("ApproximateNumberOfMessages", out string? visible))
            {
                visibleCount = int.Parse(visible);
            }

            if (response.Attributes.TryGetValue("ApproximateNumberOfMessagesNotVisible", out string? processing))
            {
                processingCount = int.Parse(processing);
            }

            _logger.LogInformation("📊 Estatísticas da fila - Disponíveis: {Visible}, Em processamento: {Processing}", 
                visibleCount, processingCount);

            return visibleCount + processingCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas da fila");
            return -1;
        }
    }
}
