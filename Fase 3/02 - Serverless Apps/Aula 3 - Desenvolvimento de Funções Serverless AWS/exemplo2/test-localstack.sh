#!/bin/bash

# Script para testar endpoints no LocalStack

set -e

echo "🧪 Testando Lambda no LocalStack"
echo "================================"

# Verificar se LocalStack está rodando
if ! curl -s http://localhost:4566/_localstack/health > /dev/null; then
    echo "❌ LocalStack não está rodando!"
    exit 1
fi

# Obter API ID
API_ID=$(awslocal apigateway get-rest-apis --query 'items[?name==`lambda-test-api`].id' --output text 2>/dev/null)

if [ -z "$API_ID" ] || [ "$API_ID" == "None" ]; then
    echo "❌ API lambda-test-api não encontrada. Execute primeiro: ./deploy-localstack.sh"
    exit 1
fi

echo "✅ API encontrada com ID: $API_ID"
echo ""

BASE_URL="http://localhost:4566/_aws/execute-api/$API_ID/dev"

echo "🔍 Testando endpoints..."
echo "========================"
echo ""

# Teste 1: Health Check
echo "📋 1. Health Check"
echo "   URL: $BASE_URL/health"
echo ""
response=$(curl -s -w "\n%{http_code}" $BASE_URL/health)
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

echo "   Resposta:"
echo "$body" | jq . 2>/dev/null || echo "$body"
echo "   Status Code: $http_code"
echo ""

# Teste 2: Get Data
echo "📊 2. Get Data"
echo "   URL: $BASE_URL/get-data"
echo ""
response=$(curl -s -w "\n%{http_code}" $BASE_URL/get-data)
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

echo "   Resposta:"
echo "$body" | jq . 2>/dev/null || echo "$body"
echo "   Status Code: $http_code"
echo ""

# Teste 3: Process Order (válido)
echo "📦 3. Process Order (dados válidos)"
echo "   URL: $BASE_URL/process-order"
echo ""
response=$(curl -s -w "\n%{http_code}" -X POST $BASE_URL/process-order \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "PROD-001",
    "quantity": 2,
    "customerEmail": "teste@exemplo.com"
  }')
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

echo "   Resposta:"
echo "$body" | jq . 2>/dev/null || echo "$body"
echo "   Status Code: $http_code"
echo ""

# Teste 4: Process Order (inválido)
echo "❌ 4. Process Order (dados inválidos)"
echo "   URL: $BASE_URL/process-order"
echo ""
response=$(curl -s -w "\n%{http_code}" -X POST $BASE_URL/process-order \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "",
    "quantity": 0
  }')
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

echo "   Resposta:"
echo "$body" | jq . 2>/dev/null || echo "$body"
echo "   Status Code: $http_code"
echo ""

# Teste 5: Endpoint inexistente
echo "🚫 5. Endpoint inexistente"
echo "   URL: $BASE_URL/endpoint-inexistente"
echo ""
response=$(curl -s -w "\n%{http_code}" $BASE_URL/endpoint-inexistente)
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

echo "   Resposta:"
echo "$body" | jq . 2>/dev/null || echo "$body"
echo "   Status Code: $http_code"
echo ""

echo "✅ Testes concluídos!"
echo ""
echo "💡 Para usar no arquivo test-requests.http, use:"
echo "   @baseUrl = $BASE_URL"
