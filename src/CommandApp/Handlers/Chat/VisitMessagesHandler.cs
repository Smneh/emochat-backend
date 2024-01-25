using Confluent.Kafka;
using Contract.Commands.Group;
using Contract.DTOs.Chat;
using Contract.DTOs.KafkaEvents;
using Contract.Enums;
using Contract.Events;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using Entities.Models.Chat;
using MediatR;
using Newtonsoft.Json;
using Repository.Group;

namespace CommandApp.Handlers.Chat;

public class VisitMessagesHandler : IRequestHandler<VisitMessagesCommand>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;

    public VisitMessagesHandler(IProducer<Null, string> producer, IdentityService identityService, GroupRepository groupRepository)
    {
        _producer = producer;
        _identityService = identityService;
        _groupRepository = groupRepository;
    }

    public async Task<Unit> Handle(VisitMessagesCommand command, CancellationToken cancellationToken)
    {
        var groupUser = await _groupRepository.GetGroupUserByGroupId( _identityService.Username , command.GroupId, cancellationToken);

        if (groupUser == null)
            throw new AppException(groupUser, Messages.NotFound, command.GroupId);
        
        // Visit All Messages in ElasticSearch
        await _groupRepository.VisitAllMessages(_identityService.Username,  command.GroupId, command.MessageId, cancellationToken);

        // Add User to Visitors
        await RegisterNewVisitor(groupUser, command.MessageId, command.GroupId, cancellationToken);

        // Update Group User Seen
        await UpdateGroupUserSeen(command.GroupId, groupUser.ReceiverId, command.MessageId, cancellationToken);

        // Push Messages Visited Event
        await PushVisitAllMessagesEvent(command);

        return Unit.Value;
    }

    private async Task RegisterNewVisitor(GroupUser groupUser, long maxMessageId, string groupId, CancellationToken cancellationToken)
    {
        var groupVisitor = new GroupVisitorDto { Username = _identityService.Username, DateTime = DateTime.Now };

        var messageIds = groupUser.UnReadMessages.Where(messageId => messageId <= maxMessageId).ToList();
        await _groupRepository.RegisterNewGroupVisitor(messageIds, _identityService.Username, groupVisitor, cancellationToken);

        // Push Register New Visitor Event
        var group = await _groupRepository.GetGroupByGroupId(groupId, cancellationToken);
        await PushRegisterNewVisitor(groupVisitor, groupId, messageIds, group.Members);
    }

    public async Task UpdateGroupUserSeen(string groupId, string receiverId, long messageId, CancellationToken cancellationToken)
    {
        await _groupRepository.UpdateGroupUserSeen( receiverId, true, groupId, cancellationToken);

        // push Group User Seen Updated Event
        await PushUpdateGroupUserSeen(groupId, messageId);
    }

    private async Task PushUpdateGroupUserSeen(string groupId, long lastMessageId)
    {
        var kafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event { Username = _identityService.Username,  Data = new GroupUserSeenUpdatedEvent { GroupId = groupId, LastMessageId = lastMessageId } },
            EventType = ChatEvent.GroupUserSeenUpdated
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(kafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }

    private async Task PushRegisterNewVisitor(GroupVisitorDto groupVisitorDto, string groupId, List<long> messageIds, List<string> members)
    {
        var kafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,
                
                Data = new VisitorAddedEvent { GroupId = groupId, Visitor = groupVisitorDto, MessageIds = messageIds, Members = members }
            },
            EventType = ChatEvent.GroupVisitorAdded
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(kafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }

    private async Task PushVisitAllMessagesEvent(VisitMessagesCommand command)
    {
        var kafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event { Username = _identityService.Username,  Data = new MessagesVisitedEvent { GroupId = command.GroupId, MessageId = command.MessageId } },
            EventType = ChatEvent.MessagesVisited
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(kafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }
}