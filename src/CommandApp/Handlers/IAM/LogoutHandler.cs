using Aerospike.Client;
using Contract.Commands.IAM;
using Core.Jwt;
using MediatR;
using Repository.AeroSpike;

namespace CommandApp.Handlers.IAM;

public class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly ITokenHelper _tokenHelper;
    private readonly UserRepository _userRepository;
    public LogoutHandler(ITokenHelper tokenHelper, UserRepository userRepository)
    {
        _tokenHelper = tokenHelper;
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var username = _tokenHelper.CurrentUser.Username;
        var sessionId = _tokenHelper.CurrentUser.SessionId;
        await KillActiveSessionAerospike(username, sessionId);
        return Unit.Value;
    }

    private async Task KillActiveSessionAerospike(string username, string sessionId)
    {
        // Create the unique key for the session that you want to remove
        var key = new Key("IAM", "Sessions", $"{username}_{sessionId}");

        // Create a WritePolicy
        var policy = new WritePolicy
        {
            recordExistsAction = RecordExistsAction.UPDATE,
        };

        // Delete the record
        await _userRepository.DeleteUserSession(key, policy);
    }
    
}