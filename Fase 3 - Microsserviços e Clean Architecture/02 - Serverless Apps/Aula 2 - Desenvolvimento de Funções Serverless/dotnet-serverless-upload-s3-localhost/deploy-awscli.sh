#!/bin/bash

echo "=== Deploy Lambda para LocalStack usando AWS CLI ==="

# Configurações
FUNCTION_NAME="s3-processor-function"
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
dotnet publish -c Release -o ./publish --self-contained false

echo "Criando pacote ZIP para Lambda..."
cd publish
zip -r ../lambda-package.zip .
cd ..

echo "Criando bucket S3 se não existir..."
aws s3 mb s3://$BUCKET_NAME --endpoint-url $ENDPOINT_URL 2>/dev/null || echo "Bucket já existe"

echo "Criando/atualizando função Lambda..."

# Verificar se a função já existe
if aws lambda get-function --function-name $FUNCTION_NAME --endpoint-url $ENDPOINT_URL >/dev/null 2>&1; then
    echo "Função já existe, atualizando..."
    aws lambda update-function-code \
        --function-name $FUNCTION_NAME \
        --zip-file fileb://lambda-package.zip \
        --endpoint-url $ENDPOINT_URL
        
    aws lambda update-function-configuration \
        --function-name $FUNCTION_NAME \
        --timeout 300 \
        --memory-size 512 \
        --environment Variables="{LOCALSTACK_ENDPOINT=$ENDPOINT_URL}" \
        --endpoint-url $ENDPOINT_URL
else
    echo "Criando nova função Lambda..."
    aws lambda create-function \
        --function-name $FUNCTION_NAME \
        --runtime dotnet8 \
        --role arn:aws:iam::000000000000:role/lambda-execution-role \
        --handler "S3ProcessorLambda::S3ProcessorLambda.Function::FunctionHandler" \
        --zip-file fileb://lambda-package.zip \
        --timeout 300 \
        --memory-size 512 \
        --environment Variables="{LOCALSTACK_ENDPOINT=$ENDPOINT_URL}" \
        --endpoint-url $ENDPOINT_URL
fi

echo "Criando role IAM para Lambda (se não existir)..."
aws iam create-role \
    --role-name lambda-execution-role \
    --assume-role-policy-document '{
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Principal": {
                    "Service": "lambda.amazonaws.com"
                },
                "Action": "sts:AssumeRole"
            }
        ]
    }' \
    --endpoint-url $ENDPOINT_URL 2>/dev/null || echo "Role já existe"

# Anexar políticas necessárias
aws iam attach-role-policy \
    --role-name lambda-execution-role \
    --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole \
    --endpoint-url $ENDPOINT_URL 2>/dev/null

aws iam attach-role-policy \
    --role-name lambda-execution-role \
    --policy-arn arn:aws:iam::aws:policy/AmazonS3FullAccess \
    --endpoint-url $ENDPOINT_URL 2>/dev/null

echo "Configurando trigger do S3..."
# Configurar notificação do bucket
aws s3api put-bucket-notification-configuration \
    --bucket $BUCKET_NAME \
    --notification-configuration '{
        "LambdaFunctionConfigurations": [
            {
                "Id": "s3-lambda-trigger",
                "LambdaFunctionArn": "arn:aws:lambda:'$REGION':000000000000:function:'$FUNCTION_NAME'",
                "Events": ["s3:ObjectCreated:*"],
                "Filter": {
                    "Key": {
                        "FilterRules": [
                            {
                                "Name": "prefix",
                                "Value": "uploads/"
                            }
                        ]
                    }
                }
            }
        ]
    }' \
    --endpoint-url $ENDPOINT_URL

echo "Adicionando permissão para S3 invocar Lambda..."
aws lambda add-permission \
    --function-name $FUNCTION_NAME \
    --principal s3.amazonaws.com \
    --action lambda:InvokeFunction \
    --statement-id s3-trigger-permission \
    --source-arn arn:aws:s3:::$BUCKET_NAME \
    --endpoint-url $ENDPOINT_URL 2>/dev/null || echo "Permissão já existe"

# Limpeza
rm -f lambda-package.zip
rm -rf publish/

if [ $? -eq 0 ]; then
    echo "✅ Deploy realizado com sucesso!"
    
    echo ""
    echo "=== Testando o setup ==="
    echo "1. Criar arquivo de teste:"
    echo "   echo 'Hello LocalStack!' > test-file.txt"
    echo ""
    echo "2. Upload para S3:"
    echo "   aws s3 cp test-file.txt s3://$BUCKET_NAME/uploads/ --endpoint-url $ENDPOINT_URL"
    echo ""
    echo "3. Verificar função Lambda:"
    echo "   aws lambda list-functions --endpoint-url $ENDPOINT_URL"
    echo ""
    echo "4. Listar objetos processados:"
    echo "   aws s3 ls s3://$BUCKET_NAME/processed/ --endpoint-url $ENDPOINT_URL"
else
    echo "❌ Falha no deploy"
    exit 1
fi
