#!/bin/bash

# Script para limpar recursos do LocalStack

set -e

# Configurações
LOCALSTACK_ENDPOINT="http://localhost:4566"
REGION="us-east-1"

# Configurar AWS CLI para LocalStack
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=$REGION
export AWS_ENDPOINT_URL=$LOCALSTACK_ENDPOINT

echo "======================================="
echo "Limpando recursos do LocalStack"
echo "======================================="

# Verificar se LocalStack está rodando
if ! curl -s $LOCALSTACK_ENDPOINT/health > /dev/null; then
    echo "LocalStack não está rodando em $LOCALSTACK_ENDPOINT"
    exit 1
fi

# Deletar State Machine
echo "Deletando State Machine..."
STATE_MACHINE_ARN=$(aws stepfunctions list-state-machines \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION \
    --query 'stateMachines[?name==`CustomerRegistrationWorkflow`].stateMachineArn' \
    --output text)

if [ ! -z "$STATE_MACHINE_ARN" ]; then
    aws stepfunctions delete-state-machine \
        --state-machine-arn $STATE_MACHINE_ARN \
        --endpoint-url=$LOCALSTACK_ENDPOINT \
        --region=$REGION
    echo "State Machine deletada"
else
    echo "State Machine não encontrada"
fi

# Deletar funções Lambda
echo "Deletando funções Lambda..."
FUNCTIONS=("ValidateDataFunction" "CheckCustomerFunction" "CreateCustomerFunction" "SendWelcomeEmailFunction")

for func in "${FUNCTIONS[@]}"; do
    aws lambda delete-function \
        --function-name $func \
        --endpoint-url=$LOCALSTACK_ENDPOINT \
        --region=$REGION 2>/dev/null && echo "Função $func deletada" || echo "Função $func não encontrada"
done

# Deletar tabela DynamoDB
echo "Deletando tabela DynamoDB..."
aws dynamodb delete-table \
    --table-name customers \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION 2>/dev/null && echo "Tabela customers deletada" || echo "Tabela customers não encontrada"

# Deletar tópico SNS
echo "Deletando tópico SNS..."
TOPIC_ARN=$(aws sns list-topics \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION \
    --query 'Topics[?contains(TopicArn, `welcome-email`)].TopicArn' \
    --output text)

if [ ! -z "$TOPIC_ARN" ]; then
    aws sns delete-topic \
        --topic-arn $TOPIC_ARN \
        --endpoint-url=$LOCALSTACK_ENDPOINT \
        --region=$REGION
    echo "Tópico SNS deletado"
else
    echo "Tópico SNS não encontrado"
fi

# Limpar arquivos de deploy
echo "Limpando arquivos de build..."
rm -rf deploy/
rm -rf src/*/bin/
rm -rf src/*/obj/

echo "======================================="
echo "Limpeza concluída!"
echo "======================================="
