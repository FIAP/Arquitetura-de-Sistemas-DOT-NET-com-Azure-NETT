using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Shared.Models;
using Shared.Utils;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace CreateCustomerFunction
{
    public class Function
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly string _tableName;

        public Function()
        {
            var config = new AmazonDynamoDBConfig();
            
            // Configuração para LocalStack
            var localStackEndpoint = Environment.GetEnvironmentVariable("LOCALSTACK_ENDPOINT");
            if (!string.IsNullOrEmpty(localStackEndpoint))
            {
                config.ServiceURL = localStackEndpoint;
                config.UseHttp = true;
            }

            _dynamoDbClient = new AmazonDynamoDBClient(config);
            _tableName = Environment.GetEnvironmentVariable("CUSTOMERS_TABLE") ?? "customers";
        }

        public async Task<CreateCustomerResult> FunctionHandler(CheckCustomerResult checkResult, ILambdaContext context)
        {
            context.Logger.LogInformation($"Iniciando criação de cliente");

            var result = new CreateCustomerResult
            {
                CheckResult = checkResult
            };

            // Verificar se os dados são válidos e se o cliente não existe
            if (checkResult.ValidationResult == null || 
                !checkResult.ValidationResult.IsValid || 
                checkResult.ValidationResult.CustomerData == null)
            {
                context.Logger.LogError("Dados de validação inválidos ou ausentes");
                result.Success = false;
                return result;
            }

            if (checkResult.Exists)
            {
                context.Logger.LogInformation($"Cliente já existe com ID: {checkResult.CustomerId}");
                result.Success = false;
                result.CustomerId = checkResult.CustomerId ?? "";
                return result;
            }

            try
            {
                var customerData = checkResult.ValidationResult.CustomerData;
                var cleanDocument = DocumentValidator.CleanDocument(customerData.Document);
                
                // Criar novo cliente
                var customer = new Customer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = customerData.Name,
                    Email = customerData.Email,
                    Document = cleanDocument,
                    DocumentType = checkResult.ValidationResult.DocumentType,
                    Phone = customerData.Phone,
                    CreatedAt = DateTime.UtcNow,
                    Status = "ACTIVE"
                };

                // Salvar no DynamoDB
                var putRequest = new PutItemRequest
                {
                    TableName = _tableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        ["document"] = new AttributeValue { S = customer.Document },
                        ["id"] = new AttributeValue { S = customer.Id },
                        ["name"] = new AttributeValue { S = customer.Name },
                        ["email"] = new AttributeValue { S = customer.Email },
                        ["documentType"] = new AttributeValue { S = customer.DocumentType },
                        ["phone"] = new AttributeValue { S = customer.Phone },
                        ["createdAt"] = new AttributeValue { S = customer.CreatedAt.ToString("O") },
                        ["status"] = new AttributeValue { S = customer.Status }
                    },
                    // Condição para evitar sobrescrever cliente existente
                    ConditionExpression = "attribute_not_exists(document)"
                };

                await _dynamoDbClient.PutItemAsync(putRequest);

                result.Success = true;
                result.CustomerId = customer.Id;
                result.Customer = customer;

                context.Logger.LogInformation($"Cliente criado com sucesso. ID: {customer.Id}");
            }
            catch (ConditionalCheckFailedException)
            {
                context.Logger.LogWarning("Cliente já existe (condição de concorrência)");
                result.Success = false;
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Erro ao criar cliente: {ex.Message}");
                result.Success = false;
            }

            return result;
        }
    }
}
