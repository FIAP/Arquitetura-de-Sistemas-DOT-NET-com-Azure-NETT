# Diagrama de Sequência - RabbitMQ Demo

## Fluxo Completo de Mensagens

```mermaid
sequenceDiagram
    participant U as Usuário
    participant W as Web Browser
    participant P as MessageProducer<br/>(ASP.NET Core)
    participant R as RabbitMQService
    participant MQ as RabbitMQ<br/>Broker
    participant Q as Queue: mensagens
    participant C as MessageConsumer<br/>(Console App)

    Note over U,C: Fluxo de Envio de Mensagem
    
    U->>W: Acessa http://localhost:5000
    W->>P: GET /Message/Index
    P->>W: Retorna formulário HTML
    W->>U: Exibe interface de envio
    
    U->>W: Preenche mensagem e clica "Enviar"
    W->>P: POST /Message/SendMessage
    
    Note over P: MessageController.SendMessage()
    P->>P: Valida MessageModel
    P->>R: SendMessageAsync(message)
    
    Note over R: RabbitMQService.SendMessageAsync()
    R->>MQ: CreateConnectionAsync()<br/>host: localhost<br/>user: admin<br/>pass: admin123
    MQ->>R: Connection estabelecida
    
    R->>MQ: CreateChannelAsync()
    MQ->>R: Channel criado
    
    R->>MQ: QueueDeclareAsync()<br/>queue: "mensagens"<br/>durable: true<br/>exclusive: false<br/>autoDelete: false
    MQ->>Q: Declara/confirma fila
    Q->>MQ: Fila pronta
    MQ->>R: Fila declarada
    
    R->>MQ: BasicPublishAsync()<br/>exchange: ""<br/>routingKey: "mensagens"<br/>body: UTF8(message)
    MQ->>Q: Envia mensagem para fila
    Q->>MQ: Mensagem armazenada
    MQ->>R: Publish confirmado
    
    R->>P: Mensagem enviada com sucesso
    P->>W: Redirect com TempData["Success"]
    W->>U: Exibe confirmação de envio
    
    Note over U,C: Fluxo de Consumo de Mensagem
    
    Note over C: Consumer já está rodando
    C->>MQ: CreateConnectionAsync()<br/>host: localhost<br/>user: admin<br/>pass: admin123
    MQ->>C: Connection estabelecida
    
    C->>MQ: CreateChannelAsync()
    MQ->>C: Channel criado
    
    C->>MQ: QueueDeclareAsync()<br/>queue: "mensagens"<br/>durable: true
    MQ->>Q: Confirma fila existe
    Q->>MQ: Fila confirmada
    MQ->>C: Fila declarada
    
    C->>MQ: BasicConsumeAsync()<br/>queue: "mensagens"<br/>autoAck: true<br/>consumer: AsyncEventingBasicConsumer
    MQ->>C: Consumer registrado
    
    loop Aguardando Mensagens
        Q->>MQ: Mensagem disponível
        MQ->>C: ReceivedAsync(message)
        Note over C: Processa mensagem:<br/>- Log de recebimento<br/>- Simula processamento (1s)<br/>- Log de conclusão
        C->>C: Task.Delay(1000)
        C->>MQ: Ack automático (autoAck: true)
    end
```

## Detalhes Técnicos

### Configurações do RabbitMQ

| Componente | Configuração | Valor |
|------------|--------------|-------|
| **Host** | HostName | localhost |
| **Porta** | Port | 5672 (AMQP) |
| **Management** | Management UI | 15672 |
| **Usuário** | UserName | admin |
| **Senha** | Password | admin123 |
| **Virtual Host** | VHost | / (default) |

### Configurações da Fila

| Propriedade | Valor | Descrição |
|-------------|-------|-----------|
| **Nome** | mensagens | Nome da fila |
| **Durable** | true | Fila persiste após restart do broker |
| **Exclusive** | false | Fila pode ser acessada por múltiplas conexões |
| **Auto-Delete** | false | Fila não é deletada automaticamente |
| **Exchange** | "" (default) | Usa o exchange padrão |
| **Routing Key** | mensagens | Mesma do nome da fila |

### Padrão de Messaging

- **Padrão**: Work Queue (Point-to-Point)
- **Exchange Type**: Direct (default)
- **Delivery Mode**: Persistent (durable queue)
- **Acknowledgment**: Auto-ACK
- **Prefetch**: Não configurado (default)

### Fluxo de Dados

1. **Serialização**: UTF-8 encoding
2. **Formato da Mensagem**: `[timestamp] texto_da_mensagem`
3. **Timestamp**: `yyyy-MM-dd HH:mm:ss`
4. **Processamento**: Assíncrono com delay simulado

