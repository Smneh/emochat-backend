namespace Contract.DTOs.Chat;

public class GroupVisitorDto
{
    public string Username { get; set; } = default!;
    public string? ProfileAddress { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateTime { get; set; }
}