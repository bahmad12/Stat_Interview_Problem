using Amazon.S3;
using Amazon.S3.Model;

namespace StatRecoveryProcessor.Services;

/// <summary>
/// Service for handling Amazon S3 operations including file upload, download, and listing.
/// </summary>
public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    /// <summary>
    /// Initializes a new instance of the S3Service.
    /// </summary>
    /// <param name="s3Client">The Amazon S3 client for AWS operations.</param>
    /// <param name="bucketName">The target S3 bucket name.</param>
    public S3Service(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    /// <summary>
    /// Downloads a file from S3 as a stream.
    /// </summary>
    /// <param name="key">The S3 object key to download.</param>
    /// <returns>A stream containing the file contents.</returns>
    public async Task<Stream> DownloadFileAsync(string key)
    {
        Console.WriteLine($"Downloading file from S3: {key}");
        var response = await _s3Client.GetObjectAsync(_bucketName, key);
        Console.WriteLine($"Successfully downloaded file: {key}, Content Length: {response.ContentLength}");
        return response.ResponseStream;
    }

    /// <summary>
    /// Uploads a stream to S3.
    /// </summary>
    /// <param name="key">The S3 object key to create/update.</param>
    /// <param name="content">The stream containing the file contents.</param>
    public async Task UploadFileAsync(string key, Stream content)
    {
        Console.WriteLine($"Uploading file to S3: {key}");
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content
        };

        await _s3Client.PutObjectAsync(request);
        Console.WriteLine($"Successfully uploaded file: {key}");
    }

    /// <summary>
    /// Lists all files in S3 with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix to filter S3 objects.</param>
    /// <returns>A list of S3 object keys matching the prefix.</returns>
    public async Task<List<string>> ListFilesAsync(string prefix)
    {
        Console.WriteLine($"Listing files in S3 with prefix: {prefix}");
        var files = new List<string>();
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = prefix
        };

        ListObjectsV2Response response;
        var pageCount = 0;
        do
        {
            pageCount++;
            response = await _s3Client.ListObjectsV2Async(request);
            files.AddRange(response.S3Objects.Select(x => x.Key));
            request.ContinuationToken = response.NextContinuationToken;
            Console.WriteLine($"Retrieved page {pageCount} with {response.S3Objects.Count} objects");
        } while (response.IsTruncated);

        Console.WriteLine($"Found total of {files.Count} files");
        return files;
    }
} 