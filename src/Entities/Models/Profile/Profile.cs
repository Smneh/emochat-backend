namespace Entities.Models.Profile;

public class Profile
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Fullname { get; set; } = default!;
    public string? Email { get; set; }
    public string? ProfilePictureId { get; set; }
    public string? WallpaperPictureId { get; set; }
}