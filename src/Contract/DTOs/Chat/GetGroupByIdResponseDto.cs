namespace Contract.DTOs.Chat;

public class GetGroupByIdResponseDto
{
    public string Type { get; set; }
    public string GroupId { get; set; }
    public string Description { get; set; }
    public string Creator { get; set; }
    public DateTime RegDateTime { get; set; }
    public long MembersSetId { get; set; }
    public long? AdminsSetId { get; set; }
    public int CopyStatus { get; set; }
    public string WallpaperPictureId { get; set; }
    public string ProfilePictureId { get; set; }
    public int LinkStatus { get; set; }
    public int SendFileStatus { get; set; }
    public int TypeId { get; set; }
    public List<string> Members { get; set; }
    public List<GroupMemberDto> GroupMembers { get; set; } = new();
    public List<string> Admins { get; set; }
    public int MembersCount { get; set; }
    public string Title { get; set; }
    public long SqlId { get; set; }

}