namespace StatRecoveryProcessor.Models;

public class ProcessingMetadata
{
    public string ZipFileName { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
    public List<ProcessedFile> ProcessedFiles { get; set; } = new();
}