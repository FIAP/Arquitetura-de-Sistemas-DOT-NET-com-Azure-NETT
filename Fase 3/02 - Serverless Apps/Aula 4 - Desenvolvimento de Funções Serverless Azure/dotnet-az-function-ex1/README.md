# Azure Function .NET - Exemplo Simples

Este é um exemplo básico de Azure Function em .NET 8 com HTTP Trigger usando o modelo Isolated Worker.

## 🏗️ Estrutura do Projeto

```
exemplo1/
├── AzureFunctionExample.csproj  # Arquivo de projeto .NET
├── Program.cs                   # Ponto de entrada da aplicação
├── HttpTriggerFunction.cs       # Função HTTP trigger
├── host.json                    # Configuração do Azure Functions runtime
├── local.settings.json          # Configurações locais
├── README.md                    # Este arquivo
└── .gitignore                   # Arquivos ignorados pelo Git
```

## 🚀 Funcionalidades

A função `HttpTriggerExample` suporta:
- **GET**: Retorna uma mensagem de saudação personalizada
- **POST**: Processa dados JSON enviados no corpo da requisição
- **Tratamento de Erros**: Respostas adequadas para métodos não suportados e JSON inválido

### 📝 Exemplos de Uso

#### GET Request
```bash
# Simples
curl http://localhost:7071/api/HttpTriggerExample

# Com parâmetro name
curl "http://localhost:7071/api/HttpTriggerExample?name=João"
```

#### POST Request
```bash
# Enviando JSON
curl -X POST http://localhost:7071/api/HttpTriggerExample \
  -H "Content-Type: application/json" \
  -d '{"name": "Maria", "idade": 25}'
```

## 🛠️ Pré-requisitos

- **.NET 8 SDK** ou superior
- **Azure Functions Core Tools v4**
- **Azure CLI** (opcional, para deploy)

## 📋 Instalação e Execução

### 1. Instalar .NET 8 SDK
```bash
# Verificar se já está instalado
dotnet --version

# Se não estiver instalado, baixe de: https://dotnet.microsoft.com/download
```

### 2. Instalar Azure Functions Core Tools
```bash
# macOS (usando Homebrew)
brew tap azure/functions
brew install azure-functions-core-tools@4

# ou usando npm
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

### 3. Restaurar Dependências
```bash
dotnet restore
```

### 4. Executar Localmente
```bash
# Compilar o projeto
dotnet build

# Executar as funções
func start
```

A função estará disponível em: `http://localhost:7071/api/HttpTriggerExample`

## 📊 Exemplo de Resposta

### GET Response
```json
{
  "message": "Olá, Thiago Adriano! Esta é uma Azure Function .NET com GET.",
  "method": "GET",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "framework": ".NET 8"
}
```

### POST Response
```json
{
  "message": "Dados recebidos via POST! Olá, Juliana!",
  "method": "POST",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "framework": ".NET 8",
  "receivedData": {
    "name": "Juliana",
    "idade": 39
  }
}
```

## 🚀 Deploy para Azure

### 1. Fazer Login no Azure
```bash
az login
```

### 2. Criar Recursos no Azure
```bash
# Criar resource group
az group create --name myResourceGroup --location eastus

# Criar storage account
az storage account create \
  --name mystorageaccount \
  --location eastus \
  --resource-group myResourceGroup \
  --sku Standard_LRS

# Criar function app
az functionapp create \
  --resource-group myResourceGroup \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4 \
  --name myFunctionApp \
  --storage-account mystorageaccount
```

### 3. Deploy da Função
```bash
func azure functionapp publish myFunctionApp
```

## 🧪 Testando a Função

### Usando curl
```bash
# GET com parâmetro
curl "https://myfunctionapp.azurewebsites.net/api/HttpTriggerExample?name=Teste"

# POST com JSON
curl -X POST https://myfunctionapp.azurewebsites.net/api/HttpTriggerExample \
  -H "Content-Type: application/json" \
  -d '{"name": "Azure", "version": "v4"}'
```