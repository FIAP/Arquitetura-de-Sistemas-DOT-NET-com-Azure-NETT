# Guia Prático de Teste com Azurite

## Visão Geral

Este guia fornece comandos práticos para testar Azure Functions com Azurite passo a passo. Você aprenderá a validar instalações, configurar o ambiente e testar cada componente individualmente.

## Início Rápido (Para quem tem pressa)

Se você só quer executar o teste automatizado rapidamente:

### 1. Verificações Essenciais
```bash
# Versões obrigatórias
dotnet --version    # Deve ser 8.x
func --version      # Deve ser 4.x
azurite --version   # Deve estar instalado
jq --version        # Deve estar instalado
```

### 2. Iniciar Serviços (nessa ordem!)
```bash
# Terminal 1: Azurite PRIMEIRO
mkdir -p ~/azurite && azurite --silent --location ~/azurite

# Terminal 2: Azure Functions DEPOIS (aguardar Azurite inicializar)
cd /Users/thiagoadriano/fiap/serverless/aula4/exemplo2
dotnet build && func start --port 7071
```

### 3. Verificar e Testar
```bash
# Terminal 3: Verificar saúde
curl -s http://localhost:7071/api/health | jq .status  # Deve retornar "Healthy"

# Executar teste automatizado
./test-azurite.sh
```

### 4. OU Use o Script Automático Completo
```bash
# Este script faz TUDO automaticamente (recomendado)
./prepare-and-test.sh
```

**IMPORTANTE**: O Azurite DEVE estar rodando ANTES de iniciar o Azure Functions, caso contrário terá erro de conexão.

---

## Passo 1: Validar Instalações e Versões

### 1.1 Verificar .NET 8 SDK

```bash
# Verificar se está instalado
dotnet --version

# Deve retornar: 8.0.x (obrigatório)
# Se não estiver instalado: https://dotnet.microsoft.com/download/dotnet/8.0

# Verificar se a versão é 8.x
DOTNET_VERSION=$(dotnet --version 2>/dev/null | cut -d. -f1)
if [ "$DOTNET_VERSION" != "8" ]; then
    echo "ERRO: .NET 8 SDK é obrigatório"
    echo "Versão atual: $(dotnet --version 2>/dev/null || echo 'não encontrada')"
    echo "Instale: https://dotnet.microsoft.com/download/dotnet/8.0"
fi
```

### 1.2 Verificar Azure Functions Core Tools

```bash
# Verificar versão
func --version

# Deve retornar: 4.x.x (obrigatório para compatibilidade com .NET 8)
# Se não estiver instalado:
npm install -g azure-functions-core-tools@4 --unsafe-perm true

# Verificar se a versão é 4.x
FUNC_VERSION=$(func --version 2>/dev/null | cut -d. -f1)
if [ "$FUNC_VERSION" != "4" ]; then
    echo "ERRO: Azure Functions Core Tools v4 é obrigatório"
    echo "Versão atual: $(func --version 2>/dev/null || echo 'não encontrada')"
fi
```

### 1.3 Verificar Azure CLI

```bash
# Verificar versão
az --version

# Deve retornar informações da versão
# Se não estiver instalado: https://docs.microsoft.com/cli/azure/install-azure-cli
```

### 1.4 Verificar Azurite

```bash
# Verificar se está instalado
azurite --version

# Se não estiver instalado:
npm install -g azurite
```

### 1.5 Verificar Ferramentas Auxiliares

```bash
# Verificar curl
curl --version

# Verificar jq (para processar JSON)
jq --version

# Se jq não estiver instalado:
# macOS: brew install jq
# Ubuntu: sudo apt-get install jq
```

## Passo 2: Preparar o Ambiente

### 2.1 Estrutura de Diretórios

```bash
# Navegar para o projeto
cd exemplo2

# Verificar estrutura
ls -la
# Deve mostrar: Functions/, host.json, local.settings.json, etc.
```

