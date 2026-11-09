#!/bin/bash

# Script para deploy automatizado no LocalStack

set -e

echo "Deploy no LocalStack - Lambda Test Project"
echo "=============================================="

# Verificar se o LocalStack está rodando
if ! curl -s http://localhost:4566/_localstack/health > /dev/null; then
    echo "ERRO: LocalStack não está rodando. Inicie com:"
    echo "   docker run --rm -it -p 4566:4566 -p 4571:4571 -e SERVICES=lambda,apigateway,logs -e DEBUG=1 -v /var/run/docker.sock:/var/run/docker.sock localstack/localstack"
    exit 1
fi

echo "OK: LocalStack está rodando"

# Verificar se awslocal está instalado
if ! command -v awslocal &> /dev/null; then
    echo "ERRO: awslocal não encontrado. Instale com: pip install awscli-local"
    exit 1
fi

echo "OK: AWS CLI Local encontrado"

# Build do projeto
echo "Fazendo build do projeto..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "ERRO: Erro no build do projeto"
    exit 1
fi

# Configurar PATH para dotnet lambda tools
export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"

# Criar pacote Lambda
echo "Criando pacote Lambda..."
mkdir -p bin/release/net8.0
dotnet lambda package --configuration Release --framework net8.0 --output-package bin/release/net8.0/lambda-test-project.zip

if [ $? -ne 0 ]; then
    echo "ERRO: Erro ao criar pacote Lambda"
    exit 1
fi

echo "OK: Pacote Lambda criado: bin/release/net8.0/lambda-test-project.zip"

# Limpar função existente (se houver)
echo "Limpando função existente..."
awslocal lambda delete-function --function-name lambda-test-project 2>/dev/null || true

# Criar função Lambda
echo "Criando função Lambda no LocalStack..."
awslocal lambda create-function \
  --function-name lambda-test-project \
  --runtime dotnet8 \
  --role arn:aws:iam::123456789012:role/lambda-role \
  --handler LambdaTestProject::LambdaTestProject.Function::FunctionHandler \
  --zip-file fileb://bin/release/net8.0/lambda-test-project.zip \
  --timeout 30 \
  --memory-size 512

if [ $? -ne 0 ]; then
    echo "ERRO: Erro ao criar função Lambda"
    exit 1
fi

echo "OK: Função Lambda criada"

# Limpar API Gateway existente (se houver)
echo "Limpando API Gateway existente..."
EXISTING_API=$(awslocal apigateway get-rest-apis --query 'items[?name==`lambda-test-api`].id' --output text 2>/dev/null)
if [ ! -z "$EXISTING_API" ] && [ "$EXISTING_API" != "None" ]; then
    awslocal apigateway delete-rest-api --rest-api-id $EXISTING_API
fi

# Criar API Gateway
echo "Criando API Gateway..."
API_ID=$(awslocal apigateway create-rest-api --name lambda-test-api --query 'id' --output text)

if [ -z "$API_ID" ] || [ "$API_ID" == "None" ]; then
    echo "ERRO: Erro ao criar API Gateway"
    exit 1
fi

echo "OK: API Gateway criada com ID: $API_ID"

# Obter o resource root ID
ROOT_ID=$(awslocal apigateway get-resources --rest-api-id $API_ID --query 'items[0].id' --output text)

# Criar recursos e métodos
echo "Criando rotas..."

# Health endpoint
awslocal apigateway create-resource --rest-api-id $API_ID --parent-id $ROOT_ID --path-part health > /dev/null
HEALTH_ID=$(awslocal apigateway get-resources --rest-api-id $API_ID --query 'items[?pathPart==`health`].id' --output text)
awslocal apigateway put-method --rest-api-id $API_ID --resource-id $HEALTH_ID --http-method GET --authorization-type NONE > /dev/null

