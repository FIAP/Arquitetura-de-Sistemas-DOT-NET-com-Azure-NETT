# Lambda .NET 8 no LocalStack com Serverless Framework

Exemplo prático de uma AWS Lambda em .NET 8 (C#) executando no LocalStack e gerenciada pelo Serverless Framework.

## Visão geral
- **projeto**: `HelloLambda` (handler `HelloLambda::HelloLambda.Function::FunctionHandler`)
- **empacotamento**: `make package` (gera `artifacts/HelloLambda.zip`)
- **deploy local**: `npx sls deploy -s local` (usa o plugin `serverless-localstack`)
- **invocação**: via Serverless (`sls invoke`) ou AWS CLI apontando para `http://localhost:4566`
- **endpoint HTTP**: API Gateway emulado no LocalStack, rota `POST /hello`

## Pré‑requisitos
- Docker e/ou LocalStack CLI
- .NET SDK 8 instalado (`dotnet --version`)
- Node.js e npm (`node -v`, `npm -v`)
- AWS CLI (`aws --version`)

## Entrar no diretório do projeto
```bash
cd /Users/thiagoadriano/fiap/serverless/aula2/lambda
```

## Configurar variáveis de ambiente (credenciais fake para LocalStack)
```bash
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=us-east-1
```

## Subir o LocalStack
- Com LocalStack CLI:
```bash
localstack start -d
```
- Health básico do LocalStack:
```bash
curl -s http://localhost:4566/health | jq .
```

## Instalar dependências do Serverless Framework
```bash
npm init -y
npm i -D serverless serverless-localstack
```

## Empacotar a Lambda (.NET)
```bash
make package
```
Saída esperada: `artifacts/HelloLambda.zip` (gerado via `dotnet publish`).

## Deploy para o LocalStack (Serverless)
```bash
npx sls deploy -s local
```
Irá criar a função Lambda e o endpoint HTTP (API Gateway emulado).

## Descobrir o endpoint HTTP
```bash
npx sls info -s local
```
Procure por `endpoints:` e copie a URL. Teste com:
```bash
curl -s -X POST "<URL_DO_ENDPOINT>/hello" \
  -H "Content-Type: application/json" \
  -d '{"Name":"Thiago"}' | jq .
```

## Invocar a função (alternativas)
- Via Serverless (inline):
```bash
npx sls invoke -f hello -s local --data '{"Name":"Thiago"}'
```
- Via Serverless (arquivo):
```bash
echo -n '{"Name":"Thiago"}' > /tmp/event.json
npx sls invoke -f hello -s local --path /tmp/event.json
```
- Via AWS CLI (direto na Lambda):
```bash
echo -n '{"Name":"Thiago"}' > /tmp/event.json
aws --endpoint-url http://localhost:4566 lambda invoke \
  --function-name hello-dotnet \
  --payload fileb:///tmp/event.json \
  /tmp/hello-out.json && cat /tmp/hello-out.json
```
Observação: com AWS CLI use `fileb://` no `--payload` para evitar erro de Base64.


## Healthchecks úteis

- Listar Lambdas:
```bash
aws --endpoint-url http://localhost:4566 lambda list-functions | jq .
```



## Estrutura principal do projeto
```text
/Users/thiagoadriano/fiap/serverless/aula2/lambda
├─ HelloLambda/
│  ├─ HelloLambda.csproj
│  └─ Function.cs
├─ artifacts/                 # (gerado pelo make package)
│  └─ HelloLambda.zip
├─ serverless.yml
├─ Makefile
└─ README.md
```

## Exemplo de request/response
- Request:
```json
{ "Name": "Thiago" }
```
- Response (exemplo):
```json
{
  "Message": "Olá, Thiago! Sua função .NET 8 na AWS Lambda está rodando no LocalStack.",
  "TimestampUtc": "2025-08-20T00:16:20.7937422Z"
}
```

## Dicas & Troubleshooting
- "Invalid base64" ao invocar com AWS CLI: use `--payload fileb:///caminho/para/arquivo.json`.
- Plugin não encontrado: `npm i -D serverless-localstack`.
- Porta ocupada: verifique se a `4566` está livre; ajuste `edgePort` no `serverless.yml` se necessário.
- Credenciais: exporte `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` e `AWS_DEFAULT_REGION`.
- Arquitetura/Runtime: este projeto usa `dotnet8`. Se notar erro de arquitetura, podemos ajustar `architectures` no `serverless.yml`.
