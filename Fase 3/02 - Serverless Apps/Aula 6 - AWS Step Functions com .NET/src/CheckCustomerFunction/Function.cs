using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Shared.Models;
using Shared.Utils;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace CheckCustomerFunction
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

        public async Task<CheckCustomerResult> FunctionHandler(ValidationResult validationResult, ILambdaContext context)
        {
            context.Logger.LogInformation($"Verificando se cliente já existe: {validationResult.CustomerData?.Document}");

            var result = new CheckCustomerResult
            {
                ValidationResult = validationResult
            };

            if (!validationResult.IsValid || validationResult.CustomerData == null)
            {
                context.Logger.LogInformation("Dados inválidos, pulando verificação no DynamoDB");
                return result;
            }

            try
            {
                var cleanDocument = DocumentValidator.CleanDocument(validationResult.CustomerData.Document);

                // Buscar no DynamoDB usando o documento como chave de partição
                var request = new GetItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        ["document"] = new AttributeValue { S = cleanDocument }
                    }
                };

                var response = await _dynamoDbClient.GetItemAsync(request);

                if (response.Item?.Count > 0)
                {
                    result.Exists = true;
                    
                    if (response.Item.TryGetValue("id", out var idAttribute) && idAttribute.S != null)
                    {
                        result.CustomerId = idAttribute.S;
                    }

                    context.Logger.LogInformation($"Cliente já existe com ID: {result.CustomerId}");
                }
                else
                {
                    result.Exists = false;
                    context.Logger.LogInformation("Cliente não encontrado, pode prosseguir com criação");
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Erro ao consultar DynamoDB: {ex.Message}");
                
                // Em caso de erro na consulta, assumimos que o cliente não existe
                // para não bloquear o fluxo (pode ser configurado diferente dependendo dos requisitos)
                result.Exists = false;
            }

            return result;
        }
    }
}
