public class ProcessedFile
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string PoNumber { get; set; } = string.Empty;
    public string S3Path { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
} 