# Get-data endpoint  
awslocal apigateway create-resource --rest-api-id $API_ID --parent-id $ROOT_ID --path-part get-data > /dev/null
GETDATA_ID=$(awslocal apigateway get-resources --rest-api-id $API_ID --query 'items[?pathPart==`get-data`].id' --output text)
awslocal apigateway put-method --rest-api-id $API_ID --resource-id $GETDATA_ID --http-method GET --authorization-type NONE > /dev/null

# Process-order endpoint
awslocal apigateway create-resource --rest-api-id $API_ID --parent-id $ROOT_ID --path-part process-order > /dev/null
ORDER_ID=$(awslocal apigateway get-resources --rest-api-id $API_ID --query 'items[?pathPart==`process-order`].id' --output text)
awslocal apigateway put-method --rest-api-id $API_ID --resource-id $ORDER_ID --http-method POST --authorization-type NONE > /dev/null

echo "OK: Rotas criadas"

# Conectar métodos à função Lambda
echo "Conectando rotas à função Lambda..."
LAMBDA_URI="arn:aws:apigateway:us-east-1:lambda:path/2015-03-31/functions/arn:aws:lambda:us-east-1:000000000000:function:lambda-test-project/invocations"

# Configurar integrações
awslocal apigateway put-integration --rest-api-id $API_ID --resource-id $HEALTH_ID --http-method GET --type AWS_PROXY --integration-http-method POST --uri $LAMBDA_URI > /dev/null
awslocal apigateway put-integration --rest-api-id $API_ID --resource-id $GETDATA_ID --http-method GET --type AWS_PROXY --integration-http-method POST --uri $LAMBDA_URI > /dev/null
awslocal apigateway put-integration --rest-api-id $API_ID --resource-id $ORDER_ID --http-method POST --type AWS_PROXY --integration-http-method POST --uri $LAMBDA_URI > /dev/null

# Configurar integration responses
awslocal apigateway put-integration-response --rest-api-id $API_ID --resource-id $HEALTH_ID --http-method GET --status-code 200 > /dev/null
awslocal apigateway put-integration-response --rest-api-id $API_ID --resource-id $GETDATA_ID --http-method GET --status-code 200 > /dev/null
awslocal apigateway put-integration-response --rest-api-id $API_ID --resource-id $ORDER_ID --http-method POST --status-code 200 > /dev/null

# Configurar method responses
awslocal apigateway put-method-response --rest-api-id $API_ID --resource-id $HEALTH_ID --http-method GET --status-code 200 > /dev/null
awslocal apigateway put-method-response --rest-api-id $API_ID --resource-id $GETDATA_ID --http-method GET --status-code 200 > /dev/null
awslocal apigateway put-method-response --rest-api-id $API_ID --resource-id $ORDER_ID --http-method POST --status-code 200 > /dev/null

# Dar permissão para API Gateway invocar Lambda
awslocal lambda add-permission --function-name lambda-test-project --statement-id apigateway-invoke --action lambda:InvokeFunction --principal apigateway.amazonaws.com 2>/dev/null || true

echo "OK: Integrações configuradas"

# Deploy da API
echo "Fazendo deploy da API..."
awslocal apigateway create-deployment --rest-api-id $API_ID --stage-name dev > /dev/null

echo "OK: Deploy concluído!"

# URLs para teste
BASE_URL="http://localhost:4566/_aws/execute-api/$API_ID/dev"

echo ""
echo "Deploy concluído com sucesso!"
echo "=================================="
echo ""
echo "URLs para teste:"
echo "   Health Check: $BASE_URL/health"
echo "   Get Data:     $BASE_URL/get-data"  
echo "   Process Order: $BASE_URL/process-order (POST)"
echo ""
echo "Comandos de teste:"
echo "   curl $BASE_URL/health"
echo "   curl $BASE_URL/get-data"
echo "   curl -X POST $BASE_URL/process-order -H 'Content-Type: application/json' -d '{\"productId\":\"PROD-001\",\"quantity\":2,\"customerEmail\":\"teste@exemplo.com\"}'"
echo ""
echo "API Gateway ID: $API_ID"
echo "LocalStack Dashboard: http://localhost:4566/_localstack/health"
