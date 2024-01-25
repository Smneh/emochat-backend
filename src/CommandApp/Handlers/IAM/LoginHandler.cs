using Contract.Commands.IAM;
using Contract.DTOs.IAM;
using Contract.Queries.Profile;
using MediatR;

namespace CommandApp.Handlers.IAM;

public class LoginHandler : IRequestHandler<LoginCommand, UserData>
{
    private readonly ISender _sender;

    public LoginHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<UserData> Handle(LoginCommand loginRequest, CancellationToken cancellationToken)
    {
        var command = new GetTokenCommand
        {
            Username = loginRequest.Username,
            Password = loginRequest.Password
        };
        
        var token = await _sender.Send(command, cancellationToken);
        
        var profile = await GetUserProfile(loginRequest.Username);
        return new UserData
        {
            Token = token.access_token,
            ValidTo = token.ValidTo,
            Username = loginRequest.Username,
            Fullname = profile!.Fullname,
            ProfilePictureId = profile.ProfilePictureId,
            WallpaperPictureId = profile.WallpaperPictureId
        };
    }
    
    private async Task<Entities.Models.Profile.Profile?> GetUserProfile(string username)
    {
        var query = new GetProfilesByUsernamesQuery
        {
            Usernames = new List<string>
            {
                username
            }
        };
        var profiles = await _sender.Send(query);
        return profiles.FirstOrDefault();
    }
}