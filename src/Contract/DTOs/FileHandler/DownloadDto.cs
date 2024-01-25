namespace Contract.DTOs.FileHandler;

public class DownloadDto
{
    public string FileName { get; set; }
    public DateTime Timestamp { get; set; }
    public byte[] File { get; set; }
    public string Extension { get; set; }
}