# Comandos Úteis - AWS Step Functions com LocalStack

## Configuração do Ambiente

```bash
# Configurar AWS CLI para LocalStack
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=us-east-1
export AWS_ENDPOINT_URL=http://localhost:4566

# Verificar se LocalStack está rodando
curl http://localhost:4566/health

# Ver logs do LocalStack
docker logs $(docker ps -q --filter ancestor=localstack/localstack)
```

## Step Functions

```bash
# Listar State Machines
aws stepfunctions list-state-machines --endpoint-url=http://localhost:4566

# Obter ARN da State Machine
STATE_MACHINE_ARN=$(aws stepfunctions list-state-machines \
    --query 'stateMachines[?name==`CustomerRegistrationWorkflow`].stateMachineArn' \
    --output text --endpoint-url=http://localhost:4566)

# Executar workflow
aws stepfunctions start-execution \
    --state-machine-arn $STATE_MACHINE_ARN \
    --name "teste-$(date +%s)" \
    --input file://examples/test-cases.json \
    --endpoint-url=http://localhost:4566

# Listar execuções
aws stepfunctions list-executions \
    --state-machine-arn $STATE_MACHINE_ARN \
    --endpoint-url=http://localhost:4566

# Descrever execução específica
aws stepfunctions describe-execution \
    --execution-arn "ARN_DA_EXECUÇÃO" \
    --endpoint-url=http://localhost:4566

# Ver histórico de execução
aws stepfunctions get-execution-history \
    --execution-arn "ARN_DA_EXECUÇÃO" \
    --endpoint-url=http://localhost:4566
```

## Lambda Functions

```bash
# Listar funções
aws lambda list-functions --endpoint-url=http://localhost:4566

# Testar função individualmente
aws lambda invoke \
    --function-name ValidateDataFunction \
    --payload '{"name":"Teste","email":"teste@email.com","document":"123.456.789-09","phone":"(11)99999-9999"}' \
    --endpoint-url=http://localhost:4566 \
    response.json

# Ver logs de uma função
aws logs describe-log-groups \
    --log-group-name-prefix "/aws/lambda/" \
    --endpoint-url=http://localhost:4566

# Atualizar código de uma função
aws lambda update-function-code \
    --function-name ValidateDataFunction \
    --zip-file fileb://deploy/ValidateDataFunction.zip \
    --endpoint-url=http://localhost:4566
```

## DynamoDB

```bash
# Listar tabelas
aws dynamodb list-tables --endpoint-url=http://localhost:4566

# Descrever tabela
aws dynamodb describe-table \
    --table-name customers \
    --endpoint-url=http://localhost:4566

# Fazer scan da tabela (todos os registros)
aws dynamodb scan \
    --table-name customers \
    --endpoint-url=http://localhost:4566

# Consultar item específico
aws dynamodb get-item \
    --table-name customers \
    --key '{"document":{"S":"12345678909"}}' \
    --endpoint-url=http://localhost:4566

# Contar itens na tabela
aws dynamodb scan \
    --table-name customers \
    --select COUNT \
    --endpoint-url=http://localhost:4566

# Deletar item
aws dynamodb delete-item \
    --table-name customers \
    --key '{"document":{"S":"12345678909"}}' \
    --endpoint-url=http://localhost:4566

# Limpar todos os dados da tabela
aws dynamodb scan \
    --table-name customers \
    --projection-expression "document" \
    --endpoint-url=http://localhost:4566 \
    --output json | jq -r '.Items[].document.S' | while read doc; do
        aws dynamodb delete-item \
            --table-name customers \
            --key "{\"document\":{\"S\":\"$doc\"}}" \
            --endpoint-url=http://localhost:4566
    done
```

## SNS

