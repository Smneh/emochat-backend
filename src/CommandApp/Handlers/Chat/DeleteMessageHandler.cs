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

public class DeleteMessageHandler : IRequestHandler<DeleteMessageCommand>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;

    public DeleteMessageHandler(GroupRepository groupRepository, IdentityService identityService, IProducer<Null, string> producer)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
        _producer = producer;
    }

    public async Task<Unit> Handle(DeleteMessageCommand command, CancellationToken cancellationToken)
    {
        var messages = await _groupRepository.GetGroupMessageById(command.MessageId, cancellationToken);
        var message = messages.FirstOrDefault();

        var group = await _groupRepository.GetGroupByGroupId(command.GroupId, cancellationToken);

        if (message == null)
            throw new AppException(command, Messages.NotFound, command.MessageId);
        if (group == null)
            throw new AppException(Messages.GroupNotFound);

        if (message.RegUser != _identityService.Username)
            throw new AppException(message, Messages.AccessError);

        // Check Member Join
        if (!group.Members.Contains(_identityService.Username))
            throw new AppException(message, Messages.AccessError);

        // Delete Content From Messages
        await _groupRepository.DeleteMessageContent(command.MessageId, cancellationToken);

        // Delete Content From Group User
        await DeleteMessageContentFromGroupUser(command, cancellationToken);

        // Push
        await PushDeleteContentEvent(command, group.Members);

        return Unit.Value;
    }

    private async Task DeleteMessageContentFromGroupUser(DeleteMessageCommand command, CancellationToken cancellationToken)
    {
        var groupUser = await _groupRepository.GetGroupUserByGroupId(_identityService.Username, command.GroupId, cancellationToken);
        if (groupUser == null || groupUser.LastMessageId != command.MessageId)
            return;

        var latestMessage = await _groupRepository.GetLatestMessage(command.GroupId);
        await _groupRepository.UpdateDeletedMessageInGroupUser(command.GroupId, latestMessage?.RegDateTime ?? DateTime.MinValue,
            latestMessage?.Content ?? "", latestMessage?.MessageId ?? 0, latestMessage?.RegUser ?? "", cancellationToken);
    }

    private async Task PushDeleteContentEvent(DeleteMessageCommand command, List<string> members)
    {
        var kafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,

                Data = new ContentDeletedEvent { Members = members, GroupId = command.GroupId, MessageId = command.MessageId }
            },
            EventType = ChatEvent.MessageDeleted
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(kafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }
}