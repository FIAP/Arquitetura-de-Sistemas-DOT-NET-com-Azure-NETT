## S3UploadApi (LocalStack + .NET 10)

### Pré-requisitos
- .NET 10 Preview
- LocalStack em execução com serviço S3 (porta 4566)
- AWS CLI ou awslocal (opcional para criar bucket)

### Configuração
Edite `appsettings.Development.json` se quiser alterar `BucketName`, `ServiceURL` ou credenciais.

### Criar bucket na LocalStack
```bash
awslocal s3 mb s3://uploads --endpoint-url=http://localhost:4566
```

### Executar a API
```bash
dotnet run --project S3UploadApi
```

Swagger/Scalar: `http://localhost:5223/scalar/v1`

### Endpoint de Upload
`POST /api/upload` (multipart/form-data)

Exemplo (curl):
```bash
curl -X POST "http://localhost:5223/api/upload?key=arquivo.txt" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@./arquivo.txt"
```

Resposta esperada:
```json
{ "bucketName": "uploads", "key": "arquivo.txt", "eTag": "\"...\"" }
```


