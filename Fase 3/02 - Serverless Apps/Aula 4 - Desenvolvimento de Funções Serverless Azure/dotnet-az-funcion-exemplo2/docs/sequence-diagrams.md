# Diagramas de Sequência - Sistema de Gestão de Tarefas

## HTTP Functions (CRUD de Tarefas)

### 1. GET /api/tasks - Listar Todas as Tarefas

```mermaid
sequenceDiagram
    participant C as Cliente
    participant F as TaskFunctions
    participant S as TaskService
    participant R as InMemoryTaskRepository
    participant M as AutoMapper
    
    C->>F: GET /api/tasks
    F->>S: GetAllTasksAsync()
    S->>R: GetAllAsync()
    R-->>S: IEnumerable<TaskModel>
    S->>M: Map<IEnumerable<TaskResponseDto>>(tasks)
    M-->>S: IEnumerable<TaskResponseDto>
    S-->>F: IEnumerable<TaskResponseDto>
    F->>F: CreateJsonResponse(ApiResponse.Success)
    F-->>C: HTTP 200 + JSON Response
```

### 2. GET /api/tasks/{id} - Obter Tarefa por ID

```mermaid
sequenceDiagram
    participant C as Cliente
    participant F as TaskFunctions
    participant S as TaskService
    participant R as InMemoryTaskRepository
    participant M as AutoMapper
    
    C->>F: GET /api/tasks/{id}
    F->>F: Validate ID
    alt ID válido
        F->>S: GetTaskByIdAsync(id)
        S->>R: GetByIdAsync(id)
        alt Tarefa encontrada
            R-->>S: TaskModel
            S->>M: Map<TaskResponseDto>(task)
            M-->>S: TaskResponseDto
            S-->>F: TaskResponseDto
            F->>F: CreateJsonResponse(ApiResponse.Success)
            F-->>C: HTTP 200 + TaskResponseDto
        else Tarefa não encontrada
            R-->>S: null
            S-->>F: null
            F->>F: CreateJsonResponse(ApiResponse.Failure)
            F-->>C: HTTP 404 + Error Message
        end
    else ID inválido
        F->>F: CreateJsonResponse(ApiResponse.Failure)
        F-->>C: HTTP 400 + Error Message
    end
```

### 3. POST /api/tasks - Criar Nova Tarefa

```mermaid
sequenceDiagram
    participant C as Cliente
    participant F as TaskFunctions
    participant V as CreateTaskDtoValidator
    participant S as TaskService
    participant M as AutoMapper
    participant R as InMemoryTaskRepository
    
    C->>F: POST /api/tasks + CreateTaskDto
    F->>F: Deserialize JSON
    alt JSON válido
        F->>V: ValidateAsync(createTaskDto)
        alt Validação passou
            V-->>F: IsValid = true
            F->>S: CreateTaskAsync(createTaskDto)
            S->>M: Map<TaskModel>(createTaskDto)
            M-->>S: TaskModel
            S->>R: CreateAsync(taskModel)
            R->>R: Generate ID, Set CreatedAt
            R-->>S: TaskModel (with ID)
            S->>M: Map<TaskResponseDto>(createdTask)
            M-->>S: TaskResponseDto
            S-->>F: TaskResponseDto
            F->>F: CreateJsonResponse(ApiResponse.Success)
            F-->>C: HTTP 201 + TaskResponseDto
        else Validação falhou
            V-->>F: IsValid = false + Errors
            F->>F: CreateJsonResponse(ApiResponse.Failure)
            F-->>C: HTTP 400 + Validation Errors
        end
    else JSON inválido
        F->>F: CreateJsonResponse(ApiResponse.Failure)
        F-->>C: HTTP 400 + JSON Error
    end
```

### 4. PUT /api/tasks/{id} - Atualizar Tarefa

```mermaid
sequenceDiagram
    participant C as Cliente
    participant F as TaskFunctions
    participant V as UpdateTaskDtoValidator
    participant S as TaskService
    participant R as InMemoryTaskRepository
    participant M as AutoMapper
    
    C->>F: PUT /api/tasks/{id} + UpdateTaskDto
    F->>F: Validate ID & Deserialize JSON
    alt ID e JSON válidos
        F->>V: ValidateAsync(updateTaskDto)
        alt Validação passou
            V-->>F: IsValid = true
            F->>S: UpdateTaskAsync(id, updateTaskDto)
            S->>R: GetByIdAsync(id)
            alt Tarefa existe
                R-->>S: TaskModel (existing)
                S->>S: Apply partial updates
                S->>R: UpdateAsync(id, updatedTask)
                R->>R: Preserve ID/CreatedAt, Set CompletedAt if Completed
                R-->>S: TaskModel (updated)
                S->>M: Map<TaskResponseDto>(updatedTask)
                M-->>S: TaskResponseDto
                S-->>F: TaskResponseDto
                F->>F: CreateJsonResponse(ApiResponse.Success)
                F-->>C: HTTP 200 + TaskResponseDto
            else Tarefa não existe
                R-->>S: null
                S-->>F: null
                F->>F: CreateJsonResponse(ApiResponse.Failure)
                F-->>C: HTTP 404 + Error Message
            end
        else Validação falhou
            V-->>F: IsValid = false + Errors
            F->>F: CreateJsonResponse(ApiResponse.Failure)
            F-->>C: HTTP 400 + Validation Errors
        end
    else ID ou JSON inválido
        F->>F: CreateJsonResponse(ApiResponse.Failure)
        F-->>C: HTTP 400 + Error Message
    end
```

