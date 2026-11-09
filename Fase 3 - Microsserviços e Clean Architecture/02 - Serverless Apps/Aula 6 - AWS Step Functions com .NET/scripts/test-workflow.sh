#!/bin/bash

# Script para testar o Step Functions workflow

set -e

# Configurações
LOCALSTACK_ENDPOINT="http://localhost:4566"
REGION="us-east-1"
ACCOUNT_ID="000000000000"

# Configurar AWS CLI para LocalStack
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=$REGION
export AWS_ENDPOINT_URL=$LOCALSTACK_ENDPOINT

echo "======================================="
echo "Testando o Workflow de Cadastro"
echo "======================================="

# Obter ARN da State Machine
STATE_MACHINE_ARN=$(aws stepfunctions list-state-machines \
    --endpoint-url=$LOCALSTACK_ENDPOINT \
    --region=$REGION \
    --query 'stateMachines[?name==`CustomerRegistrationWorkflow`].stateMachineArn' \
    --output text)

if [ -z "$STATE_MACHINE_ARN" ]; then
    echo "ERRO: State Machine 'CustomerRegistrationWorkflow' não encontrada"
    echo "Execute primeiro: ./scripts/build-and-deploy.sh"
    exit 1
fi

echo "State Machine ARN: $STATE_MACHINE_ARN"

# Função para executar um teste
execute_test() {
    local test_name=$1
    local input_json=$2
    
    echo ""
    echo "--- Teste: $test_name ---"
    
    # Executar o workflow
    EXECUTION_ARN=$(aws stepfunctions start-execution \
        --state-machine-arn $STATE_MACHINE_ARN \
        --name "test-$(date +%s)" \
        --input "$input_json" \
        --endpoint-url=$LOCALSTACK_ENDPOINT \
        --region=$REGION \
        --query 'executionArn' \
        --output text)
    
    echo "Execução iniciada: $EXECUTION_ARN"
    
    # Aguardar conclusão (máximo 30 segundos)
    for i in {1..30}; do
        STATUS=$(aws stepfunctions describe-execution \
            --execution-arn $EXECUTION_ARN \
            --endpoint-url=$LOCALSTACK_ENDPOINT \
            --region=$REGION \
            --query 'status' \
            --output text)
        
        if [ "$STATUS" == "SUCCEEDED" ] || [ "$STATUS" == "FAILED" ] || [ "$STATUS" == "ABORTED" ]; then
            break
        fi
        
        echo "Aguardando conclusão... ($i/30)"
        sleep 1
    done
    
    # Mostrar resultado
    echo "Status final: $STATUS"
    
    OUTPUT=$(aws stepfunctions describe-execution \
        --execution-arn $EXECUTION_ARN \
        --endpoint-url=$LOCALSTACK_ENDPOINT \
        --region=$REGION \
        --query 'output' \
        --output text)
    
    echo "Resultado:"
    echo "$OUTPUT" | jq . || echo "$OUTPUT"
}

# Teste 1: Dados válidos - CPF
echo "Testando cadastro com CPF válido..."
execute_test "CPF Válido" '{
  "name": "Thiago S Adriano",
  "email": "tadriano.net@email.com",
  "document": "123.456.789-09",
  "phone": "(11) 99999-9999"
}'

# Teste 2: Dados válidos - CNPJ  
echo "Testando cadastro com CNPJ válido..."
execute_test "CNPJ Válido" '{
  "name": "Empresa ABC Ltda",
  "email": "contato@empresa.com",
  "document": "11.222.333/0001-81",
  "phone": "(11) 88888-8888"
}'

# Teste 3: Dados inválidos - CPF inválido
echo "Testando com CPF inválido..."
execute_test "CPF Inválido" '{
  "name": "Maria Santos",
  "email": "maria@email.com",
  "document": "123.456.789-00",
  "phone": "(11) 77777-7777"
}'

# Teste 4: Dados inválidos - Email inválido
echo "Testando com email inválido..."
execute_test "Email Inválido" '{
  "name": "Pedro Oliveira",
  "email": "email-invalido",
  "document": "987.654.321-00",
  "phone": "(11) 66666-6666"
}'

# Teste 5: Cadastro duplicado (executar o primeiro teste novamente)
echo "Testando cadastro duplicado..."
execute_test "Cadastro Duplicado" '{
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "document": "123.456.789-09",
  "phone": "(11) 99999-9999"
}'

echo ""
echo "======================================="
echo "Testes concluídos!"
echo "======================================="
echo ""
echo "Para visualizar os dados no DynamoDB:"
echo "aws dynamodb scan --table-name customers --endpoint-url=$LOCALSTACK_ENDPOINT --region=$REGION"
echo ""
echo "Para visualizar mensagens SNS (logs do LocalStack):"
echo "docker logs \$(docker ps -q --filter ancestor=localstack/localstack) | grep SNS"
