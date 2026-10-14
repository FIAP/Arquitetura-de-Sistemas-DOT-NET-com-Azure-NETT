# Cenários de Teste - RabbitMQ Demo

## Casos de Teste Funcionais

### 1. Teste Básico - Happy Path

**Objetivo**: Verificar fluxo completo de envio e recebimento de mensagem

**Pré-condições**:
- RabbitMQ rodando
- Consumer ativo
- Producer acessível

**Passos**:
1. Acessar http://localhost:5000
2. Digite "Teste de mensagem básica"
3. Clicar em "Enviar Mensagem"
4. Verificar confirmação na web
5. Verificar log no consumer

**Resultado Esperado**:
- Web mostra: "Mensagem enviada com sucesso às HH:mm:ss!"
- Consumer mostra: "[HH:mm:ss] Mensagem recebida: [timestamp] Teste de mensagem básica"

### 2. Teste de Validação

**Objetivo**: Verificar validações do formulário

**Cenários**:

```mermaid
graph TD
    A[Teste de Validação] --> B[Mensagem Vazia]
    A --> C[Mensagem > 500 chars]
    A --> D[Caracteres Especiais]
    A --> E[Unicode/Emojis]
    
    B --> B1[Erro: A mensagem é obrigatória]
    C --> C1[Erro: Máximo 500 caracteres]
    D --> D1[Sucesso: Caracteres especiais aceitos]
    E --> E1[Sucesso: Unicode suportado]
```

**Casos de Teste**:

| Caso | Input | Resultado Esperado |
|------|-------|-------------------|
| Vazio | "" | "A mensagem é obrigatória" |
| Muito grande | String com 501 chars | "Máximo 500 caracteres" |
| Especiais | "!@#$%^&*()" | Sucesso |
| Unicode | "Olá 🚀 Teste" | Sucesso |
| SQL Injection | "'; DROP TABLE--" | Sucesso (escaped) |

### 3. Teste de Concorrência

**Objetivo**: Verificar comportamento com múltiplas mensagens simultâneas

**Setup**:
```bash
# Terminal 1 - Consumer
cd MessageConsumer && dotnet run

# Terminal 2 - Producer
cd MessageProducer && dotnet run

# Terminal 3 - Testes automatizados
curl -X POST http://localhost:5000/Message/SendMessage \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "Message=Teste 1"
```

**Script de Teste**:
```bash
#!/bin/bash
for i in {1..10}; do
  curl -X POST http://localhost:5000/Message/SendMessage \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "Message=Mensagem concorrente $i" &
done
wait
```

### 4. Teste de Resiliência

#### 4.1 RabbitMQ Indisponível

**Cenário**: RabbitMQ parado durante envio de mensagem

**Setup**:
```bash
# Parar RabbitMQ
./rabbitmq.sh stop

# Tentar enviar mensagem via web
```

**Resultado Esperado**:
- Web mostra erro de conexão
- Aplicação não quebra
- Após restart do RabbitMQ, aplicação reconecta

#### 4.2 Consumer Desconectado

**Cenário**: Consumer para durante processamento

**Setup**:
1. Enviar mensagem via web
2. Parar consumer (Ctrl+C)
3. Verificar fila no RabbitMQ Management
4. Reiniciar consumer

**Resultado Esperado**:
- Mensagem fica na fila
- Após restart, consumer processa mensagem pendente

#### 4.3 Múltiplos Consumers

**Cenário**: Vários consumers compartilhando carga

**Setup**:
```bash
# Terminal 1
cd MessageConsumer && dotnet run

# Terminal 2  
cd MessageConsumer && dotnet run

# Enviar várias mensagens
```

**Resultado Esperado**:
- Mensagens distribuídas entre consumers (round-robin)
- Cada mensagem processada apenas uma vez

## Testes de Performance

### 1. Throughput Test

**Objetivo**: Medir taxa de mensagens por segundo

**Script**:
```bash
#!/bin/bash
echo "Teste de Throughput - $(date)"
start_time=$(date +%s)

for i in {1..1000}; do
  curl -s -X POST http://localhost:5000/Message/SendMessage \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "Message=Performance test message $i" > /dev/null &
  
  if [ $((i % 50)) -eq 0 ]; then
    wait  # Aguarda batch de 50
  fi
done

wait
end_time=$(date +%s)
duration=$((end_time - start_time))
rate=$((1000 / duration))

echo "1000 mensagens em ${duration}s = ${rate} msg/s"
```

### 2. Memory Usage Test

**Objetivo**: Verificar uso de memória com muitas mensagens

**Setup**:
```bash
# Monitor RabbitMQ
docker stats rabbitmq

# Monitor aplicações  
ps aux | grep dotnet
```

