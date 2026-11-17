using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace S3ProcessorLambda;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<Function> _logger;

    public Function()
    {
        // Configuração para LocalStack
        var config = new AmazonS3Config();
        
        // Se estiver usando LocalStack, configure o endpoint
        var localStackEndpoint = Environment.GetEnvironmentVariable("LOCALSTACK_ENDPOINT") ?? "http://localhost:4566";
        if (!string.IsNullOrEmpty(localStackEndpoint))
        {
            config.ServiceURL = localStackEndpoint;
            config.ForcePathStyle = true;
            config.UseHttp = true;
        }

        _s3Client = new AmazonS3Client(config);
        
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _logger = loggerFactory.CreateLogger<Function>();
    }

    /// <summary>
    /// Função Lambda que processa eventos de upload do S3
    /// </summary>
    /// <param name="evnt">Evento S3 recebido</param>
    /// <param name="context">Contexto da Lambda</param>
    /// <returns></returns>
    public async Task<string> FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        try
        {
            _logger.LogInformation($"Recebidos {evnt.Records.Count} registros para processar");

            foreach (var record in evnt.Records)
            {
                var bucketName = record.S3.Bucket.Name;
                var objectKey = record.S3.Object.Key;

                _logger.LogInformation($"Processando arquivo: {objectKey} do bucket: {bucketName}");

                // Processar o arquivo
                await ProcessS3File(bucketName, objectKey, context);
            }

            return $"Processados {evnt.Records.Count} arquivos com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar eventos S3");
            context.Logger.LogError($"Erro: {ex.Message}");
            throw;
        }
    }

    private async Task ProcessS3File(string bucketName, string objectKey, ILambdaContext context)
    {
        try
        {
            // 1. Ler o conteúdo do arquivo e obter metadados
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            using var response = await _s3Client.GetObjectAsync(getRequest);
            using var responseStream = response.ResponseStream;

            _logger.LogInformation($"Arquivo {objectKey}: Tamanho = {response.ContentLength} bytes, Tipo = {response.Headers.ContentType}");

            // 2. Processar baseado no tipo do arquivo
            await ProcessFileByType(bucketName, objectKey, responseStream, response.Headers.ContentType, context);

            _logger.LogInformation($"Arquivo {objectKey} processado com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao processar arquivo {objectKey} do bucket {bucketName}");
            throw;
        }
    }

    private async Task ProcessFileByType(string bucketName, string objectKey, Stream fileStream, string contentType, ILambdaContext context)
    {
        string processedContent = string.Empty;
        string processedKey = $"processed/{Path.GetFileNameWithoutExtension(objectKey)}_processed{Path.GetExtension(objectKey)}";

        switch (contentType?.ToLower())
        {
            case "text/plain":
            case "text/csv":
                processedContent = await ProcessTextFile(fileStream, context);
                break;
            
            case "application/json":
                processedContent = await ProcessJsonFile(fileStream, context);
                break;
            
            case string ct when ct.StartsWith("image/"):
                await ProcessImageFile(bucketName, objectKey, fileStream, context);
                return; // Imagens não geram conteúdo de texto processado
            
            default:
                _logger.LogInformation($"Tipo de arquivo não suportado para processamento: {contentType}");
                processedContent = ProcessGenericFile(fileStream, context);
                break;
        }

        // Salvar arquivo processado de volta no S3
        if (!string.IsNullOrEmpty(processedContent))
        {
            await SaveProcessedFile(bucketName, processedKey, processedContent, context);
        }
    }

    private async Task<string> ProcessTextFile(Stream fileStream, ILambdaContext context)
    {
        using var reader = new StreamReader(fileStream);
        var content = await reader.ReadToEndAsync();
        
        _logger.LogInformation($"Processando arquivo de texto com {content.Length} caracteres");

        // Exemplo de processamento: converter para maiúsculas e adicionar timestamp
        var processedContent = new StringBuilder();
        processedContent.AppendLine($"=== ARQUIVO PROCESSADO EM {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC ===");
        processedContent.AppendLine($"Linhas originais: {content.Split('\n').Length}");
        processedContent.AppendLine($"Caracteres: {content.Length}");
        processedContent.AppendLine("=== CONTEÚDO PROCESSADO ===");
        processedContent.AppendLine(content.ToUpperInvariant());

        return processedContent.ToString();
    }

    private async Task<string> ProcessJsonFile(Stream fileStream, ILambdaContext context)
    {
        using var reader = new StreamReader(fileStream);
        var jsonContent = await reader.ReadToEndAsync();
        
        _logger.LogInformation($"Processando arquivo JSON com {jsonContent.Length} caracteres");

        // Exemplo: adicionar metadata ao JSON
        var processedJson = $@"{{
  ""processed_at"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}"",
  ""original_size"": {jsonContent.Length},
  ""processor"": ""S3ProcessorLambda"",
  ""original_content"": {jsonContent}
}}";

        return processedJson;
    }

    private async Task ProcessImageFile(string bucketName, string objectKey, Stream fileStream, ILambdaContext context)
    {
        _logger.LogInformation($"Processando arquivo de imagem: {objectKey}");

        // Para imagens, vamos criar um arquivo de metadados
        var imageInfo = new
        {
            processed_at = DateTime.UtcNow,
            original_file = objectKey,
            file_size = fileStream.Length,
            processor = "S3ProcessorLambda",
            file_type = "image",
            processing_notes = "Imagem detectada e catalogada"
        };

        var metadataJson = System.Text.Json.JsonSerializer.Serialize(imageInfo, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        var metadataKey = $"processed/{Path.GetFileNameWithoutExtension(objectKey)}_metadata.json";
        await SaveProcessedFile(bucketName, metadataKey, metadataJson, context);
    }

    private string ProcessGenericFile(Stream fileStream, ILambdaContext context)
    {
        _logger.LogInformation("Processando arquivo genérico");

        var fileInfo = new
        {
            processed_at = DateTime.UtcNow,
            file_size = fileStream.Length,
            processor = "S3ProcessorLambda",
            processing_notes = "Arquivo genérico processado - apenas metadados extraídos"
        };

        return System.Text.Json.JsonSerializer.Serialize(fileInfo, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    private async Task SaveProcessedFile(string bucketName, string objectKey, string content, ILambdaContext context)
    {
        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                ContentBody = content,
                ContentType = "text/plain"
            };

            await _s3Client.PutObjectAsync(putRequest);
            _logger.LogInformation($"Arquivo processado salvo: {objectKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao salvar arquivo processado: {objectKey}");
            throw;
        }
    }
}
