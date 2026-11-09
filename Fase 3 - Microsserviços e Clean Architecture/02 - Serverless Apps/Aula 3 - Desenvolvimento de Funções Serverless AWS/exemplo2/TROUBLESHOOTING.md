# Guia de Solução de Problemas - AWS Lambda .NET 8

Este documento contém soluções para problemas comuns ao fazer deploy de funções Lambda .NET na AWS.

## 🔴 Erro: System.BadImageFormatException

### Descrição
```
Unhandled exception. System.BadImageFormatException: An attempt was made to load a program with an incorrect format.
(0x8007000B)
```

### Causa
Incompatibilidade entre a arquitetura do código compilado e a arquitetura de execução do AWS Lambda.

### Solução

1. **Use o script de deploy correto:**
   ```bash
   ./deploy-aws.sh
   ```

2. **Ou compile manualmente com arquitetura específica:**
   ```bash
   # Build para arquitetura x86_64 (padrão AWS Lambda)
   dotnet build --configuration Release --runtime linux-x64 --self-contained false
   
   # Empacotar com arquitetura correta
   dotnet lambda package \
     --configuration Release \
     --framework net8.0 \
     --runtime linux-x64 \
     --architecture x86_64 \
     --output-package bin/lambda-package.zip
   ```

3. **Configurar função Lambda com arquitetura correta:**
   ```bash
   aws lambda create-function \
     --architectures x86_64 \
     # ... outros parâmetros
   ```

### Prevenção
- Sempre especifique `--runtime linux-x64` e `--architecture x86_64`
- Configure o arquivo `.csproj` com:
  ```xml
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <SelfContained>false</SelfContained>
  ```

---

## 🔴 Erro: Credenciais AWS Inválidas

### Descrição
```
An error occurred (InvalidClientTokenId) when calling the GetCallerIdentity operation: The security token included in the request is invalid.
```

### Causa
Credenciais AWS não configuradas, expiradas ou inválidas.

### Solução

1. **Configurar AWS CLI:**
   ```bash
   aws configure
   ```

2. **Ou usar variáveis de ambiente:**
   ```bash
   export AWS_ACCESS_KEY_ID=sua-chave
   export AWS_SECRET_ACCESS_KEY=sua-chave-secreta
   export AWS_DEFAULT_REGION=us-east-1
   ```

3. **Verificar configuração:**
   ```bash
   aws sts get-caller-identity
   aws configure list
   ```

---

## 🔴 Erro: Role não encontrada

### Descrição
```
An error occurred (NoSuchEntity) when calling the GetRole operation: The role with name lambda-execution-role cannot be found.
```

### Causa
Role IAM necessária para execução da Lambda não existe.

### Solução
O script `deploy-aws.sh` cria automaticamente a role necessária. Ou crie manualmente:

1. **Criar trust policy:**
   ```json
   {
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
   }
   ```

2. **Criar role:**
   ```bash
   aws iam create-role \
     --role-name lambda-execution-role \
     --assume-role-policy-document file://trust-policy.json
   
   aws iam attach-role-policy \
     --role-name lambda-execution-role \
     --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole
   ```

---

## 🔴 Erro: Package muito grande

### Descrição
```
RequestEntityTooLargeException: Request must be smaller than 69905067 bytes for the UpdateFunctionCode operation
```

### Causa
Pacote Lambda excede o limite de 50MB para upload direto.

### Solução

1. **Otimizar dependências no `.csproj`:**
   ```xml
   <PublishTrimmed>true</PublishTrimmed>
   <TrimMode>link</TrimMode>
   ```

2. **Usar S3 para packages grandes:**
   ```bash
   # Upload para S3
   aws s3 cp bin/lambda-package.zip s3://seu-bucket/lambda-package.zip
   
   # Deploy via S3
   aws lambda update-function-code \
     --function-name sua-funcao \
     --s3-bucket seu-bucket \
     --s3-key lambda-package.zip
   ```

3. **Remover dependências desnecessárias:**
   - Remova packages não utilizados
   - Use `<PackageReference Include="..." IncludeAssets="compile" PrivateAssets="all" />`

---

## 🔴 Erro: Timeout na execução

### Descrição
```
Task timed out after 3.00 seconds
```

### Causa
Função Lambda excede o tempo limite configurado.

### Solução

1. **Aumentar timeout:**
   ```bash
   aws lambda update-function-configuration \
     --function-name sua-funcao \
     --timeout 30
   ```

2. **Otimizar código:**
   - Use `Task.Run()` para operações CPU-intensivas
   - Configure timeouts em chamadas HTTP
   - Use connection pooling para banco de dados

---

## 🔴 Erro: Memória insuficiente

### Descrição
```
Runtime.ExitError: RequestId: xxx Process exited before completing request
```

### Causa
Função Lambda excede a memória configurada.

### Solução

1. **Aumentar memória:**
   ```bash
   aws lambda update-function-configuration \
     --function-name sua-funcao \
     --memory-size 1024
   ```

2. **Monitorar uso:**
   ```bash
   aws logs filter-log-events \
     --log-group-name /aws/lambda/sua-funcao \
     --filter-pattern "Max Memory Used"
   ```

---

## 📋 Checklist de Deploy

- [ ] AWS CLI configurado e funcionando
- [ ] Amazon.Lambda.Tools instalado (`dotnet tool list -g`)
- [ ] Projeto compila sem erros (`dotnet build`)
- [ ] Arquitetura correta especificada (x86_64)
- [ ] Runtime correto (linux-x64)
- [ ] Role IAM existe e tem permissões adequadas
- [ ] Timeout e memória apropriados para sua função
- [ ] Variáveis de ambiente configuradas se necessário

---

## 🛠️ Comandos Úteis

### Logs em tempo real:
```bash
aws logs tail /aws/lambda/sua-funcao --follow
```

### Testar função diretamente:
```bash
aws lambda invoke \
  --function-name sua-funcao \
  --payload '{"httpMethod":"GET","path":"/health"}' \
  response.json
```

### Ver configuração da função:
```bash
aws lambda get-function --function-name sua-funcao
```

### Listar todas as funções:
```bash
aws lambda list-functions
```

---

## 📞 Suporte

Se você encontrar outros problemas:

1. Verifique os logs no CloudWatch
2. Confirme que está usando a versão mais recente do .NET 8
3. Teste localmente com o LocalStack primeiro
4. Verifique se todas as dependências são compatíveis com Linux

Para mais informações, consulte a [documentação oficial da AWS](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html).
