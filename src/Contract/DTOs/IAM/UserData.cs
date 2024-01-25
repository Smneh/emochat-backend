namespace Contract.DTOs.IAM;

public class UserData
{
    public string? Token { get; set; }
    public DateTime? ValidTo { get; set; }
    public string Username { get; set; } = default!;
    public string Fullname { get; set; } = default!;
    public string? ProfilePictureId { get; set; }
    public string? WallpaperPictureId { get; set; }
}