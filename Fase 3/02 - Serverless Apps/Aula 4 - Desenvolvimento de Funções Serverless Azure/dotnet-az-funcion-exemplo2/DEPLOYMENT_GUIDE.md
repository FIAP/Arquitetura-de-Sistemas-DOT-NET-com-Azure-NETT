# Guia de Deploy - Sistema de Gestão de Tarefas

Este guia irá te ajudar a configurar o deploy automático do projeto Azure Functions usando GitHub Actions com Azure Workload Identity Federation.

## Pré-requisitos

- Conta do Azure com permissões de contribuidor
- Repositório no GitHub
- Azure CLI instalado localmente
- PowerShell ou Bash

## Configuração do Azure

### 1. Criar Function Apps

Primeiro, você precisa criar as Function Apps no Azure:

```bash
# Variáveis
RESOURCE_GROUP="rg-task-management"
LOCATION="East US"
STORAGE_ACCOUNT="sttaskmanagement$(date +%s)"
FUNCTION_APP_PROD="task-management-functions"
FUNCTION_APP_DEV="task-management-functions-dev"

# Criar Resource Group
az group create --name $RESOURCE_GROUP --location "$LOCATION"

# Criar Storage Account
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location "$LOCATION" \
  --sku Standard_LRS \
  --functions-version 4

# Criar Function App - Produção
az functionapp create \
  --resource-group $RESOURCE_GROUP \
  --consumption-plan-location "$LOCATION" \
  --runtime dotnet-isolated \
  --functions-version 4 \
  --name $FUNCTION_APP_PROD \
  --storage-account $STORAGE_ACCOUNT \
  --disable-app-insights false

# Criar Function App - Desenvolvimento  
az functionapp create \
  --resource-group $RESOURCE_GROUP \
  --consumption-plan-location "$LOCATION" \
  --runtime dotnet-isolated \
  --functions-version 4 \
  --name $FUNCTION_APP_DEV \
  --storage-account $STORAGE_ACCOUNT \
  --disable-app-insights false
```

### 2. Configurar Azure AD App Registration

#### Para Produção:

```bash
# Criar App Registration para Produção
APP_NAME="TaskManagement-GitHub-Prod"
SUBSCRIPTION_ID=$(az account show --query id --output tsv)

# Criar a aplicação
az ad app create --display-name $APP_NAME

# Obter Application ID
CLIENT_ID=$(az ad app list --display-name $APP_NAME --query "[0].appId" --output tsv)

# Criar Service Principal
az ad sp create --id $CLIENT_ID

# Obter Object ID do Service Principal
OBJECT_ID=$(az ad sp show --id $CLIENT_ID --query id --output tsv)

# Atribuir permissões
az role assignment create \
  --role "Contributor" \
  --assignee $CLIENT_ID \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP"

echo "=== CONFIGURAÇÕES PARA PRODUÇÃO ==="
echo "AZUREAPPSERVICE_CLIENTID: $CLIENT_ID"
echo "AZUREAPPSERVICE_TENANTID: $(az account show --query tenantId --output tsv)"
echo "AZUREAPPSERVICE_SUBSCRIPTIONID: $SUBSCRIPTION_ID"
```

#### Para Desenvolvimento:

```bash
# Criar App Registration para Desenvolvimento
APP_NAME_DEV="TaskManagement-GitHub-Dev"

# Criar a aplicação
az ad app create --display-name $APP_NAME_DEV

# Obter Application ID
CLIENT_ID_DEV=$(az ad app list --display-name $APP_NAME_DEV --query "[0].appId" --output tsv)

# Criar Service Principal
az ad sp create --id $CLIENT_ID_DEV

# Atribuir permissões
az role assignment create \
  --role "Contributor" \
  --assignee $CLIENT_ID_DEV \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP"

echo "=== CONFIGURAÇÕES PARA DESENVOLVIMENTO ==="
echo "AZUREAPPSERVICE_CLIENTID_DEV: $CLIENT_ID_DEV"
echo "AZUREAPPSERVICE_TENANTID_DEV: $(az account show --query tenantId --output tsv)"
echo "AZUREAPPSERVICE_SUBSCRIPTIONID_DEV: $SUBSCRIPTION_ID"
```

### 3. Configurar Federated Credentials

#### Para Produção (branch main):

```bash
# Configurar credencial federada para branch main
az ad app federated-credential create \
  --id $CLIENT_ID \
  --parameters '{
    "name": "TaskManagement-Main-Branch",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:SEU_USUARIO/SEU_REPOSITORIO:ref:refs/heads/main",
    "description": "GitHub Actions - Main Branch",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

#### Para Desenvolvimento (branch develop):

```bash
# Configurar credencial federada para branch develop
az ad app federated-credential create \
  --id $CLIENT_ID_DEV \
  --parameters '{
    "name": "TaskManagement-Develop-Branch", 
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:SEU_USUARIO/SEU_REPOSITORIO:ref:refs/heads/develop",
    "description": "GitHub Actions - Develop Branch",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

**IMPORTANTE:** Substitua `SEU_USUARIO/SEU_REPOSITORIO` pelo seu repositório GitHub real!

## Configuração dos Secrets no GitHub

### 1. Acessar Settings do Repositório

