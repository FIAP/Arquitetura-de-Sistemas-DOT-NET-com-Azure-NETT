# Projeto Lambda Test - AWS .NET

Este é um projeto de exemplo usando .NET 8 para AWS Lambda com API Gateway.

## Pré-requisitos

- .NET 8 SDK
- Docker
- Python (para awscli-local)

## Passo a Passo - LocalStack

### 1. Instalar dependências

```bash
pip install awscli-local
dotnet restore
```

### 2. Subir LocalStack

```bash
docker-compose up -d
```

### 3. Deploy e teste

```bash
./deploy-localstack.sh
./test-localstack.sh
```

### 4. Usar no VS Code

Copie a URL mostrada pelo deploy e cole em `test-requests.http`:
```
@baseUrl = http://localhost:4566/_aws/execute-api/SEU_API_ID/dev
```

### 5. Parar LocalStack

```bash
# Se usou docker-compose
docker-compose down

# Se usou docker direto
docker stop localstack
```

## Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/health` | Health check |
| GET | `/get-data` | Dados de exemplo |
| POST | `/process-order` | Processar pedido |

## Arquivos do Projeto

- `Function.cs` - Código da função Lambda
- `LambdaTestProject.csproj` - Configuração do projeto .NET
- `deploy-localstack.sh` - Script de deploy para LocalStack
- `test-localstack.sh` - Script de teste dos endpoints
- `docker-compose.yml` - Configuração do LocalStack
- `test-requests.http` - Exemplos de requisições HTTP

