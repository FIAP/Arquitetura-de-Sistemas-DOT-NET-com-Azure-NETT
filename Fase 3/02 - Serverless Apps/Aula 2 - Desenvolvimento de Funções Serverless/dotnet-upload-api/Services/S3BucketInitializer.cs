using Amazon.S3;
using Amazon.S3.Model;
using S3UploadApi.Options;

namespace S3UploadApi.Services;

public class S3BucketInitializer(IAmazonS3 s3Client, AwsS3Options options, ILogger<S3BucketInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var listResponse = await s3Client.ListBucketsAsync(cancellationToken);
            bool exists = listResponse.Buckets.Any(b => string.Equals(b.BucketName, options.BucketName, StringComparison.Ordinal));
            if (!exists)
            {
                await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = options.BucketName }, cancellationToken);
                logger.LogInformation("Bucket '{Bucket}' criado na LocalStack.", options.BucketName);
            }
            else
            {
                logger.LogInformation("Bucket '{Bucket}' já existe.", options.BucketName);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao garantir bucket '{Bucket}'. Verifique se a LocalStack está ativa (porta 4566).", options.BucketName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}


