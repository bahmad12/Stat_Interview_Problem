using Amazon.S3;
using Microsoft.Extensions.Configuration;
using StatRecoveryProcessor.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var awsOptions = new AmazonS3Config
{
    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
};

// Note: AWS credentials should be configured via environment variables:
// AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY
var s3Client = new AmazonS3Client(awsOptions);
var s3Service = new S3Service(s3Client, configuration["AWS:BucketName"]!);
var processor = new FileProcessorService(s3Service);

try
{
    await processor.ProcessFilesAsync();
    Console.WriteLine("Processing completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Environment.Exit(1);
}
