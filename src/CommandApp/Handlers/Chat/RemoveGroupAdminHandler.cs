using Confluent.Kafka;
using Contract.Commands.Group;
using Contract.DTOs.KafkaEvents;
using Contract.Enums;
using Contract.Events;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using MediatR;
using Newtonsoft.Json;
using Repository.Group;

namespace CommandApp.Handlers.Chat;

public class RemoveGroupAdminHandler : IRequestHandler<RemoveGroupAdminCommand>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;

    public RemoveGroupAdminHandler(IProducer<Null, string> producer, IdentityService identityService, GroupRepository groupRepository)
    {
        _producer = producer;
        _identityService = identityService;
        _groupRepository = groupRepository;
    }

    public async Task<Unit> Handle(RemoveGroupAdminCommand command, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupByGroupId(command.GroupId, cancellationToken);

        // Check Group Existence
        if (group == null)
            throw new AppException(command, Messages.NotFound, command.GroupId);

        // Check Member Existence
        if (!group.Admins.Contains(command.member))
            throw new AppException(command, Messages.NotFound, command.member);

        // Check Access
        if (group.Creator != _identityService.Username)
            throw new AppException(group, Messages.AccessError);
        
        if (group.Creator == command.member)
            throw new AppException(group, Messages.AccessError);
        
        // Remove Group Member
        group.Admins.Remove(command.member);
        await _groupRepository.RemoveGroupAdmin(command.GroupId, group.Admins, cancellationToken);

        // Push Remove Admin Event
        await _push(command);

        return Unit.Value;
    }
    
    private async Task _push(RemoveGroupAdminCommand command)
    {
        var KafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,  Data = new GroupAdminRemovedEvent { GroupId = command.GroupId, Member = command.member }
            },
            EventType = ChatEvent.GroupAdminRemoved
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(KafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }
}