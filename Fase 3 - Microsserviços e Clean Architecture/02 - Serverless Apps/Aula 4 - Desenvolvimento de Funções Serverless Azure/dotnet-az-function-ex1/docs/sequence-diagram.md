# Diagrama de Sequência - Azure Function .NET

Este diagrama mostra como funciona o fluxo de uma requisição HTTP no projeto Azure Function .NET.

## Cenário 1: Requisição GET

```mermaid
sequenceDiagram
    participant Client as Cliente
    participant AzFunc as Azure Functions<br/>Core Tools
    participant Runtime as .NET 8<br/>Runtime
    participant Program as Program.cs<br/>(Host Builder)
    participant Function as HttpTriggerFunction.cs
    participant Logger as ILogger

    Note over Client,Logger: Requisição GET com parâmetro name

    Client->>+AzFunc: GET /api/HttpTriggerExample?name=ThiagoAdriano
    AzFunc->>+Runtime: Iniciar worker process (.NET 8)
    Runtime->>+Program: ConfigureFunctionsWorkerDefaults()
    Program->>+Function: Instanciar HttpTriggerFunction
    
    AzFunc->>+Function: [HttpTrigger] Run(HttpRequestData)
    Function->>+Logger: LogInformation("Azure Function HTTP trigger processando...")
    
    Function->>Function: Verificar método HTTP (GET)
    Function->>Function: Extrair parâmetro 'name' da query string
    Function->>Function: Criar objeto de resposta JSON
    Function->>Function: Serializar com JsonSerializer
    
    Function->>-AzFunc: HttpResponseData (200 OK)
    AzFunc->>-Client: JSON Response
    
    Note over Client: {"message": "Olá, ThiagoAdriano!", "method": "GET", "timestamp": "...", "framework": ".NET 8"}
```

## Cenário 2: Requisição POST

```mermaid
sequenceDiagram
    participant Client as Cliente
    participant AzFunc as Azure Functions<br/>Core Tools
    participant Runtime as .NET 8<br/>Runtime
    participant Function as HttpTriggerFunction.cs
    participant StreamReader as StreamReader
    participant JsonSerializer as System.Text.Json<br/>JsonSerializer
    participant Logger as ILogger

    Note over Client,Logger: Requisição POST com dados JSON

    Client->>+AzFunc: POST /api/HttpTriggerExample<br/>Content-Type: application/json<br/>Body: {"name": "João", "curso": "FIAP"}
    
    AzFunc->>+Function: [HttpTrigger] Run(HttpRequestData)
    Function->>+Logger: LogInformation("Azure Function HTTP trigger processando...")
    
    Function->>Function: Verificar método HTTP (POST)
    Function->>+StreamReader: ReadToEndAsync(req.Body)
    StreamReader->>-Function: String requestBody
    
    alt JSON válido
        Function->>+JsonSerializer: Deserialize<Dictionary<string, object>>(requestBody)
        JsonSerializer->>-Function: Dictionary com dados parseados
        Function->>Function: Extrair 'name' do Dictionary
        Function->>Function: Criar responseData com dados recebidos
    else JSON inválido
        Function->>Function: Catch JsonException
        Function->>Function: Criar response de erro (400 Bad Request)
    end
    
    Function->>+JsonSerializer: Serialize(responseData, JsonSerializerOptions)
    Note over JsonSerializer: PropertyNamingPolicy: CamelCase<br/>WriteIndented: true<br/>Encoder: UnsafeRelaxedJsonEscaping
    JsonSerializer->>-Function: JSON string formatado
    
    Function->>Function: WriteStringAsync(jsonString)
    Function->>-AzFunc: HttpResponseData (200 OK)
    AzFunc->>-Client: JSON Response
    
    Note over Client: {"message": "Dados recebidos via POST! Olá, João!", "method": "POST", "receivedData": {...}}
```

## Cenário 3: Método Não Suportado (Erro 405)

