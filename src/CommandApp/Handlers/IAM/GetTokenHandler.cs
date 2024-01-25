using System.Security.Authentication;
using Aerospike.Client;
using Contract.Commands.IAM;
using Contract.DTOs.IAM;
using Core.Encryption;
using Core.Jwt;
using Core.Settings;
using Entities.Models.IAM;
using MediatR;
using Repository.AeroSpike;
using Repository.Profile;
using User = Entities.Models.Profile.User;

namespace CommandApp.Handlers.IAM;

public class GetTokenHandler : IRequestHandler<GetTokenCommand, UserToken>
{
    private readonly ITokenHelper _tokenHelper;
    private readonly UserRepository _userRepository;
    private readonly ProfileRepository _profileRepository;

    public GetTokenHandler(ITokenHelper tokenHelper, UserRepository userRepository, ProfileRepository profileRepository)
    {
        _tokenHelper = tokenHelper;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
    }

    public async Task<UserToken> Handle(GetTokenCommand request, CancellationToken cancellationToken)
    {
        return await GetToken(request);
    }
    
    private async Task<UserToken> GetToken(GetTokenCommand loginRequest)
    {
        var user = await _profileRepository.GetUser(loginRequest.Username);
        if (user == null)
            throw new AuthenticationException();

        var password = PasswordHelper.EncryptString(loginRequest.Password);

        if (user.Username != loginRequest.Username || user.Password != password)
            throw new AuthenticationException();//Todo check with _securityService

        var expirationDateTime = DateTime.Now.AddMinutes(Settings.AllSettings.JwtSettings.WebExpirationTimeInMinutes);
        var sessionId = await _registerActiveSession(user, expirationDateTime);
        var tokens = await _getToken(user, sessionId, expirationDateTime);
        return tokens;
    }

    private async Task<string> _registerActiveSession(User user, DateTime expirationDateTime)
    {
        var activeSession = new ActiveSession
        {
            StartTime = DateTime.Now,
            ExpireTime = expirationDateTime,
            UniqueId = Guid.NewGuid().ToString().Replace("-", ""),
            Username = user.Username,
            Ip = "",
            IsSuccessor = false,
            IsSetToKillWebSessions = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development",
        };

        var sessionId = await RegisterActiveSessionAerospike(user, activeSession);

        return sessionId;
    }

    private async Task<string> RegisterActiveSessionAerospike(User userMainInfoDto,
        ActiveSession activeSession)
    {
        var currentDateTime = DateTime.Now;
        var ttlSeconds = (int)(activeSession.ExpireTime - currentDateTime).TotalSeconds;
        var sessionId = Guid.NewGuid().ToString();
        // Create a unique key for each session using the user's username and the session ID
        var key = new Key("IAM", "Sessions", $"{userMainInfoDto.Username}_{sessionId}");

        var sessionEntry = CreateActiveSessionsEntry(activeSession, sessionId);

        var binsList = new List<Bin>
        {
            new("username", userMainInfoDto.Username), // Add the username as a separate bin
            new("activeSession", sessionEntry),
        };

        var policy = new WritePolicy
        {
            recordExistsAction = RecordExistsAction.UPDATE,
            expiration = ttlSeconds
        };

        await _userRepository.UpdateUserSession(binsList, key, policy);
        await _userRepository.GetUserActiveSessions(userMainInfoDto.Username);
        return sessionId;
    }


    private Dictionary<string, object> CreateActiveSessionsEntry(ActiveSession activeSession, string sessionId)
    {
        return new Dictionary<string, object>
        {
            { "expireTime", activeSession.ExpireTime.ToString() },
            { "username", activeSession.Username },
            { "uniqueId", activeSession.UniqueId },
            { "ip", activeSession.Ip },
            { "sessionId", sessionId }
        };
    }


    private Task<UserToken> _getToken(User user, string sessionId, DateTime expirationDateTime)
    {
        return Task.FromResult(new UserToken(_tokenHelper.CreateToken(user, sessionId, expirationDateTime)));
    }
}