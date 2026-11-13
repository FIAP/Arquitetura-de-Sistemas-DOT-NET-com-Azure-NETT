# Documentação

## Diagrama de Sequência – Fluxo de Upload (LocalStack)

```mermaid
sequenceDiagram
    autonumber
    participant C as Cliente
    participant API as S3UploadApi (.NET 10)
    participant HS as S3BucketInitializer
    participant S3 as IAmazonS3 (AWS SDK)
    participant LS as LocalStack S3 (http://localhost:4566)

    rect rgb(245,245,245)
        note over HS,LS: Inicialização da API
        HS->>S3: ListBuckets()
        alt Bucket "uploads" inexistente
            HS->>S3: PutBucket("uploads")
        else Já existe
            HS-->>S3: (sem ação)
        end
    end

    C->>API: POST /api/upload (multipart/form-data: file)
    API->>API: Validar arquivo (nulo/vazio?)
    alt Arquivo inválido
        API-->>C: 400 BadRequest ("Arquivo não enviado ou vazio.")
    else OK
        API->>S3: PutObject(Bucket="uploads", Key, Stream, ContentType)
        S3->>LS: HTTP PUT (S3 compatível)
        LS-->>S3: 200 + ETag
        S3-->>API: ETag
        API-->>C: 200 { bucketName, key, eTag }
    end
```

### Observações
- O bucket padrão é `uploads` (configurável em `appsettings.Development.json` → `AwsS3`).
- A URL e o `ForcePathStyle=true` apontam para a LocalStack (porta 4566).
- O endpoint de upload é `POST /api/upload` aceitando `multipart/form-data` com campo `file`.


