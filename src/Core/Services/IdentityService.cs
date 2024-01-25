namespace Core.Services;

public class IdentityService
{
    public IdentityService()
    {
    }

    public IdentityService(string username)
    {
        Username = username;
    }

    public string Username { get; set; }
}