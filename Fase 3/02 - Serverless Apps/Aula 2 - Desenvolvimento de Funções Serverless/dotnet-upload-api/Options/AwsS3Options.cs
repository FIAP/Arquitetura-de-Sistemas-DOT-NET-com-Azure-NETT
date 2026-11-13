namespace S3UploadApi.Options;

public class AwsS3Options
{
    public string ServiceURL { get; set; } = "http://localhost:4566";
    public bool ForcePathStyle { get; set; } = true;
    public string AccessKey { get; set; } = "test";
    public string SecretKey { get; set; } = "test";
    public string Region { get; set; } = "us-east-1";
    public string BucketName { get; set; } = "uploads";
}