### 2.2 Configurar Variáveis de Ambiente

```bash
# Configurar connection strings do Azurite
export CONN_STRING="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
export QUEUE_CONN="${CONN_STRING};QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;"
export BLOB_CONN="${CONN_STRING};BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
export BASE_URL="http://localhost:7071/api"

# Verificar se foram configuradas
echo "QUEUE_CONN: $QUEUE_CONN"
echo "BLOB_CONN: $BLOB_CONN"
```

## Passo 3: Iniciar Serviços

### 3.1 Iniciar Azurite (Terminal 1)

```bash
# Iniciar Azurite em modo silencioso
azurite --silent --location ~/azurite

# Verificar se está rodando
# Deve mostrar mensagens de inicialização
# Manter este terminal aberto
```

### 3.2 Verificar Portas do Azurite

```bash
# Em outro terminal, verificar se as portas estão ativas
lsof -i :10000  # Blob Storage
lsof -i :10001  # Queue Storage
lsof -i :10002  # Table Storage

# Cada comando deve retornar informações do processo
```

### 3.3 Compilar o Projeto .NET (Terminal 2)

```bash
# Navegar para o projeto
cd exemplo2

# Restaurar dependências
dotnet restore

# Compilar projeto
dotnet build

# Verificar se compilou sem erros
echo "Build Status: $?"  # Deve retornar 0
```

### 3.4 Iniciar Azure Functions (Terminal 2)

```bash
# Iniciar Azure Functions
func start --port 7071

# Aguardar mensagens como:
# "Host started"
# "Http Functions:"
# Manter este terminal aberto
```

### 3.5 Verificar Se Functions Estão Ativas (Terminal 3)

```bash
# Verificar health check
curl -s http://localhost:7071/api/health

# Deve retornar JSON com status "Healthy"
```

## Passo 4: Criar Recursos no Azurite

### 4.1 Criar Fila de Processamento

```bash
# Criar fila
az storage queue create \
  --name task-processing-queue \
  --connection-string "$QUEUE_CONN"

# Verificar se foi criada
az storage queue list \
  --connection-string "$QUEUE_CONN" \
  --output table

# Deve mostrar a fila "task-processing-queue"
```

### 4.2 Criar Container para Blobs

```bash
# Criar container
az storage container create \
  --name task-attachments \
  --connection-string "$BLOB_CONN"

# Verificar se foi criado
az storage container list \
  --connection-string "$BLOB_CONN" \
  --output table

# Deve mostrar o container "task-attachments"
```

## Passo 5: Testar API REST

### 5.1 Testar Health Check

```bash
# Health check básico
curl -s $BASE_URL/health | jq .

# Health check detalhado
curl -s $BASE_URL/health/detailed | jq .

# Ambos devem retornar status "Healthy"
```

### 5.2 Criar Tarefa de Teste

```bash
# Criar nova tarefa
TASK_RESPONSE=$(curl -s -X POST $BASE_URL/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Tarefa de Teste Azurite",
    "description": "Testando integração com Azurite",
    "tags": ["teste", "azurite"]
  }')

# Exibir resposta
echo $TASK_RESPONSE | jq .

# Extrair ID da tarefa
TASK_ID=$(echo $TASK_RESPONSE | jq -r .data.id)
echo "Task ID: $TASK_ID"
```

### 5.3 Listar Tarefas

```bash
# Listar todas as tarefas
curl -s $BASE_URL/tasks | jq .

# Buscar tarefa específica
curl -s $BASE_URL/tasks/$TASK_ID | jq .
```

## Passo 6: Testar Queue Functions

### 6.1 Enviar Mensagem de Notificação

