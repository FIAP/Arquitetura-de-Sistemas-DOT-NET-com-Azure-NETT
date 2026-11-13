using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using S3UploadApi.Options;

namespace S3UploadApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController(IAmazonS3 s3Client, AwsS3Options s3Options) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromQuery] string? key = null, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Arquivo não enviado ou vazio.");
        }

        string objectKey = string.IsNullOrWhiteSpace(key) ? file.FileName : key!;

        await using var stream = file.OpenReadStream();
        var putRequest = new PutObjectRequest
        {
            BucketName = s3Options.BucketName,
            Key = objectKey,
            InputStream = stream,
            AutoCloseStream = false,
            ContentType = file.ContentType
        };

        var response = await s3Client.PutObjectAsync(putRequest, cancellationToken);

        return Ok(new
        {
            s3Options.BucketName,
            Key = objectKey,
            ETag = response.ETag
        });
    }
}


