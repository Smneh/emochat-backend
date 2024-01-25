namespace Entities.Models.Chat;

public class Group
{
    public string Type { get; set; }
    public string GroupId { get; set; }
    public string Description { get; set; }
    public string Creator { get; set; }
    public DateTime RegDateTime { get; set; }
    public string ProfilePictureId { get; set; }
    public List<string> Members { get; set; }
    public List<string> Admins { get; set; }
    public int MembersCount { get; set; }
    public string Title { get; set; }
    public string Guid { get; set; }
}