```bash
# Criar mensagem de notificação
NOTIFY_MESSAGE="{\"taskId\":\"$TASK_ID\",\"action\":\"notify_assignment\",\"parameters\":{\"assignedTo\":\"test@example.com\"},\"timestamp\":\"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"}"

# Enviar para a fila
az storage message put \
  --queue-name task-processing-queue \
  --content "$NOTIFY_MESSAGE" \
  --connection-string "$QUEUE_CONN"

echo "Mensagem de notificação enviada"
```

### 6.2 Verificar Processamento da Fila

```bash
# Aguardar processamento (5-10 segundos)
sleep 8

# Verificar se a tarefa foi atualizada
curl -s $BASE_URL/tasks/$TASK_ID | jq .data.status

# Status deve ter mudado para "Assigned"
```

### 6.3 Enviar Mensagem de Auto-conclusão

```bash
# Criar mensagem de conclusão
COMPLETE_MESSAGE="{\"taskId\":\"$TASK_ID\",\"action\":\"auto_complete\",\"parameters\":{},\"timestamp\":\"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"}"

# Enviar para a fila
az storage message put \
  --queue-name task-processing-queue \
  --content "$COMPLETE_MESSAGE" \
  --connection-string "$QUEUE_CONN"

echo "Mensagem de auto-conclusão enviada"
```

### 6.4 Verificar Auto-conclusão

```bash
# Aguardar processamento
sleep 8

# Verificar status final
curl -s $BASE_URL/tasks/$TASK_ID | jq .data.status

# Status deve ser "Completed"
```

## Passo 7: Testar Blob Functions

### 7.1 Testar Processamento de Arquivo de Texto

```bash
# Criar arquivo de descrição
echo "Esta é uma descrição adicional extraída automaticamente do arquivo de texto anexado à tarefa durante o teste do Azurite." > test-description.txt

# Upload do arquivo
az storage blob upload \
  --container-name task-attachments \
  --file test-description.txt \
  --name "${TASK_ID}_description.txt" \
  --connection-string "$BLOB_CONN"

echo "Arquivo de texto enviado para processamento"
```

### 7.2 Testar Processamento de Arquivo JSON

```bash
# Criar arquivo de metadados
echo '{"tags": ["blob-processed", "automatic-extraction", "azure-functions", "test-completed"]}' > test-tags.json

# Upload do arquivo
az storage blob upload \
  --container-name task-attachments \
  --file test-tags.json \
  --name "${TASK_ID}_metadata.json" \
  --connection-string "$BLOB_CONN"

echo "Arquivo JSON enviado para processamento"
```

### 7.3 Testar Processamento de Arquivo Markdown

```bash
# Criar arquivo de notas
cat > test-notes.md << EOF
# Notas do Teste Automatizado

Este arquivo foi processado pela **Blob Function**.

## Validações Realizadas
- Queue Function: OK
- Blob Function: OK
- API REST: OK
- Azurite Integration: OK
EOF

# Upload do arquivo
az storage blob upload \
  --container-name task-attachments \
  --file test-notes.md \
  --name "${TASK_ID}_notes.md" \
  --connection-string "$BLOB_CONN"

echo "Arquivo Markdown enviado para processamento"
```

### 7.4 Verificar Processamento dos Blobs

```bash
# Aguardar processamento (blobs têm latência maior)
sleep 12

# Verificar se a descrição foi atualizada
UPDATED_TASK=$(curl -s $BASE_URL/tasks/$TASK_ID)
echo "Descrição atualizada:"
echo $UPDATED_TASK | jq .data.description

# Verificar se as tags foram extraídas
echo "Tags extraídas:"
echo $UPDATED_TASK | jq .data.tags
```

## Passo 8: Validações Finais

### 8.1 Verificar Estado Final da Tarefa

```bash
# Exibir estado completo da tarefa
curl -s $BASE_URL/tasks/$TASK_ID | jq .data

# Verificar campos que devem ter sido atualizados:
# - status: "Completed"
# - description: deve conter texto do arquivo .txt
# - tags: deve conter tags do arquivo .json
```

