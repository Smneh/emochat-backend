using Microsoft.AspNetCore.Http;

namespace Contract.DTOs.FileHandler;

public class UploadDto
{
    public IFormFile File { get; set; }
    public string Guid { get; set; }
    public string FileName { get; set; }
    public string Username { get; set; }
    public DateTime Timestamp { get; set; }
}