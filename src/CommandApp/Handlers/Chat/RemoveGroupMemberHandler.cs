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

public class RemoveGroupMemberHandler : IRequestHandler<RemoveGroupMemberCommand>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;

    public RemoveGroupMemberHandler(IProducer<Null, string> producer, IdentityService identityService, GroupRepository groupRepository)
    {
        _producer = producer;
        _identityService = identityService;
        _groupRepository = groupRepository;
    }

    public async Task<Unit> Handle(RemoveGroupMemberCommand command, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupByGroupId(command.GroupId, cancellationToken);

        // Check Group Existence
        if (group == null)
            throw new AppException(command, Messages.NotFound, command.GroupId);

        if (!group.Members.Contains(command.member))
            throw new AppException(command, Messages.NotFound, command.member);

        // Check Creator
        if (group.Creator == command.member)
            throw new AppException(group, Messages.GroupCreatorCanNotBeRemoved);

        // Left Group Member
        if (group.Creator != _identityService.Username)
            await LeftGroupMember(command, group, cancellationToken);

        // Remove Group Member
        if (group.Creator == _identityService.Username)
            await RemoveGroupMember(command, group, cancellationToken);

        return Unit.Value;
    }

    public async Task RemoveGroupMember(RemoveGroupMemberCommand command, GetGroupByIdResponseDto group, CancellationToken cancellationToken)
    {
        // Check Access
        if (group.Creator != _identityService.Username && group.Admins.All(x => x != _identityService.Username))
            throw new AppException(group, Messages.AccessError);

        group.Members.Remove(command.member);
        await _groupRepository.RemoveGroupMember(command.GroupId, group.Members, cancellationToken);

        // Remove Member From GroupUser
        await _groupRepository.RemoveGroupUser(command.member, command.GroupId, cancellationToken);
        

        // Remove Member From Admins
        if (group.Admins.Contains(command.member))
        {
            group.Admins.Remove(command.member);
            await _groupRepository.RemoveGroupAdmin(command.GroupId, group.Admins, cancellationToken);
        }

        // Push GroupMember Removed Event
        await PushRemoveGroupMemberEvent(command);
    }


    public async Task LeftGroupMember(RemoveGroupMemberCommand command, GetGroupByIdResponseDto group, CancellationToken cancellationToken)
    {
        // Remove member From Group
        group.Members.Remove(command.member);
        await _groupRepository.RemoveGroupMember(command.GroupId, group.Members, cancellationToken);

        // Remove Member From GroupUser
        await _groupRepository.RemoveGroupUser(command.member,command.GroupId, cancellationToken);
        
        // Remove Member From Admins
        if (group.Admins.Contains(command.member))
        {
            group.Admins.Remove(command.member);
            await _groupRepository.RemoveGroupAdmin(command.GroupId, group.Admins, cancellationToken);
        }

        // Push GroupMember Removed Event
        await PushLeftGroupMemberEvent(command);
    }

    private async Task PushRemoveGroupMemberEvent(RemoveGroupMemberCommand command)
    {
        var KafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,  Data = new GroupMemberRemovedEvent { GroupId = command.GroupId, Member = command.member }
            },
            EventType = ChatEvent.GroupMemberRemoved
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(KafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }

    private async Task PushLeftGroupMemberEvent(RemoveGroupMemberCommand command)
    {
        var KafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,  Data = new LeftGroupMemberEvent { GroupId = command.GroupId, Member = command.member }
            },
            EventType = ChatEvent.GroupMemberLeft
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(KafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }
}