### 8.2 Verificar Recursos Criados no Azurite

```bash
# Listar filas
echo "Filas criadas:"
az storage queue list --connection-string "$QUEUE_CONN" --output table

# Listar blobs
echo "Blobs criados:"
az storage blob list --container-name task-attachments --connection-string "$BLOB_CONN" --output table

# Deve mostrar 3 arquivos: _description.txt, _metadata.json, _notes.md
```

### 8.3 Verificar Estatísticas do Sistema

```bash
# Estatísticas detalhadas
curl -s $BASE_URL/health/detailed | jq .

# Verificar:
# - overallStatus: "Healthy"
# - statistics.total: número de tarefas
# - services: status dos serviços
```

## Passo 9: Limpeza e Finalização

### 9.1 Limpar Arquivos Temporários

```bash
# Remover arquivos de teste
rm -f test-description.txt test-tags.json test-notes.md

echo "Arquivos temporários removidos"
```

### 9.2 Parar Serviços (Opcional)

```bash
# Para parar Azurite (Terminal 1)
# Pressione Ctrl+C

# Para parar Azure Functions (Terminal 2)  
# Pressione Ctrl+C

# Verificar se as portas foram liberadas
lsof -i :7071  # Deve estar vazio
lsof -i :10000 # Deve estar vazio
```

## Passo 10: Teste Automatizado Completo

### 10.1 Opção Fácil: Script Completo

**RECOMENDADO**: Use o script que faz tudo automaticamente:

```bash
cd /Users/thiagoadriano/fiap/serverless/aula4/exemplo2
./prepare-and-test.sh
```

Este script:
- Valida todas as dependências obrigatórias
- Inicia Azurite automaticamente (se necessário)
- Compila o projeto .NET
- Inicia Azure Functions automaticamente (se necessário)
- Executa o teste automatizado
- Mostra resumo completo

### 10.2 Opção Manual: Pré-requisitos Obrigatórios

**Se quiser fazer manualmente**: O script `test-azurite.sh` não inicia os serviços automaticamente. Você DEVE seguir estes passos antes de executá-lo:

```bash
# 1. Validar dependências críticas
echo "Verificando pré-requisitos..."

# Verificar .NET 8
DOTNET_VERSION=$(dotnet --version 2>/dev/null | cut -d. -f1)
if [ "$DOTNET_VERSION" != "8" ]; then
    echo "ERRO: .NET 8 SDK é obrigatório"
    echo "Instale: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi
echo "SUCESSO: .NET 8 SDK: $(dotnet --version)"

# Verificar Azure Functions Core Tools v4
FUNC_VERSION=$(func --version 2>/dev/null | cut -d. -f1)
if [ "$FUNC_VERSION" != "4" ]; then
    echo "ERRO: Azure Functions Core Tools v4 é obrigatório"
    echo "Execute: npm install -g azure-functions-core-tools@4 --unsafe-perm true"
    exit 1
fi
echo "SUCESSO: Azure Functions Core Tools: $(func --version)"

# Verificar outras dependências
command -v curl >/dev/null 2>&1 || { echo "ERRO: curl não encontrado"; exit 1; }
command -v jq >/dev/null 2>&1 || { echo "ERRO: jq não encontrado (brew install jq)"; exit 1; }
command -v az >/dev/null 2>&1 || { echo "ERRO: Azure CLI não encontrado"; exit 1; }
command -v azurite >/dev/null 2>&1 || { echo "ERRO: Azurite não encontrado (npm install -g azurite)"; exit 1; }

echo "SUCESSO: Todas as dependências estão instaladas"
```

### 10.3 Iniciar Serviços Obrigatórios