### 3. Connection Pool Test

**Objetivo**: Testar comportamento com muitas conexões

**Modificação Temporária no RabbitMQService**:
```csharp
// APENAS PARA TESTE - Não use em produção
public async Task SendMessageAsync(string message)
{
    // Nova conexão para cada mensagem (anti-pattern)
    var factory = new ConnectionFactory() { /* config */ };
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();
    // ... resto do código
}
```

## Monitoramento e Debugging

### 1. RabbitMQ Management UI

**Métricas Importantes**:

| Métrica | Local | Descrição |
|---------|-------|-----------|
| Queue Length | Queues > mensagens | Mensagens pendentes |
| Message Rates | Overview | Pub/Sub rate |
| Connections | Connections | Conexões ativas |
| Memory | Overview | Uso de memória |

### 2. Application Logs

**Producer Logs**:
```csharp
// Adicionar logging no MessageController
private readonly ILogger<MessageController> _logger;

[HttpPost]
public async Task<IActionResult> SendMessage(MessageModel model)
{
    _logger.LogInformation("Enviando mensagem: {Message}", model.Message);
    
    try
    {
        await _rabbitMQService.SendMessageAsync(messageWithTimestamp);
        _logger.LogInformation("Mensagem enviada com sucesso");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao enviar mensagem");
    }
}
```

**Consumer Logs**:
```csharp
// Já implementado no Program.cs
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Mensagem recebida: {message}");
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Mensagem processada com sucesso!");
```

### 3. Health Checks

**Implementação para Producer**:
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddRabbitMQ(connectionString: "amqp://admin:admin123@localhost:5672");

app.MapHealthChecks("/health");
```

**Teste**:
```bash
curl http://localhost:5000/health
```

## Troubleshooting

### Problemas Comuns

#### 1. "Connection Refused"

**Sintomas**:
- Aplicação não conecta ao RabbitMQ
- Timeout de conexão

**Diagnóstico**:
```bash
# Verificar se RabbitMQ está rodando
docker ps | grep rabbitmq

# Verificar logs
docker logs rabbitmq

# Verificar portas
netstat -an | grep 5672
```

**Soluções**:
```bash
# Reiniciar RabbitMQ
./rabbitmq.sh stop
./rabbitmq.sh start

# Verificar configurações
cat MessageProducer/appsettings.json | grep RabbitMQ
```

#### 2. "Authentication Failed"

**Sintomas**:
- Erro 403 na conexão
- Log de falha de autenticação

**Diagnóstico**:
```bash
# Verificar usuários no RabbitMQ
docker exec rabbitmq rabbitmqctl list_users
```

**Solução**:
```bash
# Recriar usuário
docker exec rabbitmq rabbitmqctl add_user admin admin123
docker exec rabbitmq rabbitmqctl set_permissions -p / admin ".*" ".*" ".*"
```

#### 3. Mensagens não processadas

**Sintomas**:
- Mensagens ficam na fila
- Consumer não processa

**Diagnóstico**:
```bash
# Verificar fila no Management UI
# http://localhost:15672 > Queues > mensagens

# Verificar se consumer está ativo
ps aux | grep MessageConsumer
```

**Solução**:
```bash
# Reiniciar consumer
cd MessageConsumer
dotnet run
```

#### 4. Performance Issues

**Sintomas**:
- Lentidão no envio
- Alto uso de CPU/memória

**Diagnóstico**:
```bash
# Monitor sistema
top -p $(pgrep dotnet)

# Monitor RabbitMQ
docker exec rabbitmq rabbitmq-diagnostics status
```

**Otimizações**:
```csharp
// Connection pooling
services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory() { /* config */ };
    return factory.CreateConnection();
});

// Channel pooling
services.AddScoped<IModel>(sp =>
{
    var connection = sp.GetService<IConnection>();
    return connection.CreateModel();
});
```

### Scripts de Diagnóstico

```bash
#!/bin/bash
# diagnose.sh - Script completo de diagnóstico

echo "=== DIAGNÓSTICO RABBITMQ DEMO ==="
echo

echo "1. Docker Status:"
docker ps | grep rabbitmq

echo "2. RabbitMQ Queues:"
curl -s -u admin:admin123 http://localhost:15672/api/queues | jq '.[].name'

echo "3. Application Processes:"
ps aux | grep dotnet | grep -v grep

echo "4. Network Connections:"
netstat -an | grep :5672
netstat -an | grep :5000

echo "5. Memory Usage:"
docker stats --no-stream rabbitmq

echo "6. Disk Usage:"
df -h | grep docker
```

