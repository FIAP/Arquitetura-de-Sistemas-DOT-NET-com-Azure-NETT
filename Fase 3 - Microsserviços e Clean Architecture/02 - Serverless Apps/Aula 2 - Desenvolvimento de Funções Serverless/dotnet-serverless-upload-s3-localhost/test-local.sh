#!/bin/bash

echo "=== Teste Local da Função Lambda S3 Processor ==="

# Configurações
BUCKET_NAME="meu-bucket-upload"
ENDPOINT_URL="http://localhost:4566"

# Configurar AWS CLI para LocalStack
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=us-east-1

echo "1. Criando arquivos de teste..."

# Criar arquivo de texto
cat > test-text.txt << EOF
Este é um arquivo de teste
com múltiplas linhas
para verificar o processamento
da função Lambda.
EOF

# Criar arquivo JSON
cat > test-data.json << EOF
{
  "usuario": "Teste",
  "dados": {
    "nome": "João Silva",
    "email": "joao@example.com",
    "idade": 30
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
EOF

echo "2. Fazendo upload dos arquivos para o S3..."

# Upload do arquivo de texto
aws s3 cp test-text.txt s3://$BUCKET_NAME/uploads/ --endpoint-url $ENDPOINT_URL
if [ $? -eq 0 ]; then
    echo "✅ Arquivo de texto enviado com sucesso"
else
    echo "❌ Falha ao enviar arquivo de texto"
fi

sleep 2

# Upload do arquivo JSON
aws s3 cp test-data.json s3://$BUCKET_NAME/uploads/ --endpoint-url $ENDPOINT_URL
if [ $? -eq 0 ]; then
    echo "✅ Arquivo JSON enviado com sucesso"
else
    echo "❌ Falha ao enviar arquivo JSON"
fi

sleep 3

echo "3. Verificando arquivos processados..."
aws s3 ls s3://$BUCKET_NAME/processed/ --recursive --endpoint-url $ENDPOINT_URL

echo ""
echo "4. Baixando arquivos processados..."
mkdir -p processed-files
aws s3 sync s3://$BUCKET_NAME/processed/ processed-files/ --endpoint-url $ENDPOINT_URL

if [ -d "processed-files" ] && [ "$(ls -A processed-files)" ]; then
    echo "✅ Arquivos processados baixados em ./processed-files/"
    echo "Conteúdo dos arquivos processados:"
    for file in processed-files/*; do
        if [ -f "$file" ]; then
            echo "--- $file ---"
            cat "$file"
            echo ""
        fi
    done
else
    echo "⚠️  Nenhum arquivo processado encontrado"
fi

echo "5. Verificando logs da Lambda..."
FUNCTION_NAME="s3-processor-function"
echo "Para ver os logs da função Lambda:"
echo "aws logs describe-log-groups --endpoint-url $ENDPOINT_URL"
echo "aws logs describe-log-streams --log-group-name /aws/lambda/$FUNCTION_NAME --endpoint-url $ENDPOINT_URL"

echo ""
echo "6. Verificar função Lambda:"
aws lambda list-functions --endpoint-url $ENDPOINT_URL --query 'Functions[?FunctionName==`'$FUNCTION_NAME'`].[FunctionName,Runtime,LastModified]' --output table

# Limpeza
echo ""
echo "Para limpar os arquivos de teste:"
echo "rm -f test-text.txt test-data.json"
echo "rm -rf processed-files/"