```bash
# 2. Iniciar Azurite (Terminal 1)
echo "Iniciando Azurite..."
mkdir -p ~/azurite
azurite --silent --location ~/azurite &
AZURITE_PID=$!

# Aguardar Azurite inicializar
echo "Aguardando Azurite..."
for i in {1..30}; do
    if lsof -i :10000 >/dev/null 2>&1 && lsof -i :10001 >/dev/null 2>&1; then
        echo "SUCESSO: Azurite iniciado com sucesso (PID: $AZURITE_PID)"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "ERRO: Timeout aguardando Azurite"
        exit 1
    fi
    sleep 1
done

# 3. Navegar para o projeto e compilar
echo "Compilando projeto..."
cd /Users/thiagoadriano/fiap/serverless/aula4/exemplo2
if ! dotnet build >/dev/null 2>&1; then
    echo "ERRO: Falha na compilação"
    echo "Execute: dotnet build --verbosity normal"
    exit 1
fi
echo "SUCESSO: Projeto compilado com sucesso"

# 4. Iniciar Azure Functions (Terminal 2 - ou background)
echo "Iniciando Azure Functions..."
func start --port 7071 &
FUNCTIONS_PID=$!

# Aguardar Azure Functions inicializar
echo "Aguardando Azure Functions..."
for i in {1..60}; do
    if curl -s --max-time 5 http://localhost:7071/api/health >/dev/null 2>&1; then
        echo "SUCESSO: Azure Functions iniciado com sucesso (PID: $FUNCTIONS_PID)"
        break
    fi
    if [ $i -eq 60 ]; then
        echo "ERRO: Timeout aguardando Azure Functions"
        echo "Verifique se o Azurite está rodando"
        exit 1
    fi
    sleep 2
done

# 5. Verificação final antes do teste
echo "Verificação final..."
curl -s http://localhost:7071/api/health | jq .status
if [ $? -eq 0 ]; then
    echo "SUCESSO: Sistema pronto para teste automatizado"
else
    echo "ERRO: Sistema não está saudável"
    exit 1
fi
```

### 10.4 Executar Script Automatizado

```bash
# Agora executar o script de teste
echo "Executando teste automatizado..."
./test-azurite.sh

# O script executará todos os passos automaticamente
# Observe as mensagens de SUCESSO e ERRO
```

### 10.5 Script de Preparação Completa (Referência)

Aqui está um script completo que faz toda a preparação:

