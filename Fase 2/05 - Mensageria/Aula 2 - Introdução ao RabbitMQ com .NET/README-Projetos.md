# Projetos RabbitMQ com .NET 10

Este projeto demonstra comunicação entre aplicações usando RabbitMQ com .NET 10.

## Estrutura do Projeto

- **MessageProducer**: Aplicação ASP.NET Core MVC com interface web para enviar mensagens
- **MessageConsumer**: Aplicação Console que consome mensagens da fila
- **docker-compose.yml**: Configuração do RabbitMQ local

## Pré-requisitos

- .NET 10 SDK
- Docker e Docker Compose

## Como executar

### 1. Iniciar o RabbitMQ
```bash
# Usar o script helper
./rabbitmq.sh start

# Ou diretamente com docker-compose
docker-compose up -d
```

### 2. Executar o Consumer (Terminal 1)
```bash
cd MessageConsumer
dotnet run
```

### 3. Executar o Producer (Terminal 2)
```bash
cd MessageProducer
dotnet run
```

### 4. Usar a aplicação
1. Abra o navegador em: http://localhost:5000 (ou a porta indicada)
2. Digite uma mensagem e clique em "Enviar Mensagem"
3. Observe as mensagens sendo processadas no terminal do Consumer

## Funcionalidades

### Producer (Web App)
- Interface web moderna para enviar mensagens
- Validação de entrada (máximo 500 caracteres)
- Feedback visual de sucesso/erro
- Timestamp automático nas mensagens

### Consumer (Console App)
- Conecta automaticamente ao RabbitMQ
- Processa mensagens de forma assíncrona
- Simula processamento (delay de 1 segundo)
- Log detalhado de recebimento e processamento

## Configurações

### RabbitMQ (appsettings.json)
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "admin",
    "Password": "admin123",
    "QueueName": "mensagens"
  }
}
```

### Acesso ao RabbitMQ Management
- URL: http://localhost:15672
- Usuário: admin
- Senha: admin123

## Comandos Úteis

```bash
# Compilar toda a solução
dotnet build

# Executar testes (se houver)
dotnet test

# Verificar status do RabbitMQ
./rabbitmq.sh status

# Ver logs do RabbitMQ
./rabbitmq.sh logs

# Parar tudo
./rabbitmq.sh stop
```

## Tecnologias Utilizadas

- **.NET 10**: Framework principal
- **ASP.NET Core MVC**: Interface web do Producer
- **RabbitMQ.Client 7.1.2**: Biblioteca para comunicação com RabbitMQ
- **Bootstrap 5**: UI/UX do frontend
- **RabbitMQ 3.11**: Message broker

## Arquitetura

```
[Web Browser] → [MessageProducer] → [RabbitMQ] → [MessageConsumer]
     ↓                ↓                 ↓              ↓
  Interface        REST API         Message         Console
   Usuário        ASP.NET Core      Broker          Output
```

## 📚 Documentação Técnica

Consulte a pasta `docs/` para documentação detalhada:

- **[docs/README.md](docs/README.md)** - Índice completo da documentação
- **[docs/sequence-diagram.md](docs/sequence-diagram.md)** - Diagrama de sequência detalhado
- **[docs/architecture-diagram.md](docs/architecture-diagram.md)** - Arquitetura e componentes
- **[docs/messaging-concepts.md](docs/messaging-concepts.md)** - Conceitos de messaging e exchanges
- **[docs/testing-scenarios.md](docs/testing-scenarios.md)** - Cenários de teste e troubleshooting

## Próximos Passos

- Adicionar persistência de mensagens em banco de dados
- Implementar Dead Letter Queue
- Adicionar monitoramento e métricas
- Configurar múltiplos consumers
- Implementar retry policy
