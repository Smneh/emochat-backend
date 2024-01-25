namespace Contract.DTOs.Chat;

public class GroupMemberDto
{
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string ProfilePictureId { get; set; } = default!;
    public bool IsCreator { get; set; } = default!;
    public bool IsAdmin { get; set; } = default!;
}