using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TaskManagement.Functions.Interfaces;
using System.Text.Json;

namespace TaskManagement.Functions.Functions
{
    public class BlobFunctions
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<BlobFunctions> _logger;

        public BlobFunctions(ITaskService taskService, ILogger<BlobFunctions> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [Function("ProcessTaskAttachmentFunction")]
        public async Task ProcessTaskAttachmentFunction(
            [BlobTrigger("task-attachments/{name}", Connection = "AzureWebJobsStorage")] byte[] blobContent,
            string name)
        {
            _logger.LogInformation("Processando anexo de tarefa: {BlobName}, Tamanho: {Size} bytes", name, blobContent.Length);

            try
            {
                // Extrair o ID da tarefa do nome do arquivo (formato: taskId_filename.ext)
                var parts = name.Split('_', 2);
                if (parts.Length != 2)
                {
                    _logger.LogWarning("Nome do arquivo de anexo inválido: {BlobName}. Formato esperado: taskId_filename.ext", name);
                    return;
                }

                var taskId = parts[0];
                var fileName = parts[1];

                var task = await _taskService.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("Tarefa {TaskId} não encontrada para anexo {FileName}", taskId, fileName);
                    return;
                }

                // Processar anexo baseado no tipo de arquivo
                var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                await ProcessAttachmentByType(taskId, fileName, fileExtension, blobContent);

                _logger.LogInformation("Anexo {FileName} processado com sucesso para tarefa {TaskId}", fileName, taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar anexo {BlobName}", name);
                throw;
            }
        }

        private async Task ProcessAttachmentByType(string taskId, string fileName, string fileExtension, byte[] content)
        {
            switch (fileExtension)
            {
                case ".txt":
                case ".md":
                    await ProcessTextAttachment(taskId, fileName, content);
                    break;
                
                case ".json":
                    await ProcessJsonAttachment(taskId, fileName, content);
                    break;
                
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                    await ProcessImageAttachment(taskId, fileName, content);
                    break;
                
                case ".pdf":
                    await ProcessPdfAttachment(taskId, fileName, content);
                    break;
                
                default:
                    await ProcessGenericAttachment(taskId, fileName, content);
                    break;
            }
        }

        private async Task ProcessTextAttachment(string taskId, string fileName, byte[] content)
        {
            _logger.LogInformation("Processando anexo de texto para tarefa {TaskId}: {FileName}", taskId, fileName);
            
            var textContent = System.Text.Encoding.UTF8.GetString(content);
            
            // Verificar se é uma descrição adicional ou notas
            if (fileName.Contains("description") || fileName.Contains("notes"))
            {
                // Atualizar a descrição da tarefa com o conteúdo do arquivo
                var task = await _taskService.GetTaskByIdAsync(taskId);
                if (task != null)
                {
                    var updateDto = new Models.DTOs.UpdateTaskDto
                    {
                        Description = task.Description + "\n\nAnexo: " + textContent
                    };
                    
                    await _taskService.UpdateTaskAsync(taskId, updateDto);
                    _logger.LogInformation("Descrição da tarefa {TaskId} atualizada com conteúdo do anexo", taskId);
                }
            }

            await Task.Delay(100); // Simular processamento
        }

        private async Task ProcessJsonAttachment(string taskId, string fileName, byte[] content)
        {
            _logger.LogInformation("Processando anexo JSON para tarefa {TaskId}: {FileName}", taskId, fileName);
            
            try
            {
                var jsonContent = System.Text.Encoding.UTF8.GetString(content);
                var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                // Verificar se contém tags ou metadados
                if (jsonData.TryGetProperty("tags", out var tagsElement) && tagsElement.ValueKind == JsonValueKind.Array)
                {
                    var tags = tagsElement.EnumerateArray()
                        .Where(t => t.ValueKind == JsonValueKind.String)
                        .Select(t => t.GetString()!)
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();

                    if (tags.Any())
                    {
                        var updateDto = new Models.DTOs.UpdateTaskDto
                        {
                            Tags = tags
                        };
                        
                        await _taskService.UpdateTaskAsync(taskId, updateDto);
                        _logger.LogInformation("Tags da tarefa {TaskId} atualizadas a partir do anexo JSON", taskId);
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Anexo JSON inválido para tarefa {TaskId}: {FileName}", taskId, fileName);
            }

            await Task.Delay(100); // Simular processamento
        }

        private async Task ProcessImageAttachment(string taskId, string fileName, byte[] content)
        {
            _logger.LogInformation("Processando anexo de imagem para tarefa {TaskId}: {FileName}, Tamanho: {Size} bytes", 
                taskId, fileName, content.Length);
            
            // Simular processamento de imagem (redimensionamento, OCR, etc.)
            if (content.Length > 1024 * 1024) // > 1MB
            {
                _logger.LogInformation("Imagem grande detectada para tarefa {TaskId}. Aplicando compressão.", taskId);
            }

            await Task.Delay(200); // Simular processamento
        }

        private async Task ProcessPdfAttachment(string taskId, string fileName, byte[] content)
        {
            _logger.LogInformation("Processando anexo PDF para tarefa {TaskId}: {FileName}, Tamanho: {Size} bytes", 
                taskId, fileName, content.Length);
            
            // Simular extração de texto do PDF
            _logger.LogInformation("Extraindo texto do PDF para tarefa {TaskId}", taskId);
            
            await Task.Delay(300); // Simular processamento
        }

        private async Task ProcessGenericAttachment(string taskId, string fileName, byte[] content)
        {
            _logger.LogInformation("Processando anexo genérico para tarefa {TaskId}: {FileName}, Tipo: {Extension}", 
                taskId, fileName, Path.GetExtension(fileName));
            
            // Armazenar metadados do arquivo
            var metadata = new
            {
                FileName = fileName,
                Size = content.Length,
                UploadedAt = DateTime.UtcNow,
                TaskId = taskId
            };

            _logger.LogInformation("Metadados do anexo armazenados: {@Metadata}", metadata);
            
            await Task.Delay(100); // Simular processamento
        }
    }
}
