# Diagrama de Sequência - Fluxo Completo

Este diagrama mostra o fluxo end-to-end completo, desde a geração de um pedido no Producer até o processamento final no Consumer, passando pelo SQS como middleware de mensageria.

## Visão Geral do Fluxo

O exemplo simula um sistema de e-commerce real com as seguintes fases:

1. **🏭 Geração**: Producer cria pedidos fictícios com dados realistas
2. **📤 Envio**: Mensagens são enviadas para fila SQS com metadados
3. **⚡ Processamento**: Consumer processa pedidos de forma assíncrona
4. **🎯 Conclusão**: Mensagens processadas são removidas da fila

```mermaid
sequenceDiagram
    participant U as Usuário
    participant PROD as Producer
    participant SQS as LocalStack SQS
    participant CONS as Consumer
    participant SYS as Sistema E-commerce

    Note over U,SYS: Fluxo Completo: Pedido → Processamento → Conclusão
    
    U->>PROD: Inicia Producer
    CONS->>CONS: Inicia Consumer (rodando em paralelo)
    CONS->>SQS: Aguarda mensagens (long polling)
    
    rect rgb(240, 255, 240)
        Note over U,PROD: Fase 1: Geração do Pedido
        U->>PROD: Pressiona tecla
        PROD->>PROD: Gera PedidoMessage aleatório<br/>Cliente: João Silva<br/>Itens: Notebook, Mouse<br/>Total: R$ 2.550,00
        
        PROD->>SQS: Verifica se fila existe
        alt Primeira execução
            SQS-->>PROD: Fila não existe
            PROD->>SQS: CreateQueue("mensagens-exemplo")
            SQS-->>PROD: Fila criada com sucesso
        else Fila já existe
            SQS-->>PROD: URL da fila existente
        end
    end
    
    rect rgb(240, 240, 255)
        Note over PROD,SQS: Fase 2: Envio da Mensagem
        PROD->>PROD: Serializa pedido para JSON
        PROD->>SQS: SendMessage(pedidoJSON + attributes)
        Note right of SQS: Attributes:<br/>TipoPedido: "Ecommerce"<br/>ValorTotal: "2550.00"<br/>DataEnvio: timestamp
        SQS-->>PROD: MessageId: abc-123
        PROD->>U: ✅ Pedido enviado!<br/>📊 1 mensagem na fila
    end
    
    rect rgb(255, 240, 240)  
        Note over SQS,SYS: Fase 3: Processamento Assíncrono
        SQS-->>CONS: Nova mensagem disponível
        CONS->>SQS: ReceiveMessage(maxMsg=10, wait=20s)
        SQS-->>CONS: PedidoMessage + ReceiptHandle
        
        CONS->>CONS: Deserializa JSON → PedidoMessage
        CONS->>SYS: Inicia processamento do pedido
        
        SYS->>SYS: 🔍 Validar pedido (500ms)
        SYS->>SYS: 📦 Verificar estoque (800ms)
        SYS->>SYS: 💳 Processar pagamento R$ 2.550,00 (1000ms)
        SYS->>SYS: 📧 Enviar email para João Silva (300ms)
        SYS->>SYS: ⏱️ Delay adicional (2000ms)
        
        SYS-->>CONS: ✅ Pedido processado com sucesso
        
        CONS->>SQS: DeleteMessage(receiptHandle)
        SQS-->>CONS: Mensagem removida da fila
        CONS->>CONS: 📊 Atualiza estatísticas
        CONS->>CONS: Log: ✅ Pedido João Silva processado
    end
    
    rect rgb(255, 255, 240)
        Note over CONS,SQS: Fase 4: Monitoramento Contínuo
        CONS->>SQS: GetQueueAttributes()
        SQS-->>CONS: Disponíveis: 0, Em processamento: 0
        CONS->>CONS: Volta ao loop de polling
        CONS->>SQS: ReceiveMessage (aguarda próxima mensagem...)
    end
    
    Note over U,SYS: Processo completo: ~4.6 segundos por pedido<br/>Sistema pronto para próximo pedido
    
    opt Novo pedido
        U->>PROD: Pressiona tecla novamente
        Note over PROD,CONS: O ciclo se repete...
    end
    
    opt Parar aplicações
        U->>PROD: Digite 'q'
        PROD->>PROD: Finaliza Producer
        U->>CONS: Ctrl+C
        CONS->>CONS: Finaliza Consumer
    end
```

## Componentes do Sistema

### 🏭 Producer
- **Função**: Simula um frontend/API que recebe pedidos
- **Tecnologia**: Aplicação Console .NET 10
- **Características**: Interface interativa, geração de dados aleatórios realistas

### 📨 LocalStack SQS  
- **Função**: Middleware de mensageria (simula Amazon SQS)
- **Tecnologia**: Contêiner Docker LocalStack
- **Características**: Persistência local, APIs compatíveis com AWS

### 📦 Consumer
- **Função**: Simula sistema de processamento de pedidos
- **Tecnologia**: Aplicação Console .NET 10  
- **Características**: Processamento assíncrono, tratamento de erros

### 🎯 Sistema E-commerce
- **Função**: Lógica de negócio para processamento de pedidos
- **Implementação**: Simulação com delays realistas
- **Etapas**: Validação → Estoque → Pagamento → Notificação

## Características Técnicas

### Performance
- **Long Polling**: Reduz latência e número de requests desnecessários
- **Batch Processing**: Processa até 10 mensagens simultaneamente
- **Processamento Assíncrono**: ~4.6 segundos por pedido

### Confiabilidade
- **At-Least-Once Delivery**: SQS garante entrega das mensagens
- **Visibility Timeout**: Previne processamento duplicado
- **Error Handling**: Mensagens com falha permanecem para retry

### Observabilidade
- **Logs Estruturados**: Informações detalhadas de cada etapa
- **Métricas**: Contadores de mensagens e estatísticas da fila
- **Cores no Console**: Facilita identificação visual de status
