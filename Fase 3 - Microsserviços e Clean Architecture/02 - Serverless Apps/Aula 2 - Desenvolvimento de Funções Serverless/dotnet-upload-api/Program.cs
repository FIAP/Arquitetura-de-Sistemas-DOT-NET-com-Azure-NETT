using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

// Configurações AWS S3 / LocalStack
builder.Services.AddOptions<S3UploadApi.Options.AwsS3Options>()
    .Bind(builder.Configuration.GetSection("AwsS3"))
    .ValidateDataAnnotations();

builder.Services.AddSingleton(provider =>
{
    var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<S3UploadApi.Options.AwsS3Options>>().Value;
    return options;
});

builder.Services.AddSingleton<Amazon.S3.IAmazonS3>(sp =>
{
    var opts = sp.GetRequiredService<S3UploadApi.Options.AwsS3Options>();
    var config = new Amazon.S3.AmazonS3Config
    {
        ServiceURL = opts.ServiceURL,
        ForcePathStyle = opts.ForcePathStyle,
        AuthenticationRegion = opts.Region
    };

    var creds = new Amazon.Runtime.BasicAWSCredentials(opts.AccessKey, opts.SecretKey);
    return new Amazon.S3.AmazonS3Client(creds, config);
});

builder.Services.AddHostedService<S3UploadApi.Services.S3BucketInitializer>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapScalarApiReference();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
