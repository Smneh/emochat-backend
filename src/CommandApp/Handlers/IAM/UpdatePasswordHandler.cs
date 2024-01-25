using Aerospike.Client;
using Contract.Commands.IAM;
using Contract.DTOs.IAM;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using MediatR;
using Repository.AeroSpike;
using Services.Services;

namespace CommandApp.Handlers.IAM;

public class UpdatePasswordHandler : IRequestHandler<UpdatePasswordCommand, UpdatePasswordResult>
{
    private readonly UserRepository _userRepository;
    private readonly SecurityService _securityService;
    private readonly IdentityService _identityService;

    public UpdatePasswordHandler(UserRepository userRepository, SecurityService securityService, IdentityService identityService)
    {
        _userRepository = userRepository;
        _securityService = securityService;
        _identityService = identityService;
    }

    public async Task<UpdatePasswordResult> Handle(UpdatePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var result = new UpdatePasswordResult();

        var user = await _userRepository.GetUserMainInfo(_identityService.Username);

        if (user == null)
            throw new AppException(Messages.WrongUsername, _identityService.Username);
        
        if (!_securityService.VerifyPassword(request.Password,user.Password))
            throw new AppException(Messages.WrongPassword);

        var newPassword = _securityService.GenerateHash(request.NewPassword);
        var lastModifyDate = DateTime.Now;

        var key = new Key("IAM", "Users", user.Username);

        var binsList = new List<Bin>
        {
            new("password", newPassword),
            new("lastModifyDate", lastModifyDate)
        };

        await _userRepository.UpdateUser(binsList, key);

        result.Description = "Updated";

        return result;
    }
}