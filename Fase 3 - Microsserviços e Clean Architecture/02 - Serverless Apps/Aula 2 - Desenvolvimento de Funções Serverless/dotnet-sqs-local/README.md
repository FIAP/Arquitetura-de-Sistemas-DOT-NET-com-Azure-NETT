# Exemplo .NET 8 com SQS e LocalStack

Este projeto demonstra como integrar Amazon SQS com .NET 8 usando LocalStack para desenvolvimento local. O exemplo simula um sistema de e-commerce com processamento assíncrono de pedidos através de mensageria.

## 🎯 Overview dos Projetos

### 🏭 Producer
Aplicação console que simula um sistema de pedidos, gerando e enviando mensagens para uma fila SQS. Cada mensagem representa um pedido de e-commerce com itens, valores e dados do cliente.

**Funcionalidades:**
- Gera pedidos aleatórios com dados fictícios realistas
- Cria automaticamente a fila SQS se ela não existir
- Envia mensagens com atributos personalizados (tipo, valor, data)
- Exibe estatísticas da fila em tempo real
- Interface interativa para controle manual

### 📨 Consumer  
Aplicação console que processa mensagens da fila de forma assíncrona, simulando o fluxo completo de processamento de um pedido.

**Funcionalidades:**
- Recebe mensagens usando long polling (eficiente)
- Processa pedidos simulando: validação, verificação de estoque, processamento de pagamento
- Remove mensagens processadas com sucesso da fila
- Mantém mensagens com erro na fila para reprocessamento
- Exibe logs detalhados com cores para facilitar acompanhamento

## 📋 Pré-requisitos

- **.NET 8 SDK** instalado
- **Docker** e **Docker Compose** instalados
- **LocalStack** rodando (ou use o docker-compose fornecido)

## 🚀 Como executar

### 1. Iniciar LocalStack

Se você não tem o LocalStack rodando, use o docker-compose fornecido:

```bash
docker-compose up -d
```

Para verificar se está funcionando:
```bash
curl http://localhost:4566/_localstack/health
```

### 2. Executar o Producer

Em um terminal, navegue até a pasta do Producer:

```bash
cd Producer
dotnet restore
dotnet run
```

O Producer permite enviar pedidos de exemplo pressionando qualquer tecla. Digite 'q' para sair.

### 3. Executar o Consumer

Em outro terminal, navegue até a pasta do Consumer:

```bash
cd Consumer
dotnet restore
dotnet run
```

O Consumer ficará aguardando mensagens e processará automaticamente conforme chegarem. Use Ctrl+C para parar.

## 🏗️ Estrutura do Projeto

```
dotnet-sqs/
├── Producer/                    # Projeto que envia mensagens
│   ├── Models/
│   │   └── PedidoMessage.cs    # Modelo da mensagem de pedido
│   ├── Services/
│   │   └── SqsProducerService.cs # Serviço para enviar mensagens
│   ├── Program.cs              # Aplicação principal do producer
│   ├── appsettings.json        # Configurações
│   └── Producer.csproj         # Arquivo do projeto
│
├── Consumer/                    # Projeto que recebe mensagens
│   ├── Models/
│   │   └── PedidoMessage.cs    # Modelo da mensagem de pedido
│   ├── Services/
│   │   └── SqsConsumerService.cs # Serviço para receber mensagens
│   ├── Program.cs              # Aplicação principal do consumer
│   ├── appsettings.json        # Configurações
│   └── Consumer.csproj         # Arquivo do projeto
│
├── docker-compose.yml          # LocalStack setup
└── README.md                   # Este arquivo
```

## ⚙️ Configuração

### appsettings.json

Ambos os projetos usam configurações similares:

```json
{
  "AWS": {
    "Region": "us-east-1",
    "ServiceURL": "http://localhost:4566",
    "AccessKey": "test",
    "SecretKey": "test"
  },
  "SQS": {
    "QueueName": "mensagens-exemplo",
    "MaxMessages": 10,
    "WaitTimeSeconds": 20,
    "VisibilityTimeoutSeconds": 30
  }
}
```

### Principais Configurações:

- **ServiceURL**: Endpoint do LocalStack (http://localhost:4566)
- **QueueName**: Nome da fila SQS
- **MaxMessages**: Máximo de mensagens por polling
- **WaitTimeSeconds**: Tempo de espera por mensagens (long polling)
- **VisibilityTimeoutSeconds**: Tempo que mensagem fica invisível após ser recebida

## 🔧 Funcionalidades

### Producer
- ✅ Gera pedidos aleatórios com dados fictícios
- ✅ Envia mensagens para SQS com atributos personalizados
- ✅ Cria automaticamente a fila se não existir
- ✅ Mostra estatísticas da fila
- ✅ Interface interativa no console

### Consumer
- ✅ Recebe mensagens da fila usando long polling
- ✅ Processa pedidos de forma assíncrona
- ✅ Simula validação, verificação de estoque e pagamento
- ✅ Remove mensagens processadas com sucesso da fila
- ✅ Tratamento de erros (mensagens com erro permanecem na fila)
- ✅ Mostra estatísticas em tempo real

## 📊 Exemplo de Uso

1. **Inicie o LocalStack**:
   ```bash
   docker-compose up -d
   ```

2. **Execute o Consumer** (em um terminal):
   ```bash
   cd Consumer && dotnet run
   ```

3. **Execute o Producer** (em outro terminal):
   ```bash
   cd Producer && dotnet run
   ```

4. **No Producer**, pressione qualquer tecla para enviar pedidos
5. **No Consumer**, observe o processamento automático das mensagens

## 🛠️ Dependências

### Pacotes NuGet utilizados:
- `AWSSDK.SQS` - SDK AWS para SQS
- `Microsoft.Extensions.Configuration.Json` - Configuração JSON
- `Microsoft.Extensions.DependencyInjection` - Injeção de dependência
- `Microsoft.Extensions.Hosting` - Host builder
- `Microsoft.Extensions.Logging.Console` - Logging no console
- `System.Text.Json` - Serialização JSON

## 🐳 LocalStack

O LocalStack simula serviços AWS localmente. A configuração fornecida:
- Expõe a porta 4566 para acesso aos serviços
- Habilita persistência de dados
- Configura apenas o serviço SQS

## 🔍 Troubleshooting

### Problemas Comuns:

1. **Erro de conexão com LocalStack**:
   - Verifique se o LocalStack está rodando: `docker ps`
   - Teste a conectividade: `curl http://localhost:4566/_localstack/health`

2. **Fila não encontrada**:
   - Execute o Producer primeiro para criar a fila
   - Ou crie manualmente: `aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name mensagens-exemplo`

3. **Mensagens não sendo processadas**:
   - Verifique os logs do Consumer
   - Confirme que ambos usam o mesmo nome de fila

## 📝 Próximos Passos

Sugestões para expandir o exemplo:

- ✨ Adicionar Dead Letter Queue (DLQ)
- ✨ Implementar retry com backoff exponencial  
- ✨ Adicionar métricas e monitoramento
- ✨ Usar AWS Lambda para processamento serverless
- ✨ Implementar batch processing
- ✨ Adicionar validação de schema das mensagens
