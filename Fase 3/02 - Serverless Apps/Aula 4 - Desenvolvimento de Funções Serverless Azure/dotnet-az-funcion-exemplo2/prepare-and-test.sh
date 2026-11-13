#!/bin/bash
# prepare-and-test.sh - Script completo de preparação e teste do Azurite
# Execute este script para preparar o ambiente e executar o teste automatizado

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_step() {
    echo -e "${BLUE}$1${NC}"
}

print_success() {
    echo -e "${GREEN}SUCESSO: $1${NC}"
}

print_error() {
    echo -e "${RED}ERRO: $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}AVISO: $1${NC}"
}

echo "Preparando ambiente para teste automatizado do Azurite..."
echo ""

# 1. Validações
print_step "1. Validando dependências obrigatórias..."

# Verificar .NET 8
if ! command -v dotnet >/dev/null 2>&1; then
    print_error ".NET SDK não encontrado"
    echo "Instale: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version 2>/dev/null | cut -d. -f1)
if [ "$DOTNET_VERSION" != "8" ]; then
    print_error ".NET 8 SDK é obrigatório"
    echo "Versão atual: $(dotnet --version 2>/dev/null || echo 'não encontrada')"
    echo "Instale: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi
print_success ".NET 8 SDK: $(dotnet --version)"

# Verificar Azure Functions Core Tools v4
if ! command -v func >/dev/null 2>&1; then
    print_error "Azure Functions Core Tools não encontrado"
    echo "Execute: npm install -g azure-functions-core-tools@4 --unsafe-perm true"
    exit 1
fi

FUNC_VERSION=$(func --version 2>/dev/null | cut -d. -f1)
if [ "$FUNC_VERSION" != "4" ]; then
    print_error "Azure Functions Core Tools v4 é obrigatório"
    echo "Versão atual: $(func --version 2>/dev/null || echo 'não encontrada')"
    echo "Execute: npm install -g azure-functions-core-tools@4 --unsafe-perm true"
    exit 1
fi
print_success "Azure Functions Core Tools: $(func --version)"

# Verificar outras dependências
if ! command -v curl >/dev/null 2>&1; then
    print_error "curl não encontrado"
    exit 1
fi
print_success "curl: $(curl --version | head -1)"

if ! command -v jq >/dev/null 2>&1; then
    print_error "jq não encontrado"
    echo "macOS: brew install jq"
    echo "Ubuntu: sudo apt-get install jq"
    exit 1
fi
print_success "jq: $(jq --version)"

if ! command -v az >/dev/null 2>&1; then
    print_error "Azure CLI não encontrado"
    echo "Instale: https://docs.microsoft.com/cli/azure/install-azure-cli"
    exit 1
fi
print_success "Azure CLI: $(az --version | head -1 | cut -d' ' -f2)"

if ! command -v azurite >/dev/null 2>&1; then
    print_error "Azurite não encontrado"
    echo "Execute: npm install -g azurite"
    exit 1
fi
print_success "Azurite: $(azurite --version)"

echo ""

# 2. Verificar se já estão rodando
print_step "2. Verificando serviços ativos..."

AZURITE_RUNNING=false
FUNCTIONS_RUNNING=false

if lsof -i :7071 >/dev/null 2>&1; then
    print_warning "Azure Functions já está rodando na porta 7071"
    FUNCTIONS_RUNNING=true
fi

if lsof -i :10000 >/dev/null 2>&1; then
    print_warning "Azurite já está rodando na porta 10000"
    AZURITE_RUNNING=true
fi

# 3. Iniciar Azurite se necessário
if [ "$AZURITE_RUNNING" = false ]; then
    print_step "3. Iniciando Azurite..."
    mkdir -p ~/azurite
    azurite --silent --location ~/azurite &
    AZURITE_PID=$!
    
    # Aguardar inicialização
    print_step "Aguardando Azurite inicializar..."
    for i in {1..30}; do
        if lsof -i :10000 >/dev/null 2>&1 && lsof -i :10001 >/dev/null 2>&1; then
            print_success "Azurite iniciado (PID: $AZURITE_PID)"
            print_success "  - Blob Storage: http://127.0.0.1:10000"
            print_success "  - Queue Storage: http://127.0.0.1:10001"
            print_success "  - Table Storage: http://127.0.0.1:10002"
            break
        fi
        if [ $i -eq 30 ]; then
            print_error "Timeout aguardando Azurite inicializar"
            exit 1
        fi
        sleep 1
    done
else
    print_success "Azurite já está rodando"
fi

echo ""

