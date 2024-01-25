namespace Contract.DTOs.FileHandler;

public class FileInfo
{
    public string Extension { get; set; }
    public string FileName { get; set; }
    public string Username { get; set; }
    public DateTime RegDate { get; set; }
    public string RegTime { get; set; }
    public string UniqueId { get; set; }
    public string Type { get; set; }
    public string HashCode { get; set; }
    public DateTime Timestamp { get; set; }
    public string FileHandler { get; set; }
    public string NameSpace { get; set; }
    public long FileSize { get; set; }
    public string FileGuid { get; set; }
}