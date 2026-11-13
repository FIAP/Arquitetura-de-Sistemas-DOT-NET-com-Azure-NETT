# Diagrama de Sequência - Producer

Este diagrama mostra o fluxo de execução do Producer, desde o início da aplicação até o envio de mensagens para o SQS.

## Fluxo Principal

1. **Inicialização**: Configuração do Dependency Injection e cliente AWS
2. **Geração de Dados**: Criação de pedidos aleatórios com dados fictícios
3. **Verificação da Fila**: Busca pela fila ou criação automática se não existir
4. **Envio da Mensagem**: Serialização e envio do pedido para o SQS
5. **Estatísticas**: Consulta do número de mensagens na fila
6. **Interface**: Exibição de resultados e controle do usuário

```mermaid
sequenceDiagram
    participant U as Usuário
    participant P as Program.cs
    participant PS as SqsProducerService
    participant SQS as LocalStack SQS
    participant G as Gerador de Dados

    U->>P: Inicia aplicação
    P->>P: Configura DI e AWS Client
    P->>U: Exibe menu inicial
    
    loop Para cada pedido
        U->>P: Pressiona tecla para enviar
        P->>G: Gerar dados aleatórios
        G-->>P: PedidoMessage com itens
        
        P->>PS: EnviarPedidoAsync(pedido)
        PS->>SQS: GetQueueUrl("mensagens-exemplo")
        
        alt Fila existe
            SQS-->>PS: URL da fila
        else Fila não existe
            PS->>SQS: CreateQueue(config)
            SQS-->>PS: Nova URL da fila criada
        end
        
        PS->>PS: Serializar pedido para JSON
        PS->>SQS: SendMessage(messageBody, attributes)
        SQS-->>PS: MessageId
        PS->>P: Sucesso/Falha
        
        P->>PS: GetQueueMessageCountAsync()
        PS->>SQS: GetQueueAttributes()
        SQS-->>PS: Número de mensagens
        PS-->>P: Contagem
        
        P->>U: Exibe resultado e estatísticas
    end
    
    U->>P: Digite 'q' para sair
    P->>P: Finaliza aplicação
```

## Componentes

- **Usuário**: Interage com a aplicação via console
- **Program.cs**: Ponto de entrada e orquestração principal
- **SqsProducerService**: Serviço responsável pela comunicação com SQS
- **LocalStack SQS**: Simulador local do Amazon SQS
- **Gerador de Dados**: Lógica para criar pedidos fictícios realistas
