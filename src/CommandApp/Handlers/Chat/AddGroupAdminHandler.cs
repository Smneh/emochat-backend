using Confluent.Kafka;
using Contract.Commands.Group;
using Contract.DTOs.Chat;
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

public class AddGroupAdminHandler : IRequestHandler<AddGroupAdminCommand>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;
    
    public AddGroupAdminHandler(GroupRepository groupRepository, IdentityService identityService, IProducer<Null, string> producer)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
        _producer = producer;
    }

    public async Task<Unit> Handle(AddGroupAdminCommand command, CancellationToken cancellationToken)
    {
        var group = await GetGroupByGroupId(command.GroupId, cancellationToken);

        // Check Group Existence
        if (group == null)
            throw new AppException(command, Messages.NotFound, command.GroupId);

        // Check Member Existence
        if (!group.Members.Contains(command.member))
            throw new AppException(command, Messages.NotFound, command.member);
        
        // Check IsAdmin
        if (group.Admins.Contains(command.member))
            return Unit.Value;

        // Check Access
        if (group.Creator != _identityService.Username)
            throw new AppException(command, Messages.AccessError);
        
        // Add member to Group
        group.Admins.Add(command.member);
        await _groupRepository.AddGroupAdmin(command.GroupId, group.Admins, cancellationToken);

        // Push Add New Admin Event
        await _push(command);

        return Unit.Value;
    }

    private async Task _push(AddGroupAdminCommand command)
    {
        var KafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,
                Data = new GroupAdminAddedEvent
                {
                    GroupId = command.GroupId,
                    Member = command.member
                }
            },
            EventType = ChatEvent.GroupAdminAdded
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(KafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }

    private async Task<GetGroupByIdResponseDto?> GetGroupByGroupId(string groupId, CancellationToken cancellationToken)
    {
        return await _groupRepository.GetGroupByGroupId(groupId, cancellationToken);
    }
}