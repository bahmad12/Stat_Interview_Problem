using CsvHelper;
using CsvHelper.Configuration;
using StatRecoveryProcessor.Models;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace StatRecoveryProcessor.Services;

/// <summary>
/// Service for processing ZIP files containing CSV and PDF documents, uploading them to S3,
/// and maintaining processing metadata.
/// </summary>
public class FileProcessorService
{
    private readonly S3Service _s3Service;
    private const string METADATA_KEY = "processing-metadata.json";
    private List<ProcessingMetadata> _processedFiles = new();

    /// <summary>
    /// Initializes a new instance of the FileProcessorService.
    /// </summary>
    /// <param name="s3Service">The S3 service for file operations.</param>
    public FileProcessorService(S3Service s3Service)
    {
        _s3Service = s3Service;
    }

    /// <summary>
    /// Processes all unprocessed ZIP files in the S3 bucket.
    /// </summary>
    public async Task ProcessFilesAsync()
    {
        Console.WriteLine("Starting file processing operation");
        await LoadMetadataAsync();
        var zipFiles = await _s3Service.ListFilesAsync("");
        
        var unprocessedFiles = zipFiles.Where(f => f.EndsWith(".zip") && !_processedFiles.Any(p => p.ZipFileName == f)).ToList();
        Console.WriteLine($"Found {unprocessedFiles.Count} unprocessed ZIP files");

        foreach (var zipFile in unprocessedFiles)
        {
            await ProcessZipFileAsync(zipFile);
        }

        await SaveMetadataAsync();
        Console.WriteLine("Completed file processing operation");
    }

    /// <summary>
    /// Processes a single ZIP file, extracting CSV data and associated PDFs.
    /// </summary>
    /// <param name="zipFile">The S3 key of the ZIP file to process.</param>
    private async Task ProcessZipFileAsync(string zipFile)
    {
        Console.WriteLine($"\nProcessing ZIP file: {zipFile}");
        var metadata = new ProcessingMetadata
        {
            ZipFileName = zipFile,
            ProcessedDate = DateTime.UtcNow
        };

        var tempZipPath = Path.GetTempFileName();
        try
        {
            // Download and extract ZIP file
            using (var zipStream = await _s3Service.DownloadFileAsync(zipFile))
            using (var fileStream = File.Create(tempZipPath))
            {
                await zipStream.CopyToAsync(fileStream);
            }

            using var archive = ZipFile.OpenRead(tempZipPath);
            var csvFile = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".csv"));
            if (csvFile == null)
            {
                Console.WriteLine($"No CSV file found in {zipFile}");
                return;
            }

            Console.WriteLine($"Processing CSV file: {csvFile.Name}");
            var tempCsvPath = Path.GetTempFileName();
            csvFile.ExtractToFile(tempCsvPath, true);

            // Configure CSV reader
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "~",
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            // Read CSV records
            List<ProcessedCSVMetadata> records;
            using (var csvReader = new StreamReader(tempCsvPath))
            using (var csv = new CsvReader(csvReader, config))
            {
                // Map csv content ProcessedCSVMetadata
                csv.Context.RegisterClassMap<ProcessedCSVMetadataMap>();
                records = csv.GetRecords<ProcessedCSVMetadata>().ToList();
                Console.WriteLine($"Successfully parsed {records.Count} records from CSV");
            }

            File.Delete(tempCsvPath);

            // Process PDF attachments
            var pdfFiles = archive.Entries.Where(e => e.Name.EndsWith(".pdf")).ToList();
            Console.WriteLine($"Found {pdfFiles.Count} PDF files to process");

            foreach (var entry in pdfFiles)
            {
                Console.WriteLine($"\nProcessing PDF: {entry.Name}");
                var record = records.FirstOrDefault(r => r.AttachmentList.Any(a => a.Contains(entry.Name, StringComparison.OrdinalIgnoreCase)));

                if (record == null)
                {
                    Console.WriteLine($"Warning: No matching record found for PDF: {entry.Name}");
                    continue;
                }

                if (string.IsNullOrEmpty(record.PONumber) || !record.PONumber.All(char.IsDigit))
                {
                    Console.WriteLine($"Warning: Invalid PO number '{record.PONumber}' for file: {entry.Name}");
                    continue;
                }

                // Upload PDF to S3
                var tempPdfPath = Path.GetTempFileName();
                entry.ExtractToFile(tempPdfPath, true);

                var s3Path = $"by-po/{record.PONumber}/{entry.Name}";
                using (var fileStream = File.OpenRead(tempPdfPath))
                {
                    await _s3Service.UploadFileAsync(s3Path, fileStream);
                }

                File.Delete(tempPdfPath);

                metadata.ProcessedFiles.Add(new ProcessedFile
                {
                    OriginalFileName = entry.Name,
                    PoNumber = record.PONumber,
                    S3Path = s3Path,
                    ProcessedDate = DateTime.UtcNow
                });
                Console.WriteLine($"Successfully processed PDF: {entry.Name} -> {s3Path}");
            }

            _processedFiles.Add(metadata);
            Console.WriteLine($"Completed processing ZIP file: {zipFile}");
        }
        finally
        {
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }
        }
    }

    /// <summary>
    /// Loads processing metadata from S3.
    /// </summary>
    private async Task LoadMetadataAsync()
    {
        Console.WriteLine("Loading processing metadata from S3");
        try
        {
            using var stream = await _s3Service.DownloadFileAsync(METADATA_KEY);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            _processedFiles = JsonSerializer.Deserialize<List<ProcessingMetadata>>(json) ?? new();
            Console.WriteLine($"Loaded metadata for {_processedFiles.Count} previously processed files");
        }
        catch
        {
            Console.WriteLine("No existing metadata found, starting fresh");
            _processedFiles = new List<ProcessingMetadata>();
        }
    }

    /// <summary>
    /// Saves processing metadata to S3.
    /// </summary>
    private async Task SaveMetadataAsync()
    {
        Console.WriteLine("Saving processing metadata to S3");
        var json = JsonSerializer.Serialize(_processedFiles);
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();
        stream.Position = 0;
        await _s3Service.UploadFileAsync(METADATA_KEY, stream);
        Console.WriteLine($"Successfully saved metadata for {_processedFiles.Count} processed files");
    }
} 