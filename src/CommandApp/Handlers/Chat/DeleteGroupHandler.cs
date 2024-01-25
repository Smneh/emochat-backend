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

public class DeleteGroupHandler : IRequestHandler<DeleteGroupCommand, DeleteGroupResponseDto>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;

    public DeleteGroupHandler(GroupRepository groupRepository, IdentityService identityService, IProducer<Null, string> producer)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
        _producer = producer;
    }

    public async Task<DeleteGroupResponseDto> Handle(DeleteGroupCommand command, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupByGroupId(command.GroupId, cancellationToken);

        if (group == null)
            throw new AppException(command, Messages.NotFound, command.GroupId);

        // Check Access
        if (group.Creator != _identityService.Username)
            throw new AppException(group, Messages.AccessError);

        // Delete Group From ElasticSearch
        await _groupRepository.DeleteGroupById(command.GroupId, cancellationToken);

        // Delete Group User For All Members From Elastic
        await _groupRepository.DeleteGroupUserById(command.GroupId, cancellationToken);

        // Delete Group Messages From Elastic
        await _groupRepository.DeleteGroupMessages(command.GroupId, cancellationToken);


        // Push GroupDeleted Event
        await _push(command);

        return new DeleteGroupResponseDto
        {
            GroupId = command.GroupId
        };
    }


    private async Task _push(DeleteGroupCommand command)
    {
        var KafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event {Username = _identityService.Username,  Data = new GroupDeletedEvent {GroupId = command.GroupId,}},
            EventType = ChatEvent.GroupDeleted
        };

        var message = new Message<Null, string> {Value = JsonConvert.SerializeObject(KafkaEvent)};
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }
}