```bash
# Listar tópicos
aws sns list-topics --endpoint-url=http://localhost:4566

# Obter atributos do tópico
aws sns get-topic-attributes \
    --topic-arn "arn:aws:sns:us-east-1:000000000000:welcome-email" \
    --endpoint-url=http://localhost:4566

# Listar subscrições
aws sns list-subscriptions --endpoint-url=http://localhost:4566

# Publicar mensagem diretamente no tópico
aws sns publish \
    --topic-arn "arn:aws:sns:us-east-1:000000000000:welcome-email" \
    --message "Teste de mensagem" \
    --subject "Teste" \
    --endpoint-url=http://localhost:4566
```

## Debug e Troubleshooting

```bash
# Verificar status de todos os serviços
curl http://localhost:4566/health | jq

# Ver logs em tempo real
docker logs -f $(docker ps -q --filter ancestor=localstack/localstack)

# Filtrar logs por serviço
docker logs $(docker ps -q --filter ancestor=localstack/localstack) 2>&1 | grep "lambda"
docker logs $(docker ps -q --filter ancestor=localstack/localstack) 2>&1 | grep "stepfunctions"
docker logs $(docker ps -q --filter ancestor=localstack/localstack) 2>&1 | grep "dynamodb"

# Reiniciar LocalStack
docker restart $(docker ps -q --filter ancestor=localstack/localstack)

# Verificar recursos criados
echo "=== Lambda Functions ==="
aws lambda list-functions --endpoint-url=http://localhost:4566 --query 'Functions[].FunctionName'

echo "=== DynamoDB Tables ==="
aws dynamodb list-tables --endpoint-url=http://localhost:4566

echo "=== SNS Topics ==="
aws sns list-topics --endpoint-url=http://localhost:4566

echo "=== Step Functions ==="
aws stepfunctions list-state-machines --endpoint-url=http://localhost:4566 --query 'stateMachines[].name'
```

## Build e Deploy

```bash
# Build individual de uma função
cd src/ValidateDataFunction
dotnet publish -c Release -r linux-x64 --self-contained false
cd bin/Release/net8.0/linux-x64/publish
zip -r ValidateDataFunction.zip .

# Build de todo o projeto
dotnet build

# Limpeza completa
find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true
rm -rf deploy/

# Verificar dependências
dotnet list package --outdated
```

## Monitoramento

```bash
# Contar execuções por status
aws stepfunctions list-executions \
    --state-machine-arn $STATE_MACHINE_ARN \
    --endpoint-url=http://localhost:4566 \
    | jq '.executions | group_by(.status) | map({status: .[0].status, count: length})'

# Ver últimas 10 execuções
aws stepfunctions list-executions \
    --state-machine-arn $STATE_MACHINE_ARN \
    --max-items 10 \
    --endpoint-url=http://localhost:4566 \
    | jq '.executions[] | {name: .name, status: .status, startDate: .startDate}'

# Estatísticas da tabela DynamoDB
aws dynamodb describe-table \
    --table-name customers \
    --endpoint-url=http://localhost:4566 \
    | jq '.Table | {TableName: .TableName, ItemCount: .ItemCount, TableSizeBytes: .TableSizeBytes}'
```

## Teste de Carga

```bash
# Executar múltiplas instâncias em paralelo
for i in {1..5}; do
    aws stepfunctions start-execution \
        --state-machine-arn $STATE_MACHINE_ARN \
        --name "carga-teste-$i" \
        --input "{\"name\":\"Usuario$i\",\"email\":\"user$i@teste.com\",\"document\":\"12345678$(printf "%02d" $i)\",\"phone\":\"(11)9999-$i$i$i$i\"}" \
        --endpoint-url=http://localhost:4566 &
done
wait

echo "Testes de carga concluídos"
```

## Backup e Restore

```bash
# Backup da tabela DynamoDB
aws dynamodb scan \
    --table-name customers \
    --endpoint-url=http://localhost:4566 > backup_customers.json

# Restore (requer processamento dos dados)
# Este é um exemplo básico - em produção usaria AWS Data Pipeline
jq -r '.Items[] | @base64' backup_customers.json | while read item; do
    echo $item | base64 -d | aws dynamodb put-item \
        --table-name customers \
        --item file:///dev/stdin \
        --endpoint-url=http://localhost:4566
done
```
