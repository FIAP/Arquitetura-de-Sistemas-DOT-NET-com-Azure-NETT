#!/bin/bash

# Script para compilar e fazer deploy das Lambda Functions no LocalStack

set -e

echo "======================================="
echo "Iniciando build e deploy das funções"
echo "======================================="

# Configurações
LOCALSTACK_ENDPOINT="http://localhost:4566"
REGION="us-east-1"
ACCOUNT_ID="000000000000"

# Verificar se LocalStack está rodando
echo "Verificando se LocalStack está rodando..."
if ! curl -s $LOCALSTACK_ENDPOINT/health > /dev/null; then
    echo "ERRO: LocalStack não está rodando em $LOCALSTACK_ENDPOINT"
    echo "Inicie o LocalStack primeiro: docker run --rm -it -p 4566:4566 -p 4571:4571 localstack/localstack"
    exit 1
fi

echo "LocalStack detectado e funcionando!"

# Criar diretório para deploys primeiro
mkdir -p deploy

# Função para fazer o build de um projeto Lambda
build_lambda() {
    local project_name=$1
    local project_path="src/$project_name"
    local current_dir=$(pwd)
    
    echo "Fazendo build do projeto: $project_name"
    
    cd $project_path
    
    # Limpar builds anteriores
    rm -rf bin obj
    
    # Build do projeto
    dotnet publish -c Release -r linux-x64 --self-contained false -o bin/Release/net8.0/linux-x64/publish
    
    # Criar ZIP para deploy
    cd bin/Release/net8.0/linux-x64/publish
    zip -r $current_dir/deploy/$project_name.zip .
    
    cd $current_dir
    
    echo "Build concluído para $project_name"
}

# Build de todas as funções Lambda
build_lambda "ValidateDataFunction"
build_lambda "CheckCustomerFunction" 
build_lambda "CreateCustomerFunction"
build_lambda "SendWelcomeEmailFunction"

echo "======================================="
echo "Criando recursos AWS no LocalStack"
echo "======================================="

# Configurar AWS CLI para LocalStack
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=$REGION
export AWS_ENDPOINT_URL=$LOCALSTACK_ENDPOINT

echo "1. Criando tabela DynamoDB..."
aws dynamodb create-table \
    --table-name customers \
    --attribute-definitions \
        AttributeName=document,AttributeType=S \
    --key-schema \
        AttributeName=document,KeyType=HASH \
    --provisioned-throughput \
        ReadCapacityUnits=5,WriteCapacityUnits=5 \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION || echo "Tabela 'customers' pode já existir"

echo "2. Criando tópico SNS..."
aws sns create-topic \
    --name welcome-email \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION || echo "Tópico 'welcome-email' pode já existir"

echo "3. Criando funções Lambda..."

# Função de validação
aws lambda create-function \
    --function-name ValidateDataFunction \
    --runtime dotnet8 \
    --role arn:aws:iam::$ACCOUNT_ID:role/lambda-execution-role \
    --handler ValidateDataFunction::ValidateDataFunction.Function::FunctionHandler \
    --zip-file fileb://deploy/ValidateDataFunction.zip \
    --timeout 30 \
    --environment Variables="{LOCALSTACK_ENDPOINT=$LOCALSTACK_ENDPOINT}" \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION || echo "Função ValidateDataFunction pode já existir"

# Função de verificação
aws lambda create-function \
    --function-name CheckCustomerFunction \
    --runtime dotnet8 \
    --role arn:aws:iam::$ACCOUNT_ID:role/lambda-execution-role \
    --handler CheckCustomerFunction::CheckCustomerFunction.Function::FunctionHandler \
    --zip-file fileb://deploy/CheckCustomerFunction.zip \
    --timeout 30 \
    --environment Variables="{LOCALSTACK_ENDPOINT=$LOCALSTACK_ENDPOINT,CUSTOMERS_TABLE=customers}" \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION || echo "Função CheckCustomerFunction pode já existir"

# Função de criação
aws lambda create-function \
    --function-name CreateCustomerFunction \
    --runtime dotnet8 \
    --role arn:aws:iam::$ACCOUNT_ID:role/lambda-execution-role \
    --handler CreateCustomerFunction::CreateCustomerFunction.Function::FunctionHandler \
    --zip-file fileb://deploy/CreateCustomerFunction.zip \
    --timeout 30 \
    --environment Variables="{LOCALSTACK_ENDPOINT=$LOCALSTACK_ENDPOINT,CUSTOMERS_TABLE=customers}" \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION || echo "Função CreateCustomerFunction pode já existir"

# Função de email
aws lambda create-function \
    --function-name SendWelcomeEmailFunction \
    --runtime dotnet8 \
    --role arn:aws:iam::$ACCOUNT_ID:role/lambda-execution-role \
    --handler SendWelcomeEmailFunction::SendWelcomeEmailFunction.Function::FunctionHandler \
    --zip-file fileb://deploy/SendWelcomeEmailFunction.zip \
    --timeout 30 \
    --environment Variables="{LOCALSTACK_ENDPOINT=$LOCALSTACK_ENDPOINT,WELCOME_EMAIL_TOPIC_ARN=arn:aws:sns:$REGION:$ACCOUNT_ID:welcome-email}" \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION || echo "Função SendWelcomeEmailFunction pode já existir"

echo "4. Criando State Machine (Step Functions)..."
aws stepfunctions create-state-machine \
    --name CustomerRegistrationWorkflow \
    --definition file://step-functions/customer-registration-workflow.json \
    --role-arn arn:aws:iam::$ACCOUNT_ID:role/stepfunctions-execution-role \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION || echo "State Machine pode já existir"

echo "======================================="
echo "Deploy concluído com sucesso!"
echo "======================================="

echo "Recursos criados:"
echo "- Tabela DynamoDB: customers"
echo "- Tópico SNS: welcome-email"
echo "- 4 Funções Lambda"
echo "- State Machine: CustomerRegistrationWorkflow"
echo ""
echo "Para testar, execute:"
echo "./scripts/test-workflow.sh"