```mermaid
sequenceDiagram
    participant Client as Cliente
    participant AzFunc as Azure Functions<br/>Core Tools
    participant Function as HttpTriggerFunction.cs
    participant Logger as ILogger

    Note over Client,Logger: Requisição com método não suportado

    Client->>+AzFunc: PUT /api/HttpTriggerExample
    AzFunc->>+Function: [HttpTrigger] Run(HttpRequestData)
    Function->>+Logger: LogInformation("Azure Function HTTP trigger processando...")
    
    Function->>Function: Verificar método HTTP (PUT)
    Function->>Function: Método não é GET nem POST
    Function->>Function: Criar errorData com métodos suportados
    Function->>Function: CreateResponse(MethodNotAllowed)
    
    Function->>-AzFunc: HttpResponseData (405 Method Not Allowed)
    AzFunc->>-Client: JSON Error Response
    
    Note over Client: {"error": "Método PUT não suportado", "supportedMethods": ["GET", "POST"]}
```

## Arquitetura do Sistema

```mermaid
graph TB
    subgraph "Cliente"
        Browser[Browser/Postman/curl]
        RestClient[VS Code REST Client]
    end
    
    subgraph "Azure Functions Local"
        CoreTools[Azure Functions Core Tools<br/>Port 7071]
        
        subgraph ".NET 8 Runtime"
            Host[Host Builder<br/>Program.cs]
            DI[Dependency Injection<br/>ILogger, Application Insights]
            
            subgraph "Function App"
                HttpFunc[HttpTriggerFunction.cs<br/>[GET, POST]]
                Config[Configuration<br/>host.json, local.settings.json]
            end
        end
    end
    
    subgraph "Infraestrutura"
        AppInsights[Application Insights<br/>Telemetry]
        Storage[Azure Storage<br/>UseDevelopmentStorage]
    end
    
    Browser --> CoreTools
    RestClient --> CoreTools
    CoreTools --> Host
    Host --> DI
    DI --> HttpFunc
    HttpFunc --> Config
    HttpFunc --> AppInsights
    CoreTools --> Storage
```

## Fluxo de Dados

```mermaid
flowchart LR
    A[HTTP Request] --> B{Método?}
    
    B -->|GET| C[Extrair query string]
    B -->|POST| D[Ler JSON do body]
    B -->|Outros| E[Erro 405]
    
    C --> F[Criar resposta GET]
    D --> G{JSON válido?}
    
    G -->|Sim| H[Deserializar JSON]
    G -->|Não| I[Erro 400]
    
    H --> J[Criar resposta POST]
    
    F --> K[Serializar resposta]
    J --> K
    I --> L[Resposta de erro]
    E --> L
    
    K --> M[HTTP 200 Response]
    L --> N[HTTP Error Response]
```

## Configurações Importantes

### host.json
- **version**: 2.0
- **extensionBundle**: Microsoft.Azure.Functions.ExtensionBundle v4
- **functionTimeout**: 5 minutos
- **logging**: Application Insights habilitado

### local.settings.json
- **FUNCTIONS_WORKER_RUNTIME**: dotnet-isolated
- **AzureWebJobsStorage**: UseDevelopmentStorage=true
- **FUNCTIONS_EXTENSION_VERSION**: ~4

### Dependências (.csproj)
- **Microsoft.Azure.Functions.Worker**: 1.21.0
- **Microsoft.Azure.Functions.Worker.Extensions.Http**: 3.2.0
- **Microsoft.ApplicationInsights.WorkerService**: 2.22.0
- **Target Framework**: .NET 8.0

## Logs de Execução

Durante a execução, você verá logs como:

```
[2025-08-22T14:47:40.039Z] Executing 'Functions.HttpTriggerExample' (Id=fec864e3-3429-4ca4-b3fb-b07128defcfc)
[2025-08-22T14:47:40.049Z] Azure Function HTTP trigger processando uma requisição.
[2025-08-22T14:47:40.051Z] Executed 'Functions.HttpTriggerExample' (Succeeded, Duration=14ms)
```

Isso mostra:
1. **Inicio da execução** com ID único
2. **Log personalizado** da função
3. **Fim da execução** com duração
