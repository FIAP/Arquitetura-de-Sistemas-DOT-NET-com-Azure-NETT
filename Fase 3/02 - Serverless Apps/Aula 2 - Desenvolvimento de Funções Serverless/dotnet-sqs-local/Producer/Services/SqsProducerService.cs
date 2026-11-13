using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Producer.Models;
using System.Text.Json;

namespace Producer.Services;

public class SqsProducerService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqsProducerService> _logger;
    private string? _queueUrl;

    public SqsProducerService(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<SqsProducerService> logger)
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
            _logger.LogInformation("Fila encontrada: {QueueUrl}", _queueUrl);
            return _queueUrl;
        }
        catch (QueueDoesNotExistException)
        {
            _logger.LogWarning("Fila não encontrada. Criando nova fila: {QueueName}", queueName);
            return await CreateQueueAsync(queueName);
        }
    }

    private async Task<string> CreateQueueAsync(string queueName)
    {
        var createQueueRequest = new CreateQueueRequest
        {
            QueueName = queueName,
            Attributes = new Dictionary<string, string>
            {
                { "VisibilityTimeout", _configuration["SQS:VisibilityTimeoutSeconds"] ?? "30" },
                { "MessageRetentionPeriod", "1209600" } // 14 dias
            }
        };

        var response = await _sqsClient.CreateQueueAsync(createQueueRequest);
        _queueUrl = response.QueueUrl;
        _logger.LogInformation("Nova fila criada: {QueueUrl}", _queueUrl);
        return _queueUrl;
    }

    public async Task<bool> EnviarPedidoAsync(PedidoMessage pedido)
    {
        try
        {
            var queueUrl = await GetQueueUrlAsync();
            
            var messageBody = JsonSerializer.Serialize(pedido, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "TipoPedido", new MessageAttributeValue { StringValue = "Ecommerce", DataType = "String" } },
                    { "ValorTotal", new MessageAttributeValue { StringValue = pedido.ValorTotal.ToString("F2"), DataType = "String" } },
                    { "DataEnvio", new MessageAttributeValue { StringValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), DataType = "String" } }
                }
            };

            var response = await _sqsClient.SendMessageAsync(sendMessageRequest);
            
            _logger.LogInformation("Pedido enviado com sucesso! MessageId: {MessageId}, Pedido: {Pedido}", 
                response.MessageId, pedido);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar pedido: {Pedido}", pedido);
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
                AttributeNames = ["ApproximateNumberOfMessages"]
            });

            if (response.Attributes.TryGetValue("ApproximateNumberOfMessages", out string? count))
            {
                return int.Parse(count);
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem de mensagens na fila");
            return -1;
        }
    }
}
