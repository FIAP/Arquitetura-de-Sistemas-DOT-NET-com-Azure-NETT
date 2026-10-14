# Diagrama de Arquitetura 


```mermaid
graph TB
    subgraph "Cliente"
        U[Usuário]
        B[Navegador Web]
    end
    
    subgraph "MessageProducer (.NET 10)"
        W[Web Application<br/>ASP.NET Core MVC]
        MC[MessageController]
        RMQ[RabbitMQService]
        IM[IRabbitMQService]
        MM[MessageModel]
    end
    
    subgraph "RabbitMQ Broker"
        EX[Default Exchange<br/>""]
        Q[Queue: mensagens<br/>durable: true]
        MNG[Management UI<br/>:15672]
    end
    
    subgraph "MessageConsumer (.NET 10)"
        CA[Console Application]
        AEC[AsyncEventingBasicConsumer]
    end
    
    subgraph "Infrastructure"
        D[Docker Compose]
        RMQ_CONT[RabbitMQ Container<br/>rabbitmq:3.11-management]
    end
    
    %% Conexões
    U --> B
    B <--> W
    W --> MC
    MC --> MM
    MC --> IM
    IM --> RMQ
    
    RMQ -.->|AMQP Connection<br/>localhost:5672| EX
    EX --> Q
    Q -.->|Message Delivery| AEC
    AEC --> CA
    
    D --> RMQ_CONT
    RMQ_CONT --> EX
    
    U -.->|Monitoring| MNG
    
    %% Styling
    classDef producer fill:#e1f5fe
    classDef consumer fill:#f3e5f5
    classDef broker fill:#fff3e0
    classDef infra fill:#e8f5e8
    
    class W,MC,RMQ,IM,MM producer
    class CA,AEC consumer
    class EX,Q,MNG broker
    class D,RMQ_CONT infra
```

## Componentes Detalhados

### MessageProducer (Port: 5000)

```mermaid
graph LR
    subgraph "MessageProducer"
        direction TB
        V[Views/Message/Index.cshtml<br/>Bootstrap 5 UI]
        C[Controllers/MessageController]
        S[Services/RabbitMQService]
        I[Services/IRabbitMQService]
        M[Models/MessageModel]
        P[Program.cs<br/>DI Container]
        CFG[appsettings.json<br/>RabbitMQ Config]
    end
    
    V --> C
    C --> M
    C --> I
    I --> S
    S --> CFG
    P --> I
    P --> S
```

### MessageConsumer

```mermaid
graph LR
    subgraph "MessageConsumer"
        direction TB
        PR[Program.cs<br/>Main Entry]
        CF[ConnectionFactory]
        CN[IConnection]
        CH[IChannel]
        CO[AsyncEventingBasicConsumer]
        HD[ReceivedAsync Handler]
    end
    
    PR --> CF
    CF --> CN
    CN --> CH
    CH --> CO
    CO --> HD
```

### RabbitMQ Topology

```mermaid
graph TB
    subgraph "RabbitMQ Broker"
        EX[Default Exchange<br/>Type: Direct<br/>Name: ""]
        Q[Queue: mensagens<br/>Durable: true<br/>Exclusive: false<br/>Auto-delete: false]
        B[Binding<br/>Routing Key: "mensagens"]
    end
    
    PROD[MessageProducer] -.->|publish| EX
    EX --> B
    B --> Q
    Q -.->|consume| CONS[MessageConsumer]
    
    classDef exchange fill:#ffd54f
    classDef queue fill:#81c784
    classDef binding fill:#ffb74d
    
    class EX exchange
    class Q queue
    class B binding
```

## Padrões de Messaging Implementados

### 1. Work Queue Pattern
- **Tipo**: Point-to-Point
- **Descrição**: Uma fila de trabalho onde mensagens são distribuídas entre consumers
- **Uso**: Processamento assíncrono de tarefas

### 2. Connection Management
```mermaid
sequenceDiagram
    participant App as Application
    participant CF as ConnectionFactory
    participant Conn as Connection
    participant Ch as Channel
    
    App->>CF: new ConnectionFactory()
    App->>CF: CreateConnectionAsync()
    CF->>Conn: Nova conexão AMQP
    App->>Conn: CreateChannelAsync()
    Conn->>Ch: Novo canal
    Note over Ch: Canal usado para operações
    Ch->>Ch: QueueDeclare/Publish/Consume
```

### 3. Message Flow
```mermaid
flowchart TD
    A[Web Form Input] --> B[MessageModel Validation]
    B --> C[RabbitMQService.SendMessageAsync]
    C --> D[UTF-8 Encoding]
    D --> E[BasicPublishAsync]
    E --> F[Default Exchange]
    F --> G[Queue: mensagens]
    G --> H[AsyncEventingBasicConsumer]
    H --> I[Message Processing]
    I --> J[Auto-ACK]
```

## Configurações Técnicas

### Connection Settings
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "admin", 
    "Password": "admin123",
    "QueueName": "mensagens",
    "Port": 5672,
    "VirtualHost": "/"
  }
}
```

### Queue Declaration Parameters
```csharp
await channel.QueueDeclareAsync(
    queue: "mensagens",        // Nome da fila
    durable: true,             // Persiste restarts
    exclusive: false,          // Compartilhada
    autoDelete: false,         // Não auto-remove
    arguments: null            // Sem argumentos extras
);
```

### Consumer Configuration
```csharp
await channel.BasicConsumeAsync(
    queue: "mensagens",        // Fila a consumir
    autoAck: true,             // ACK automático
    consumer: asyncConsumer    // Handler assíncrono
);
```

## Métricas e Monitoramento

### Endpoints Disponíveis
- **Producer Web**: http://localhost:5000
- **RabbitMQ Management**: http://localhost:15672
- **Docker Health**: `docker ps` para status dos containers

### Logs
- **Producer**: Logs do ASP.NET Core
- **Consumer**: Console output com timestamps
- **RabbitMQ**: Container logs via `docker logs`