```bash
#!/bin/bash
# prepare-and-test.sh - Script completo de preparação e teste

set -e

echo "Preparando ambiente para teste automatizado do Azurite..."

# 1. Validações
echo "1. Validando dependências..."
DOTNET_VERSION=$(dotnet --version 2>/dev/null | cut -d. -f1)
FUNC_VERSION=$(func --version 2>/dev/null | cut -d. -f1)

[ "$DOTNET_VERSION" = "8" ] || { echo "ERRO: .NET 8 obrigatório"; exit 1; }
[ "$FUNC_VERSION" = "4" ] || { echo "ERRO: Azure Functions Core Tools v4 obrigatório"; exit 1; }
command -v curl >/dev/null || { echo "ERRO: curl obrigatório"; exit 1; }
command -v jq >/dev/null || { echo "ERRO: jq obrigatório"; exit 1; }
command -v az >/dev/null || { echo "ERRO: Azure CLI obrigatório"; exit 1; }
command -v azurite >/dev/null || { echo "ERRO: Azurite obrigatório"; exit 1; }

echo "SUCESSO: Dependências validadas"

# 2. Verificar se já estão rodando
if lsof -i :7071 >/dev/null 2>&1; then
    echo "AVISO: Azure Functions já está rodando na porta 7071"
    read -p "Continuar mesmo assim? (y/N): " -n 1 -r
    echo
    [[ ! $REPLY =~ ^[Yy]$ ]] && exit 0
fi

if lsof -i :10000 >/dev/null 2>&1; then
    echo "AVISO: Azurite já está rodando na porta 10000"
else
    # 3. Iniciar Azurite
    echo "2. Iniciando Azurite..."
    mkdir -p ~/azurite
    azurite --silent --location ~/azurite &
    AZURITE_PID=$!
    
    # Aguardar inicialização
    for i in {1..30}; do
        if lsof -i :10000 >/dev/null 2>&1; then
            echo "SUCESSO: Azurite iniciado (PID: $AZURITE_PID)"
            break
        fi
        [ $i -eq 30 ] && { echo "ERRO: Timeout Azurite"; exit 1; }
        sleep 1
    done
fi

# 4. Compilar projeto
echo "3. Compilando projeto..."
cd /Users/thiagoadriano/fiap/serverless/aula4/exemplo2
dotnet build >/dev/null 2>&1 || { echo "ERRO: Erro compilação"; exit 1; }
echo "SUCESSO: Projeto compilado"

# 5. Iniciar Azure Functions
if ! curl -s --max-time 5 http://localhost:7071/api/health >/dev/null 2>&1; then
    echo "4. Iniciando Azure Functions..."
    func start --port 7071 &
    FUNCTIONS_PID=$!
    
    # Aguardar inicialização
    for i in {1..60}; do
        if curl -s --max-time 5 http://localhost:7071/api/health >/dev/null 2>&1; then
            echo "SUCESSO: Azure Functions iniciado (PID: $FUNCTIONS_PID)"
            break
        fi
        [ $i -eq 60 ] && { echo "ERRO: Timeout Azure Functions"; exit 1; }
        sleep 2
    done
else
    echo "SUCESSO: Azure Functions já está rodando"
fi

# 6. Verificação final
echo "5. Verificação final..."
STATUS=$(curl -s http://localhost:7071/api/health | jq -r .status 2>/dev/null)
if [ "$STATUS" = "Healthy" ]; then
    echo "SUCESSO: Sistema pronto!"
else
    echo "ERRO: Sistema não está saudável: $STATUS"
    exit 1
fi

# 7. Executar teste
echo "6. Executando teste automatizado..."
echo "================================================"
./test-azurite.sh

echo ""
echo "Teste concluído!"
echo "Para parar os serviços iniciados:"
[ ! -z "$AZURITE_PID" ] && echo "   Azurite: kill $AZURITE_PID"
[ ! -z "$FUNCTIONS_PID" ] && echo "   Functions: kill $FUNCTIONS_PID"
```

### 10.2 Interpretar Resultados do Script

**Mensagens de SUCESSO indicam:**
- Dependências instaladas corretamente
- Serviços rodando
- Recursos criados no Azurite
- Queue Functions processando
- Blob Functions processando
- API REST funcionando

**Mensagens de ERRO podem indicar:**
- Serviços não iniciados
- Dependências faltando
- Problemas de conectividade
- Configuração incorreta

## Comandos de Referência Rápida

### Verificações Essenciais

```bash
# Verificar se tudo está funcionando
curl -s $BASE_URL/health | jq .status                    # Deve retornar "Healthy"
lsof -i :7071                                           # Azure Functions ativo
lsof -i :10000                                          # Azurite Blob ativo  
lsof -i :10001                                          # Azurite Queue ativo

# Listar recursos do Azurite
az storage queue list --connection-string "$QUEUE_CONN" --output table
az storage container list --connection-string "$BLOB_CONN" --output table
```

### Comandos para Debugging

```bash
# Ver logs detalhados do sistema
curl -s $BASE_URL/health/detailed | jq .

# Verificar se Azure Functions compilou corretamente
dotnet build

# Verificar configuração do projeto
cat local.settings.json

# Verificar se Azurite tem dados
az storage blob list --container-name task-attachments --connection-string "$BLOB_CONN"
```

### Fluxo Completo em Um Script