### 5. DELETE /api/tasks/{id} - Deletar Tarefa

```mermaid
sequenceDiagram
    participant C as Cliente
    participant F as TaskFunctions
    participant S as TaskService
    participant R as InMemoryTaskRepository
    
    C->>F: DELETE /api/tasks/{id}
    F->>F: Validate ID
    alt ID válido
        F->>S: DeleteTaskAsync(id)
        S->>R: DeleteAsync(id)
        alt Tarefa encontrada e deletada
            R-->>S: true
            S-->>F: true
            F->>F: CreateJsonResponse(ApiResponse.Success)
            F-->>C: HTTP 200 + Success Message
        else Tarefa não encontrada
            R-->>S: false
            S-->>F: false
            F->>F: CreateJsonResponse(ApiResponse.Failure)
            F-->>C: HTTP 404 + Error Message
        end
    else ID inválido
        F->>F: CreateJsonResponse(ApiResponse.Failure)
        F-->>C: HTTP 400 + Error Message
    end
```

### 6. GET /api/tasks/status/{status} - Filtrar por Status

```mermaid
sequenceDiagram
    participant C as Cliente
    participant F as TaskFunctions
    participant S as TaskService
    participant R as InMemoryTaskRepository
    participant M as AutoMapper
    
    C->>F: GET /api/tasks/status/{status}
    F->>F: Parse & Validate TaskStatus enum
    alt Status válido
        F->>S: GetTasksByStatusAsync(taskStatus)
        S->>R: GetByStatusAsync(taskStatus)
        R-->>S: IEnumerable<TaskModel> (filtered)
        S->>M: Map<IEnumerable<TaskResponseDto>>(tasks)
        M-->>S: IEnumerable<TaskResponseDto>
        S-->>F: IEnumerable<TaskResponseDto>
        F->>F: CreateJsonResponse(ApiResponse.Success)
        F-->>C: HTTP 200 + Filtered Tasks
    else Status inválido
        F->>F: CreateJsonResponse(ApiResponse.Failure)
        F-->>C: HTTP 400 + Invalid Status Error
    end
```

## Health Check Functions

### 7. GET /api/health - Health Check Básico

```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as HealthCheckFunction
    participant S as TaskService
    
    C->>H: GET /api/health
    H->>S: GetAllTasksAsync()
    alt Serviço funcionando
        S-->>H: IEnumerable<TaskResponseDto>
        H->>H: Count tasks & Build HealthStatus
        H->>H: Set status = "Healthy"
        H->>H: CreateJsonResponse(healthStatus)
        H-->>C: HTTP 200 + HealthStatus
    else Serviço com erro
        S-->>H: Exception
        H->>H: Build HealthStatus with error
        H->>H: Set status = "Unhealthy"
        H->>H: CreateJsonResponse(healthStatus)
        H-->>C: HTTP 503 + Error HealthStatus
    end
```

### 8. GET /api/health/detailed - Health Check Detalhado

```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as HealthCheckFunction
    participant S as TaskService
    
    C->>H: GET /api/health/detailed
    H->>H: Initialize DetailedHealthStatus
    H->>S: GetAllTasksAsync()
    alt Todos os serviços OK
        S-->>H: IEnumerable<TaskResponseDto>
        H->>H: Measure TaskService response time
        H->>H: Calculate task statistics
        H->>H: Build services status map
        H->>H: Set overallStatus = "Healthy"
        H->>H: CreateJsonResponse(detailedHealth)
        H-->>C: HTTP 200 + DetailedHealthStatus
    else Erro em algum serviço
        S-->>H: Exception
        H->>H: Build error details
        H->>H: Set overallStatus = "Unhealthy"
        H->>H: CreateJsonResponse(detailedHealth)
        H-->>C: HTTP 503 + Error DetailedHealthStatus
    end
```

## Timer Functions (Scheduled)

### 9. Timer: Limpeza Automática de Tarefas

```mermaid
sequenceDiagram
    participant T as Azure Timer
    participant F as TimerFunctions
    participant S as TaskService
    participant R as InMemoryTaskRepository
    participant L as Logger
    
    T->>F: TimerCleanupFunction (CRON: 0 0 2 * * *)
    F->>L: LogInformation("Iniciando limpeza...")
    F->>S: CleanupOldTasksAsync(30)
    S->>R: DeleteOldTasksAsync(cutoffDate)
    R->>R: Find old completed/cancelled tasks
    R->>R: Delete old tasks from memory
    R-->>S: deletedCount
    S-->>F: deletedCount
    F->>L: LogInformation("Limpeza concluída: {count} tarefas")
```

