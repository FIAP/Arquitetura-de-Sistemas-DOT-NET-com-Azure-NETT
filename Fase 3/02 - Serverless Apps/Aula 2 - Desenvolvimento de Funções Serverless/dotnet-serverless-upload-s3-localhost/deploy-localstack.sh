#!/bin/bash

echo "=== Deploy Lambda para LocalStack ==="

# Configurações
STACK_NAME="s3-processor-stack"
BUCKET_NAME="meu-bucket-upload"
REGION="us-east-1"
ENDPOINT_URL="http://localhost:4566"

# Verificar se LocalStack está rodando
echo "Verificando se LocalStack está rodando..."
if ! curl -s $ENDPOINT_URL/_localstack/health > /dev/null; then
    echo "❌ LocalStack não está rodando. Execute: docker-compose -f localstack-config.yaml up -d"
    exit 1
fi
echo "✅ LocalStack está rodando"

# Configurar AWS CLI para LocalStack
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=$REGION

echo "Construindo o projeto .NET..."
dotnet restore
dotnet build -c Release

echo "Empacotando a aplicação..."
sam build --use-container

echo "Fazendo deploy no LocalStack..."
sam deploy \
    --stack-name $STACK_NAME \
    --parameter-overrides BucketName=$BUCKET_NAME Environment=local \
    --capabilities CAPABILITY_IAM \
    --resolve-s3 \
    --region $REGION \
    --s3-endpoint-url $ENDPOINT_URL \
    --endpoint-url $ENDPOINT_URL

if [ $? -eq 0 ]; then
    echo " Deploy realizado com sucesso!"
    
    echo ""
    echo "=== Testando o setup ==="
    echo "1. Criar arquivo de teste:"
    echo "   echo 'Hello LocalStack!' > test-file.txt"
    echo ""
    echo "2. Upload para S3:"
    echo "   aws s3 cp test-file.txt s3://$BUCKET_NAME/uploads/ --endpoint-url $ENDPOINT_URL"
    echo ""
    echo "3. Verificar logs da Lambda:"
    echo "   aws logs describe-log-groups --endpoint-url $ENDPOINT_URL"
    echo ""
    echo "4. Listar objetos processados:"
    echo "   aws s3 ls s3://$BUCKET_NAME/processed/ --endpoint-url $ENDPOINT_URL"
else
    echo " Falha no deploy"
    exit 1
fi
