using Confluent.Kafka;
using Contract.Commands.Group;
using Contract.DTOs.KafkaEvents;
using Contract.Enums;
using Contract.Events;
using Contract.Queries.Profile;
using Core.Services;
using Entities.Models.Chat;
using MediatR;
using Newtonsoft.Json;
using Repository.Group;

namespace CommandApp.Handlers.Chat;

public class CreateGroupHandler : IRequestHandler<CreateGroupCommand>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;
    private readonly ISender _sender;
    
    public CreateGroupHandler(IProducer<Null, string> producer, IdentityService identityService, GroupRepository groupRepository, ISender sender)
    {
        _producer = producer;
        _identityService = identityService;
        _groupRepository = groupRepository;
        _sender = sender;
    }

    public async Task<Unit> Handle(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        var groupId = Guid.NewGuid().ToString();
        var guid = Guid.NewGuid().ToString();

        // Register  Group in ElasticSearch
        var group = await RegisterGroup(command, guid, groupId, cancellationToken);

        // Register  Group User in ElasticSearch
        await RegisterGroupUser(group, cancellationToken);

        // Register First  Message
        await RegisterMessage(groupId);

        // Push  Group Created Event
        await PushRegisterGroupEvent(command, groupId);

        return Unit.Value;
    }

    private async Task RegisterMessage(string groupId)
    {
        var creatorProfile = await _getProfileInfo(_identityService.Username);
        var command = new SendMessageCommand
        {
            Content = $"Created By {creatorProfile.Fullname}",
            Attachments = "-",
            ReceiverId = groupId,
            ReceiverType = "Group",
            ParentId = 0,
            MessageTypeId = 56,
            Self = true,
            RegDate = DateTime.Now.ToString(),
            RegTime = DateTime.Now.TimeOfDay.ToString(), //Todo : check if its okay
            IsFirst = true
        };

        await _sender.Send(command);
    }

    private async Task PushRegisterGroupEvent(CreateGroupCommand command, string groupId)
    {
        var kafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,
                Data = new GroupCreatedEvent
                {
                    GroupId = groupId,
                    Creator = _identityService.Username,
                    Description = command.Description,
                    RegDateTime = DateTime.Now,
                    ProfilePictureId = command.ProfilePictureId,
                    Members = new List<string> {_identityService.Username}
                },
                DateTime = DateTime.Now,
            },
            EventType = ChatEvent.GroupCreated,
        };

        var message = new Message<Null, string> {Value = JsonConvert.SerializeObject(kafkaEvent)};
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }

    private async Task<Group> RegisterGroup(CreateGroupCommand command, string guid, string groupId, CancellationToken cancellationToken)
    {
        var group = new Group
        {
            GroupId = groupId,
            Creator = _identityService.Username,
            Description = command.Description,
            Type = "Group",
            RegDateTime = DateTime.Now,
            ProfilePictureId = command.ProfilePictureId,
            Members = new List<string> {_identityService.Username},
            Admins = new List<string> {_identityService.Username},
            Title = command.Title,
            Guid = guid
        };

        if (command.Members.Count == 0)
            group.Members = new List<string> { _identityService.Username };
        else
        {
            command.Members.Add(_identityService.Username);
            group.Members = command.Members;
        }
        
        // Set Members Count
        group.MembersCount = group.Members.Count;

        await _groupRepository.RegisterGroup(group, cancellationToken);
        return group;
    }

    private async Task RegisterGroupUser(Group group, CancellationToken cancellationToken)
    {
        foreach (var member in group.Members)
        {
            var groupUser = new GroupUser
            {
                GroupId = group.GroupId,
                Creator = _identityService.Username,
                Title = group.Title,
                ProfilePictureId = group.ProfilePictureId,
                FirstUnreadMessageId = 0,
                LastMessageId = 0,
                UnReadMessages = new List<long>(),
                Type = "Group",
                Content = string.Empty,
                RegUser = _identityService.Username,
                IsSeen = false,
                RegDateTime = DateTime.Now,
                Username = member
            };

            await _groupRepository.RegisterGroupUser(groupUser, cancellationToken);
        }
    }

    private async Task<Entities.Models.Profile.Profile> _getProfileInfo(string username)
    {
        var query = new GetProfilesByUsernamesQuery
        {
            Usernames = new List<string>
            {
                username
            }
        };
        var userProfiles = await _sender.Send(query);
        return userProfiles.FirstOrDefault()!;
    }
}