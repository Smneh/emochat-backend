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

public class AddGroupMemberHandler : IRequestHandler<AddGroupMemberCommand>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;

    public AddGroupMemberHandler(GroupRepository groupRepository, IdentityService identityService, IProducer<Null, string> producer)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
        _producer = producer;
    }

    public async Task<Unit> Handle(AddGroupMemberCommand command, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupByGroupId(command.GroupId, cancellationToken);

        // Check Group Existence
        if (group == null)
            throw new AppException(command, Messages.NotFound, command.GroupId);

        // Check Access
        if (group.Creator != _identityService.Username && group.Admins.All(x => x != _identityService.Username))
            throw new AppException(group, Messages.AccessError);

        // Check Member Join
        if (group.Members.Contains(command.member))
            return Unit.Value;
        
        // Add member to Group In Elastic
        group.Members.Add(command.member);
        await _groupRepository.AddGroupMember(command.GroupId, group.Members, cancellationToken);

        // Register Group For All Members
        await RegisterGroupForNewMember(command.GroupId, command.member, cancellationToken);

        // Push Add Group Member Event
        await _push(command);

        return Unit.Value;
    }


    private async Task _push(AddGroupMemberCommand command)
    {
        var kafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,  Data = new GroupMemberAddedEvent {GroupId = command.GroupId, Member = command.member}
            },
            EventType = ChatEvent.GroupMemberAdded
        };

        var message = new Message<Null, string> {Value = JsonConvert.SerializeObject(kafkaEvent)};
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }

    private async Task RegisterGroupForNewMember(string groupId, string newMember, CancellationToken cancellationToken)
    {
        // Get GroupUser
        var groupUser = await _groupRepository.GetGroupUserByGroupId(_identityService.Username, groupId, cancellationToken);
        groupUser!.Username = newMember;
        // Add groupUser for new Member
        await _groupRepository.CreateGroupUser(groupUser, cancellationToken);
    }
}