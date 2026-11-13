FROM public.ecr.aws/lambda/dotnet:8

# Copiar arquivos de projeto e restaurar dependências
COPY *.csproj ${LAMBDA_TASK_ROOT}/
RUN dotnet restore

# Copiar código fonte
COPY . ${LAMBDA_TASK_ROOT}/

# Build da aplicação
RUN dotnet publish -c Release -o out

# Configurar o handler
CMD ["S3ProcessorLambda::S3ProcessorLambda.Function::FunctionHandler"]
