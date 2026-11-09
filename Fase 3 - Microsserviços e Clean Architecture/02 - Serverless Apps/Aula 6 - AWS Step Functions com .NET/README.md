# AWS Step Functions com LocalStack - Fluxo de Cadastro de Cliente

Este projeto demonstra como implementar um fluxo completo usando AWS Step Functions com LocalStack, simulando um processo de cadastro de cliente que inclui validação, verificação de duplicidade, criação do cliente e envio de email de boas-vindas.

## Arquitetura do Fluxo

O workflow é composto por 4 Lambda Functions em .NET 8 que se comunicam através de um Step Functions:

1. **ValidateDataFunction**: Valida os dados de entrada (CPF/CNPJ, email, telefone)
2. **CheckCustomerFunction**: Verifica se o cliente já existe no DynamoDB
3. **CreateCustomerFunction**: Cria um novo cliente no DynamoDB
4. **SendWelcomeEmailFunction**: Envia email de boas-vindas via SNS

### Recursos AWS Utilizados

- **AWS Lambda**: 4 funções serverless em .NET 8
- **AWS Step Functions**: Orquestração do workflow
- **Amazon DynamoDB**: Armazenamento dos dados dos clientes
- **Amazon SNS**: Envio de notificações por email
- **LocalStack**: Simulação local dos serviços AWS

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://docs.docker.com/get-docker/)
- [AWS CLI v2](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html)
- [LocalStack](https://docs.localstack.cloud/getting-started/installation/)
- [jq](https://jqlang.github.io/jq/download/) (para formatação de JSON nos testes)

## Passo a Passo

### 1. Clonar e Preparar o Projeto

```bash
# Navegar para o diretório do projeto
cd /Users/thiagoadriano/fiap/serverless/aula6

# Verificar se todos os arquivos estão presentes
ls -la
```

### 2. Iniciar o LocalStack

```bash
# Iniciar o LocalStack com Docker
docker run --rm -it -p 4566:4566 -p 4571:4571 localstack/localstack
```

**Aguarde até ver a mensagem indicando que o LocalStack está ready.**

### 3. Build e Deploy das Funções

Em outro terminal, execute:

```bash
# Navegar para o diretório do projeto
cd /Users/thiagoadriano/fiap/serverless/aula6

# Executar o script de build e deploy
./scripts/build-and-deploy.sh
```

Este script irá:
- Compilar todas as funções Lambda
- Criar os arquivos ZIP para deploy
- Criar a tabela DynamoDB
- Criar o tópico SNS
- Deploy das 4 funções Lambda
- Criar o Step Functions workflow

### 4. Testar o Workflow

```bash
# Executar os testes automatizados
./scripts/test-workflow.sh
```

Os testes incluem:
- ✅ Cadastro com CPF válido
- ✅ Cadastro com CNPJ válido
- ❌ Tentativa com CPF inválido
- ❌ Tentativa com email inválido
- ❌ Tentativa de cadastro duplicado

### 5. Monitorar os Resultados

#### Verificar dados no DynamoDB:
```bash
aws dynamodb scan \
    --table-name customers \
    --endpoint-url=http://localhost:4566 \
    --region=us-east-1
```

#### Verificar logs do SNS (mensagens de email):
```bash
docker logs $(docker ps -q --filter ancestor=localstack/localstack) | grep SNS
```

## Estrutura do Projeto

```
aula6/
├── CustomerRegistrationWorkflow.sln
├── README.md
├── src/
│   ├── Shared/
│   │   ├── Models/Customer.cs
│   │   ├── Utils/DocumentValidator.cs
│   │   └── Shared.csproj
│   ├── ValidateDataFunction/
│   │   ├── Function.cs
│   │   └── ValidateDataFunction.csproj
│   ├── CheckCustomerFunction/
│   │   ├── Function.cs
│   │   └── CheckCustomerFunction.csproj
│   ├── CreateCustomerFunction/
│   │   ├── Function.cs
│   │   └── CreateCustomerFunction.csproj
│   └── SendWelcomeEmailFunction/
│       ├── Function.cs
│       └── SendWelcomeEmailFunction.csproj
├── step-functions/
│   └── customer-registration-workflow.json
└── scripts/
    ├── build-and-deploy.sh
    ├── test-workflow.sh
    └── cleanup.sh
```

## Fluxo Detalhado

### 1. Entrada (Input)
```json
{
  "name": "João Silva",
  "email": "joao.silva@email.com", 
  "document": "123.456.789-09",
  "phone": "(11) 99999-9999"
}
```

### 2. ValidateDataFunction
- Valida formato do email
- Valida CPF/CNPJ usando algoritmo oficial
- Valida formato do telefone
- Valida nome (mínimo 2 caracteres)

### 3. CheckCustomerFunction  
- Consulta DynamoDB usando documento como chave
- Retorna se cliente já existe

### 4. CreateCustomerFunction
- Cria novo registro no DynamoDB
- Gera ID único para o cliente
- Usa condição para evitar duplicatas

### 5. SendWelcomeEmailFunction
- Publica mensagem no tópico SNS
- Inclui dados do cliente no email

## Possíveis Resultados

### ✅ Sucesso Completo
```json
{
  "result": {
    "status": "SUCCESS",
    "message": "Cliente cadastrado com sucesso e email de boas-vindas enviado"
  }
}
```

### ❌ Dados Inválidos
```json
{
  "result": {
    "status": "VALIDATION_ERROR", 
    "message": "Os dados fornecidos são inválidos"
  }
}
```

### ❌ Cliente Já Existe
```json
{
  "result": {
    "status": "CUSTOMER_EXISTS",
    "message": "Cliente já cadastrado no sistema"
  }
}
```

### ⚠️ Sucesso Parcial
```json
{
  "result": {
    "status": "PARTIAL_SUCCESS",
    "message": "Cliente criado com sucesso, mas houve erro no envio do email de boas-vindas"
  }
}
```

## Executar Testes Individuais

### Teste Manual via AWS CLI
```bash
# Configurar variáveis
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=us-east-1
export AWS_ENDPOINT_URL=http://localhost:4566

# Obter ARN da State Machine
STATE_MACHINE_ARN=$(aws stepfunctions list-state-machines \
    --query 'stateMachines[?name==`CustomerRegistrationWorkflow`].stateMachineArn' \
    --output text)

# Executar workflow
aws stepfunctions start-execution \
    --state-machine-arn $STATE_MACHINE_ARN \
    --name "teste-manual-$(date +%s)" \
    --input '{
      "name": "Maria Santos",
      "email": "maria@exemplo.com",
      "document": "987.654.321-00", 
      "phone": "(11) 88888-8888"
    }'
```

## Limpeza

Para remover todos os recursos criados:

```bash
./scripts/cleanup.sh
```

## Troubleshooting

### LocalStack não está rodando
```bash
# Verificar se está rodando
curl http://localhost:4566/health

# Se não estiver, iniciar
docker run --rm -it -p 4566:4566 localstack/localstack
```

### Erro de build do .NET
```bash
# Verificar versão do .NET
dotnet --version

# Limpar cache
dotnet clean
dotnet restore
```

### AWS CLI não encontra recursos
```bash
# Verificar configuração
echo $AWS_ENDPOINT_URL
echo $AWS_DEFAULT_REGION

# Reconfigurar se necessário
export AWS_ENDPOINT_URL=http://localhost:4566
export AWS_DEFAULT_REGION=us-east-1
```

## 📚 Documentação Técnica Detalhada

Para informações técnicas aprofundadas, consulte a pasta [`docs/`](docs/):

- **[`docs/funcoes-lambda.md`](docs/funcoes-lambda.md)** - Documentação detalhada de cada função Lambda com diagramas de sequência
- **[`docs/arquivos-projeto.md`](docs/arquivos-projeto.md)** - Explicação completa de todos os arquivos do projeto  
- **[`docs/README.md`](docs/README.md)** - Índice da documentação técnica

## Próximos Passos

Este exemplo pode ser estendido com:

- **Autenticação**: Integrar com AWS Cognito
- **Rate Limiting**: Implementar throttling nas APIs
- **Dead Letter Queue**: Para tratamento de falhas
- **CloudWatch**: Métricas e logs customizados  
- **X-Ray**: Tracing distribuído
- **API Gateway**: Expor via REST API
- **EventBridge**: Integração com outros sistemas
- **Testes Unitários**: Implementar testes para as funções

## Contribuição

Este é um projeto educacional para demonstrar conceitos de serverless computing com AWS Step Functions e LocalStack.