1. Vá para seu repositório no GitHub
2. Clique em **Settings**
3. Na barra lateral, clique em **Secrets and variables** > **Actions**

### 2. Adicionar Secrets

Clique em **New repository secret** e adicione os seguintes secrets:

#### Para Produção:
- **Name:** `AZUREAPPSERVICE_CLIENTID`
- **Value:** [CLIENT_ID do passo anterior]

- **Name:** `AZUREAPPSERVICE_TENANTID`
- **Value:** [TENANT_ID do passo anterior]

- **Name:** `AZUREAPPSERVICE_SUBSCRIPTIONID`
- **Value:** [SUBSCRIPTION_ID do passo anterior]

#### Para Desenvolvimento:
- **Name:** `AZUREAPPSERVICE_CLIENTID_DEV`
- **Value:** [CLIENT_ID_DEV do passo anterior]

- **Name:** `AZUREAPPSERVICE_TENANTID_DEV`
- **Value:** [TENANT_ID do passo anterior]

- **Name:** `AZUREAPPSERVICE_SUBSCRIPTIONID_DEV`
- **Value:** [SUBSCRIPTION_ID do passo anterior]

## Configuração de Environments

### 1. Criar Environments

No GitHub:
1. Vá para **Settings** > **Environments**
2. Clique em **New environment**
3. Crie dois environments:
   - `production`
   - `development`

### 2. Configurar Protection Rules (Recomendado)

Para o environment `production`:
1. Marque **Required reviewers** e adicione revisores
2. Marque **Wait timer** e defina 5 minutos
3. Marque **Deployment branches** e selecione **Selected branches**
4. Adicione a regra: `main`

## Testando o Deploy

### 1. Primeiro Deploy - Desenvolvimento

```bash
# Criar branch develop se não existir
git checkout -b develop

# Fazer alguma mudança pequena
echo "# Deploy Test" >> TEST.md
git add .
git commit -m "test: deploy de desenvolvimento"
git push origin develop
```

### 2. Deploy para Produção

```bash
# Fazer merge para main
git checkout main
git merge develop
git push origin main
```

### 3. Verificar o Deploy

Após o deploy, verifique:

```bash
# Health check - Desenvolvimento
curl "https://task-management-functions-dev.azurewebsites.net/api/health"

# Health check - Produção
curl "https://task-management-functions.azurewebsites.net/api/health"

# Listar tarefas
curl "https://task-management-functions.azurewebsites.net/api/tasks"
```

## Troubleshooting

### Erro: "Login failed"

**Problema:** Federated credential não configurado corretamente.

**Solução:**
1. Verifique se o subject está correto: `repo:USUARIO/REPOSITORIO:ref:refs/heads/BRANCH`
2. Verifique se o CLIENT_ID está correto nos secrets
3. Aguarde alguns minutos para propagação

### Erro: "Insufficient permissions"

**Problema:** Service Principal sem permissões.

**Solução:**
```bash
# Recriar role assignment
az role assignment create \
  --role "Contributor" \
  --assignee $CLIENT_ID \
  --scope "/subscriptions/$SUBSCRIPTION_ID"
```

### Erro: "Function App not found"

**Problema:** Nome da Function App incorreto.

**Solução:**
1. Verifique o nome no arquivo `.github/workflows/ci-cd.yml`
2. Confirme que a Function App existe no Azure

### Health Check falhando

**Problema:** Function App não está respondendo.

**Solução:**
1. Aguarde alguns minutos após o deploy
2. Verifique os logs no portal Azure
3. Verifique se todas as configurações estão corretas

## Monitoramento

### Application Insights

Configure o Application Insights para monitorar a aplicação:

```bash
# Criar Application Insights
az monitor app-insights component create \
  --app task-management-insights \
  --location "$LOCATION" \
  --resource-group $RESOURCE_GROUP

# Obter connection string
INSIGHTS_KEY=$(az monitor app-insights component show \
  --app task-management-insights \
  --resource-group $RESOURCE_GROUP \
  --query instrumentationKey --output tsv)

# Configurar na Function App
az functionapp config appsettings set \
  --name $FUNCTION_APP_PROD \
  --resource-group $RESOURCE_GROUP \
  --settings "APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=$INSIGHTS_KEY"
```

### Verificar Logs

```bash
# Logs em tempo real
az webapp log tail --name $FUNCTION_APP_PROD --resource-group $RESOURCE_GROUP

# Download de logs
az webapp log download --name $FUNCTION_APP_PROD --resource-group $RESOURCE_GROUP
```

## Checklist Final

- [ ] Function Apps criadas no Azure
- [ ] App Registrations configurados
- [ ] Federated Credentials criados
- [ ] Secrets configurados no GitHub
- [ ] Environments configurados
- [ ] Primeiro deploy testado
- [ ] Health checks funcionando
- [ ] Application Insights configurado

## Suporte

Se encontrar problemas:

1. **Verifique os logs** do GitHub Actions
2. **Consulte a documentação** oficial da Microsoft
3. **Verifique as permissões** do Service Principal
4. **Teste localmente** com Azure CLI

---

**Parabéns! Seu pipeline CI/CD está configurado e funcionando!**

*Para dúvidas, consulte a documentação oficial ou abra uma issue no repositório.*