### 10. Timer: Relatório Diário de Tarefas

```mermaid
sequenceDiagram
    participant T as Azure Timer
    participant F as TimerFunctions
    participant S as TaskService
    participant L as Logger
    
    T->>F: DailyTaskSummary (CRON: 0 0 8 * * *)
    F->>L: LogInformation("Gerando resumo diário...")
    F->>S: GetAllTasksAsync()
    S-->>F: IEnumerable<TaskResponseDto>
    F->>F: Calculate statistics by status
    F->>F: Calculate completed today count
    F->>L: LogInformation("Resumo: {@Summary}")
```

## Queue Functions (Message Processing)

### 11. Queue: Processamento de Mensagens

```mermaid
sequenceDiagram
    participant Q as Azure Queue
    participant F as QueueFunctions
    participant S as TaskService
    participant L as Logger
    
    Q->>F: ProcessTaskFunction(queueMessage)
    F->>F: Deserialize TaskQueueMessage
    alt Message válida
        F->>F: Switch on message.Action
        alt Action = "notify_assignment"
            F->>F: ProcessTaskAssignmentNotification()
            F->>S: GetTaskByIdAsync(taskId)
            S-->>F: TaskResponseDto
            F->>L: LogInformation("Notificação enviada")
        else Action = "auto_complete"
            F->>F: ProcessAutoCompletion()
            F->>S: GetTaskByIdAsync(taskId)
            S-->>F: TaskResponseDto
            F->>S: UpdateTaskAsync(id, {status: Completed})
            S-->>F: Updated TaskResponseDto
            F->>L: LogInformation("Tarefa auto-completada")
        else Action = "priority_escalation"
            F->>F: ProcessPriorityEscalation()
            F->>S: GetTaskByIdAsync(taskId)
            S-->>F: TaskResponseDto
            F->>F: Check if task is old and pending
            F->>L: LogWarning("Escalação necessária")
        end
    else Message inválida
        F->>L: LogWarning("Mensagem inválida")
    end
```

## Blob Functions (File Processing)

### 12. Blob: Processamento de Anexos

```mermaid
sequenceDiagram
    participant B as Azure Blob
    participant F as BlobFunctions
    participant S as TaskService
    participant L as Logger
    
    B->>F: ProcessTaskAttachmentFunction(blobContent, name)
    F->>F: Parse filename: {taskId}_{filename}.{ext}
    alt Filename válido
        F->>S: GetTaskByIdAsync(taskId)
        alt Task exists
            S-->>F: TaskResponseDto
            F->>F: Determine file type by extension
            alt File type = .txt/.md
                F->>F: ProcessTextAttachment()
                F->>F: Extract text content
                F->>S: UpdateTaskAsync(id, {description: updated})
                S-->>F: Updated task
            else File type = .json
                F->>F: ProcessJsonAttachment()
                F->>F: Parse JSON for tags
                F->>S: UpdateTaskAsync(id, {tags: extracted})
                S-->>F: Updated task
            else File type = image
                F->>F: ProcessImageAttachment()
                F->>L: LogInformation("Image processed")
            else File type = .pdf
                F->>F: ProcessPdfAttachment()
                F->>L: LogInformation("PDF text extracted")
            else Generic file
                F->>F: ProcessGenericAttachment()
                F->>L: LogInformation("Generic file processed")
            end
            F->>L: LogInformation("Anexo processado com sucesso")
        else Task não existe
            S-->>F: null
            F->>L: LogWarning("Tarefa não encontrada")
        end
    else Filename inválido
        F->>L: LogWarning("Nome de arquivo inválido")
    end
```

## Middleware (Exception Handling)

### 13. Exception Handling Middleware

```mermaid
sequenceDiagram
    participant C as Cliente
    participant M as ExceptionHandlingMiddleware
    participant F as Any Function
    participant L as Logger
    
    C->>M: HTTP Request
    M->>F: next(context)
    alt Function executes successfully
        F-->>M: Success response
        M-->>C: HTTP Response
    else Function throws exception
        F-->>M: Exception
        M->>L: LogError(exception)
        M->>M: Determine error type & status code
        alt ArgumentException
            M->>M: Set HTTP 400 + "Parâmetros inválidos"
        else KeyNotFoundException
            M->>M: Set HTTP 404 + "Recurso não encontrado"
        else UnauthorizedAccessException
            M->>M: Set HTTP 401 + "Acesso não autorizado"
        else Generic Exception
            M->>M: Set HTTP 500 + "Erro interno"
        end
        M->>M: Serialize ApiResponse with error
        M-->>C: HTTP Error Response + JSON
    end
```