```bash
#!/bin/bash
# Script rápido de validação

echo "1. Verificando dependências..."
dotnet --version && func --version && az --version && jq --version

echo "2. Configurando variáveis..."
export CONN_STRING="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
export QUEUE_CONN="${CONN_STRING};QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;"
export BLOB_CONN="${CONN_STRING};BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
export BASE_URL="http://localhost:7071/api"

echo "3. Verificando serviços..."
curl -s $BASE_URL/health > /dev/null && echo "✓ Azure Functions OK" || echo "✗ Azure Functions ERRO"
lsof -i :10000 > /dev/null && echo "✓ Azurite OK" || echo "✗ Azurite ERRO"

echo "4. Criando recursos..."
az storage queue create --name task-processing-queue --connection-string "$QUEUE_CONN" > /dev/null 2>&1
az storage container create --name task-attachments --connection-string "$BLOB_CONN" > /dev/null 2>&1

echo "5. Testando API..."
TASK_RESPONSE=$(curl -s -X POST $BASE_URL/tasks -H "Content-Type: application/json" -d '{"title":"Teste Rápido","description":"Validação","tags":["teste"]}')
TASK_ID=$(echo $TASK_RESPONSE | jq -r .data.id)
echo "Task criada: $TASK_ID"

echo "6. Testando Queue..."
QUEUE_MESSAGE="{\"taskId\":\"$TASK_ID\",\"action\":\"auto_complete\",\"parameters\":{},\"timestamp\":\"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"}"
az storage message put --queue-name task-processing-queue --content "$QUEUE_MESSAGE" --connection-string "$QUEUE_CONN"
sleep 5
STATUS=$(curl -s $BASE_URL/tasks/$TASK_ID | jq -r .data.status)
echo "Status da tarefa: $STATUS"

echo "7. Testando Blob..."
echo "Teste de blob" > test-file.txt
az storage blob upload --container-name task-attachments --file test-file.txt --name "${TASK_ID}_test.txt" --connection-string "$BLOB_CONN"
sleep 8
rm test-file.txt

echo "8. Resultado final:"
curl -s $BASE_URL/tasks/$TASK_ID | jq .data
```

## Troubleshooting - Problemas Comuns

### Erro: "curl: command not found"

```bash
# Instalar curl
# macOS: brew install curl
# Ubuntu: sudo apt-get install curl
# Windows: winget install curl
```

### Erro: "jq: command not found"

```bash
# Instalar jq
# macOS: brew install jq
# Ubuntu: sudo apt-get install jq
# Windows: winget install jqlang.jq
```

### Erro: "azurite: command not found"

```bash
# Instalar Azurite
npm install -g azurite

# Verificar instalação
azurite --version
```

### Erro: "func: command not found"

```bash
# Instalar Azure Functions Core Tools
npm install -g azure-functions-core-tools@4 --unsafe-perm true

# Verificar instalação
func --version
```

### Erro: "Port 7071 is already in use"

```bash
# Encontrar processo usando a porta
lsof -i :7071

# Matar processo (substituir PID)
kill -9 PID

# Ou usar porta alternativa
func start --port 7072
```

### Erro: Azure Functions não compila

```bash
# Limpar e rebuild
dotnet clean
dotnet restore
dotnet build

# Verificar erros de compilação
dotnet build --verbosity normal
```

### Erro: "Azurite não está rodando"

```bash
# Verificar se está instalado
azurite --version

# Criar diretório se não existir
mkdir -p ~/azurite

# Iniciar Azurite
azurite --silent --location ~/azurite

# Verificar portas (aguardar alguns segundos)
sleep 5
lsof -i :10000  # Blob Storage
lsof -i :10001  # Queue Storage  
lsof -i :10002  # Table Storage

# Se as portas não estiverem ativas, verificar erros
ps aux | grep azurite
```

### Erro: "Azure Functions falha ao conectar com Azurite"

