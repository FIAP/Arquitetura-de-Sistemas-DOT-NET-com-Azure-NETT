# S3 Processor Lambda - .NET 8

Este projeto implementa uma função AWS Lambda em .NET 8 que processa arquivos automaticamente quando são enviados para um bucket S3, usando LocalStack para desenvolvimento local.

## 🚀 Funcionalidades

- **Trigger automático**: A Lambda é ativada automaticamente quando arquivos são enviados para a pasta `uploads/` do bucket S3
- **Processamento por tipo**: Diferentes tipos de arquivo recebem processamento especializado:
  - **Arquivos de texto (.txt, .csv)**: Conversão para maiúsculas e adição de metadados
  - **Arquivos JSON**: Adição de timestamp e metadados de processamento
  - **Imagens**: Criação de arquivo de metadados com informações da imagem
  - **Outros**: Extração de metadados básicos
- **Salvamento automático**: Arquivos processados são salvos na pasta `processed/` do mesmo bucket
- **Compatível com LocalStack**: Totalmente configurado para desenvolvimento local

## 📁 Estrutura do Projeto

```
dotnet-lambda-s3/
├── Function.cs                 # Função Lambda principal
├── S3ProcessorLambda.csproj   # Arquivo de projeto .NET
├── template.yaml              # Template SAM para deployment
├── samconfig.toml            # Configurações do SAM
├── Dockerfile                # Container para Lambda
├── localstack-config.yaml   # Configuração do LocalStack
├── deploy-localstack.sh      # Script de deploy para LocalStack
├── test-local.sh            # Script para testar a funcionalidade
└── README.md               # Este arquivo
```

## 🛠️ Pré-requisitos

- .NET 8 SDK
- Docker e Docker Compose
- AWS CLI
- SAM CLI
- LocalStack (já configurado conforme mencionado)

## 🏗️ Setup e Execução

### 1. Iniciando o LocalStack

Se você já não tiver o LocalStack rodando:

```bash
# Usar a configuração fornecida
docker-compose -f localstack-config.yaml up -d

# Ou se você já tem sua própria configuração do LocalStack
# certifique-se que os serviços lambda, s3, iam e cloudformation estão habilitados
```

### 2. Deploy da Aplicação

Você tem duas opções para fazer o deploy:

#### Opção A: Usando AWS SAM (requer instalação do SAM CLI)

```bash
# Se você tiver o SAM CLI instalado
./deploy-localstack.sh
```

#### Opção B: Usando apenas AWS CLI (recomendado)

```bash
# Dar permissão ao script
chmod +x deploy-awscli.sh

# Executar o deploy
./deploy-awscli.sh
```

O script irá:
- Verificar se o LocalStack está rodando
- Construir o projeto .NET
- Criar o pacote ZIP da Lambda
- Fazer o deploy usando AWS CLI
- Configurar o bucket S3 e os triggers
- Configurar permissões IAM necessárias

### 3. Testando a Funcionalidade

```bash
# Executar o script de teste
./test-local.sh
```

O script de teste irá:
- Criar arquivos de exemplo (texto e JSON)
- Fazer upload para o S3
- Aguardar o processamento
- Baixar e mostrar os arquivos processados

## 📝 Uso Manual

### Enviando arquivos para processamento:

```bash
# Configurar credenciais para LocalStack
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=us-east-1

# Enviar arquivo de texto
echo "Olá, mundo!" > teste.txt
aws s3 cp teste.txt s3://meu-bucket-upload/uploads/ --endpoint-url http://localhost:4566

# Enviar arquivo JSON
echo '{"nome": "João", "idade": 30}' > dados.json
aws s3 cp dados.json s3://meu-bucket-upload/uploads/ --endpoint-url http://localhost:4566
```

### Verificando resultados:

```bash
# Listar arquivos processados
aws s3 ls s3://meu-bucket-upload/processed/ --recursive --endpoint-url http://localhost:4566

# Baixar arquivo processado
aws s3 cp s3://meu-bucket-upload/processed/teste_processed.txt . --endpoint-url http://localhost:4566

# Ver conteúdo
cat teste_processed.txt
```

## 🔍 Monitoramento e Logs

### Verificar logs da Lambda:

```bash
# Listar grupos de log
aws logs describe-log-groups --endpoint-url http://localhost:4566

# Ver logs da função (substitua o nome do grupo correto)
aws logs describe-log-streams --log-group-name /aws/lambda/local-s3-processor --endpoint-url http://localhost:4566
```

### Verificar status dos serviços LocalStack:

```bash
curl http://localhost:4566/_localstack/health
```

## 🧪 Desenvolvimento

### Executar localmente (sem container):

```bash
# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar testes (se houver)
dotnet test
```

### Modificar a lógica de processamento:

Edite o arquivo `Function.cs` e modifique os métodos:
- `ProcessTextFile()` - para processamento de texto
- `ProcessJsonFile()` - para processamento de JSON
- `ProcessImageFile()` - para processamento de imagens
- `ProcessGenericFile()` - para outros tipos de arquivo

### Rebuild e redeploy:

```bash
./deploy-localstack.sh
```

## 📊 Exemplo de Processamento

### Arquivo de entrada (texto):
```
Olá mundo
Este é um teste
```

### Arquivo processado:
```
=== ARQUIVO PROCESSADO EM 2024-01-15 10:30:45 UTC ===
Linhas originais: 2
Caracteres: 26
=== CONTEÚDO PROCESSADO ===
OLÁ MUNDO
ESTE É UM TESTE
```

## ❗ Troubleshooting

### Erro "sam: command not found":
Se você receber este erro ao executar `./deploy-localstack.sh`:
1. Use o script alternativo: `./deploy-awscli.sh`
2. Ou instale o AWS SAM CLI:
   ```bash
   # macOS
   brew install aws-sam-cli
   
   # Outros sistemas: https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html
   ```

### Lambda não está sendo ativada:
1. Verifique se o LocalStack está rodando: `curl http://localhost:4566/_localstack/health`
2. Confirme que o bucket existe: `aws s3 ls --endpoint-url http://localhost:4566`
3. Verifique se o arquivo foi enviado para a pasta `uploads/`
4. Confirme que a função Lambda existe: `aws lambda list-functions --endpoint-url http://localhost:4566`

### Erro de permissões:
- O LocalStack usa credenciais fake (`test`/`test`), certifique-se de que estão configuradas
- Verifique se a role IAM foi criada corretamente

### Função não encontrada:
- Execute novamente o deploy: `./deploy-awscli.sh`
- Verifique se não há erros durante o build: `dotnet build`

### Logs não aparecem:
- Os logs podem demorar alguns segundos para aparecer no LocalStack
- Verifique o nome correto do grupo de logs
- Use: `aws logs describe-log-groups --endpoint-url http://localhost:4566`

## 🗑️ Limpeza

Para remover tudo e começar limpo:

```bash
# Remover stack do CloudFormation
aws cloudformation delete-stack --stack-name s3-processor-stack --endpoint-url http://localhost:4566

# Parar LocalStack
docker-compose -f localstack-config.yaml down

# Limpar arquivos de teste
rm -f test-*.txt test-*.json
rm -rf processed-files/
```

## 🔧 Configurações Avançadas

### Alterando o bucket de destino:
Modifique o parâmetro `BucketName` no arquivo `samconfig.toml` ou passe como parâmetro no deploy.

### Configurando outros triggers:
Edite o arquivo `template.yaml` na seção `Events` da função Lambda para adicionar outros triggers ou filtros.

### Mudando o processamento:
Personalize os métodos de processamento no arquivo `Function.cs` conforme suas necessidades específicas.
