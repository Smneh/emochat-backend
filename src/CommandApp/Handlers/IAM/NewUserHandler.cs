using System.Globalization;
using Aerospike.Client;
using Contract.Commands.IAM;
using Contract.DTOs.IAM;
using Core.Encryption;
using Core.Enums;
using Core.Exceptions;
using MediatR;
using Repository.AeroSpike;
using Repository.Profile;
using User = Entities.Models.Profile.User;

namespace CommandApp.Handlers.IAM;

public class NewUserHandler : IRequestHandler<NewUserCommand, UserData>
{
    private readonly UserRepository _userRepository;
    private readonly ProfileRepository _profileRepository;
    private readonly ISender _sender;
    
    private const string Namespace = "IAM";

    public NewUserHandler(UserRepository userRepository, ProfileRepository profileRepository, ISender sender)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _sender = sender;
    }

    public async Task<UserData> Handle(NewUserCommand user, CancellationToken cancellationToken)
    {
        var checkUser = await _profileRepository.GetUser(user.Username);
        
        if (checkUser != null)
            throw new AppException(Messages.UsernameExist);
        
        if (await _profileRepository.IsEmailExist(user.Email))
            throw new AppException(Messages.EmailExist);

        var pass = user.Password;
        user.Password = PasswordHelper.EncryptString(user.Password);
        
        var key = new Key(Namespace, "Users", user.Username);
        var binsList = new List<Bin>
        {
            new("password", user.Password),
            new("regDate", DateTime.Now.ToString(CultureInfo.CurrentCulture)),
            new("fullName", user.Fullname),
            new("email", user.Email)
        };

        await _userRepository.AddNewUser(binsList, key);
        await RegisterNewProfile(user);

        var getTokenCommand = new GetTokenCommand
        {
            Username = user.Username,
            Password = pass
        };
        
        var token = await _sender.Send(getTokenCommand, cancellationToken);
        return new UserData
        {
            Token = token.access_token,
            ValidTo = token.ValidTo,
            Username = user.Username,
            Fullname = user.Fullname,
            ProfilePictureId = "-",
            WallpaperPictureId = "-"
        };
    }

    private async Task RegisterNewProfile(NewUserCommand command)
    {
        var profile = new User()
        {
            Id = Guid.NewGuid().ToString(),
            Username = command.Username,
            Fullname = command.Fullname,
            ProfilePictureId = "-",
            WallpaperPictureId = "-",
            Email = command.Email,
            Password = command.Password
        };
        
        await _profileRepository.IndexProfile(profile);
    }
}