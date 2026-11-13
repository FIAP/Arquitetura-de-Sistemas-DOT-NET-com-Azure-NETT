# Diagrama de Sequência - Consumer

Este diagrama mostra o fluxo de execução do Consumer, desde a inicialização até o processamento contínuo de mensagens do SQS.

## Fluxo Principal

1. **Inicialização**: Configuração do Dependency Injection e cliente AWS
2. **Loop Principal**: Processamento contínuo de mensagens
3. **Recepção**: Uso de long polling para receber mensagens eficientemente
4. **Processamento**: Simulação completa do fluxo de um pedido de e-commerce
5. **Limpeza**: Remoção de mensagens processadas com sucesso
6. **Tratamento de Erros**: Mensagens com erro permanecem na fila para reprocessamento

```mermaid
sequenceDiagram
    participant P as Program.cs
    participant CS as SqsConsumerService
    participant SQS as LocalStack SQS
    participant PP as Processador de Pedidos

    P->>P: Inicia aplicação
    P->>P: Configura DI e AWS Client
    P->>P: Inicia loop principal
    
    loop Processamento contínuo
        P->>CS: ReceberMensagensAsync()
        CS->>SQS: GetQueueUrl("mensagens-exemplo")
        
        alt Fila existe
            SQS-->>CS: URL da fila
        else Fila não existe
            CS-->>P: Erro - Fila não encontrada
            P->>P: Aguarda e tenta novamente
        end
        
        CS->>SQS: ReceiveMessage(maxMsg=10, waitTime=20s)
        
        alt Mensagens disponíveis
            SQS-->>CS: Lista de mensagens
            
            loop Para cada mensagem
                CS->>CS: Deserializar JSON para PedidoMessage
                CS->>PP: ProcessarPedidoAsync(pedido)
                
                PP->>PP: Validar pedido (500ms)
                PP->>PP: Verificar estoque (800ms)
                PP->>PP: Processar pagamento (1000ms)
                PP->>PP: Enviar email confirmação (300ms)
                PP->>PP: Delay configurável (2s)
                PP-->>CS: Processamento concluído
                
                alt Processamento bem sucedido
                    CS->>SQS: DeleteMessage(receiptHandle)
                    SQS-->>CS: Mensagem removida
                    CS->>P: Log - Mensagem processada
                else Erro no processamento
                    CS->>P: Log - Mensagem permanece na fila
                end
            end
            
        else Nenhuma mensagem
            SQS-->>CS: Lista vazia
            CS->>P: Log - Aguardando mensagens
        end
        
        P->>CS: GetQueueMessageCountAsync()
        CS->>SQS: GetQueueAttributes()
        SQS-->>CS: Estatísticas da fila
        CS->>P: Exibe estatísticas
    end
    
    Note over P: Ctrl+C para parar
    P->>P: Finaliza aplicação
```

## Componentes

- **Program.cs**: Ponto de entrada e loop principal de processamento
- **SqsConsumerService**: Serviço responsável pela comunicação com SQS
- **LocalStack SQS**: Simulador local do Amazon SQS
- **Processador de Pedidos**: Lógica de negócio para processar pedidos

## Características Técnicas

- **Long Polling**: Aguarda até 20 segundos por mensagens para reduzir chamadas desnecessárias
- **Batch Processing**: Processa até 10 mensagens por vez
- **Tratamento de Erros**: Mensagens com falha permanecem na fila para retry automático
- **Processamento Assíncrono**: Simula operações reais de um sistema de e-commerce
