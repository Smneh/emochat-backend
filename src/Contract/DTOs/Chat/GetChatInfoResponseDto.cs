namespace Contract.DTOs.Chat;

public class GetChatInfoResponseDto
{
    public string Creator { get; set; }
    public int FirstUnreadMessageId { get; set; }
    public int FollowerCount { get; set; }
    public string GroupId { get; set; }
    public string ReceiverId { get; set; }
    public string LastActionTime { get; set; }
    public int MembersSetId { get; set; }
    public string ProfilePictureId { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public List<long> UnReadMessages { get; set; }
    public bool CopyStatus { get; set; }
    public bool SendFileStatus { get; set; }
    public bool LinkStatus { get; set; }
}