```bash
# Erro típico: "Connection refused (127.0.0.1:10000)"
# Isso acontece quando Azure Functions inicia antes do Azurite

# 1. Verificar se Azurite está rodando PRIMEIRO
lsof -i :10000 || echo "Azurite não está rodando!"

# 2. Se Azurite não estiver rodando, iniciar primeiro
azurite --silent --location ~/azurite &
sleep 5

# 3. Só depois iniciar Azure Functions
func start --port 7071

# 4. Verificar health check
curl -s http://localhost:7071/api/health | jq .
```

### Erro: ".NET 8 SDK obrigatório"

```bash
# Verificar versão atual
dotnet --version

# O projeto requer .NET 8 especificamente
DOTNET_VERSION=$(dotnet --version | cut -d. -f1)
if [ "$DOTNET_VERSION" != "8" ]; then
    echo "Instale .NET 8: https://dotnet.microsoft.com/download/dotnet/8.0"
fi

# Verificar se múltiplas versões estão instaladas
dotnet --list-sdks
```

### Erro: "Azure Functions Core Tools v4 obrigatório"

```bash
# Verificar versão atual
func --version

# O projeto requer v4 especificamente
FUNC_VERSION=$(func --version | cut -d. -f1)
if [ "$FUNC_VERSION" != "4" ]; then
    echo "Instale v4: npm install -g azure-functions-core-tools@4 --unsafe-perm true"
fi

# Desinstalar versões antigas se necessário
npm uninstall -g azure-functions-core-tools
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

### Erro: Queue Functions não processam

```bash
# Verificar connection string
echo $QUEUE_CONN

# Verificar se a fila existe
az storage queue list --connection-string "$QUEUE_CONN"

# Verificar logs do Azure Functions (Terminal 2)
# Procurar por erros ou mensagens de processamento
```

### Erro: Blob Functions não processam

```bash
# Verificar container
az storage container list --connection-string "$BLOB_CONN"

# Verificar se blobs foram criados
az storage blob list --container-name task-attachments --connection-string "$BLOB_CONN"

# Aguardar mais tempo (blobs têm latência maior)
sleep 15
```

### Erro: API retorna 500

```bash
# Verificar logs detalhados
curl -s $BASE_URL/health/detailed | jq .

# Verificar se Azure Functions está saudável
curl -s $BASE_URL/health

# Verificar logs no terminal do Azure Functions
```

## Resumo dos Comandos de Validação

### Checklist Rápido

```bash
# 1. Dependências
dotnet --version      # .NET 8
func --version        # Azure Functions Core Tools 4.x
az --version          # Azure CLI
jq --version          # JSON processor
azurite --version     # Azurite emulator

# 2. Serviços Ativos
lsof -i :7071         # Azure Functions
lsof -i :10000        # Azurite Blob
lsof -i :10001        # Azurite Queue

# 3. API Funcionando
curl -s $BASE_URL/health | jq .status
# Deve retornar: "Healthy"

# 4. Recursos Criados
az storage queue list --connection-string "$QUEUE_CONN"
az storage container list --connection-string "$BLOB_CONN"
```

### Comandos de Reset

```bash
# Limpar tudo e recomeçar
pkill -f "azurite"
pkill -f "func"
pkill -f "dotnet"

# Aguardar alguns segundos
sleep 3

# Reiniciar serviços
# Terminal 1: azurite --silent --location ~/azurite  
# Terminal 2: cd exemplo2 && func start --port 7071
```

## Conclusão

Este guia prático permite testar cada componente individualmente:

1. **Validação de Instalações** - Confirma que todas as dependências estão corretas
2. **Teste de Serviços** - Valida que Azurite e Azure Functions estão funcionando
3. **Teste de API REST** - Confirma que endpoints HTTP estão respondendo
4. **Teste de Queue Functions** - Valida processamento de mensagens assíncronas
5. **Teste de Blob Functions** - Confirma processamento de arquivos automatizado
6. **Validação End-to-End** - Testa o fluxo completo integrado

Use este guia para entender cada parte do sistema e diagnosticar problemas específicos antes de executar o teste automatizado completo.
