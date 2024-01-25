namespace Entities.Models.Profile;

public class User : Profile
{
    public string Password { get; set; } = default!;
}