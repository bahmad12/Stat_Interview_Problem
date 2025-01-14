using System;
using System.Collections.Generic;

namespace StatRecoveryProcessor.Models;

public class ProcessingMetadata
{
    public string ZipFileName { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
    public List<ProcessedFile> ProcessedFiles { get; set; } = new();
}

public class ProcessedFile
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string PoNumber { get; set; } = string.Empty;
    public string S3Path { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
} 