# 4. Verificar se estamos no diretório correto
print_step "4. Verificando diretório do projeto..."
if [ ! -f "host.json" ] || [ ! -f "TaskManagement.Functions.csproj" ]; then
    print_error "Execute o script do diretório do projeto Azure Functions"
    echo "Caminho esperado: /Users/thiagoadriano/fiap/serverless/aula4/exemplo2"
    echo "Arquivos necessários: host.json, TaskManagement.Functions.csproj"
    exit 1
fi
print_success "Diretório do projeto verificado"

# 5. Compilar projeto
print_step "5. Compilando projeto .NET..."
if ! dotnet build >/dev/null 2>&1; then
    print_error "Falha na compilação do projeto .NET"
    echo ""
    echo "Para ver detalhes do erro:"
    echo "dotnet build --verbosity normal"
    exit 1
fi
print_success "Projeto compilado com sucesso"

echo ""

# 6. Iniciar Azure Functions se necessário
if [ "$FUNCTIONS_RUNNING" = false ]; then
    print_step "6. Iniciando Azure Functions..."
    
    # Verificar se a porta está livre
    if lsof -i :7071 >/dev/null 2>&1; then
        print_error "Porta 7071 está em uso"
        echo "Execute: lsof -i :7071 e mate o processo"
        exit 1
    fi
    
    func start --port 7071 >/dev/null 2>&1 &
    FUNCTIONS_PID=$!
    
    # Aguardar inicialização
    print_step "Aguardando Azure Functions inicializar..."
    for i in {1..60}; do
        if curl -s --max-time 5 http://localhost:7071/api/health >/dev/null 2>&1; then
            print_success "Azure Functions iniciado (PID: $FUNCTIONS_PID)"
            print_success "  - Endpoint: http://localhost:7071/api"
            break
        fi
        if [ $i -eq 60 ]; then
            print_error "Timeout aguardando Azure Functions inicializar"
            print_warning "Verifique se o Azurite está rodando e tente novamente"
            exit 1
        fi
        sleep 2
    done
else
    print_success "Azure Functions já está rodando"
fi

echo ""

# 7. Verificação final
print_step "7. Verificação final do sistema..."
STATUS=$(curl -s http://localhost:7071/api/health | jq -r .status 2>/dev/null)
if [ "$STATUS" = "Healthy" ]; then
    print_success "Sistema está saudável e pronto para testes"
else
    print_error "Sistema não está saudável: $STATUS"
    echo ""
    echo "Para diagnosticar:"
    echo "curl -s http://localhost:7071/api/health/detailed | jq ."
    exit 1
fi

echo ""

# 8. Executar teste automatizado
print_step "8. Executando teste automatizado..."
echo "================================================"

if [ ! -f "./test-azurite.sh" ]; then
    print_error "Script test-azurite.sh não encontrado no diretório atual"
    exit 1
fi

if [ ! -x "./test-azurite.sh" ]; then
    print_step "Tornando test-azurite.sh executável..."
    chmod +x ./test-azurite.sh
fi

./test-azurite.sh

echo ""
echo "================================================"
echo ""

# 9. Resumo final
print_step "Teste automatizado concluído com sucesso!"
echo ""
echo -e "${BLUE}SERVIÇOS INICIADOS PELO SCRIPT:${NC}"
if [ ! -z "$AZURITE_PID" ]; then
    echo -e "${GREEN}   Azurite (PID: $AZURITE_PID) - http://127.0.0.1:10000${NC}"
    echo "     Para parar: kill $AZURITE_PID"
fi
if [ ! -z "$FUNCTIONS_PID" ]; then
    echo -e "${GREEN}   Azure Functions (PID: $FUNCTIONS_PID) - http://localhost:7071/api${NC}"
    echo "     Para parar: kill $FUNCTIONS_PID"
fi
if [ -z "$AZURITE_PID" ] && [ -z "$FUNCTIONS_PID" ]; then
    echo "   Nenhum serviço foi iniciado pelo script"
    echo "   Todos os serviços já estavam rodando"
fi

echo ""
echo -e "${YELLOW}COMANDOS ÚTEIS:${NC}"
echo "   Health check: curl -s http://localhost:7071/api/health | jq ."
echo "   Health detalhado: curl -s http://localhost:7071/api/health/detailed | jq ."
echo "   Verificar Azurite: lsof -i :10000"
echo "   Verificar Functions: lsof -i :7071"
echo "   Parar tudo: pkill -f azurite && pkill -f func"

echo ""
echo -e "${YELLOW}NOTA IMPORTANTE:${NC}"
echo "   Os serviços iniciados pelo script continuarão rodando após o término."
echo "   Para uma limpeza completa, execute os comandos 'kill' mostrados acima."

echo ""
echo -e "${GREEN}PREPARAÇÃO E TESTE COMPLETOS!${NC}"
