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

public class UpdateGroupHandler : IRequestHandler<UpdateGroupCommand>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;
    
    public UpdateGroupHandler(IProducer<Null, string> producer, IdentityService identityService, GroupRepository groupRepository)
    {
        _producer = producer;
        _identityService = identityService;
        _groupRepository = groupRepository;
    }
    
    public async Task<Unit> Handle(UpdateGroupCommand command, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupByGroupId(command.ReceiverId, cancellationToken);

        if (group == null)
            throw new AppException(command, Messages.NotFound, command.ReceiverId);

        // Check Access
        if (group.Creator != _identityService.Username && group.Admins.All(x => x != _identityService.Username))
            throw new AppException(group, Messages.AccessError);

        // Update Group In Elastic
        await _groupRepository.UpdateGroup(command, cancellationToken);

        // Update Group User In Elastic
        await _groupRepository.UpdateGroupUserInfo(command, cancellationToken);

        // Push Update Group Event
        await PushUpdateGroupEvent(command);

        return Unit.Value;
    }
    
    private async Task PushUpdateGroupEvent(UpdateGroupCommand command)
    {
        var kafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,
                
                Data = new GroupUpdatedEvent
                {
                    ReceiverId = command.ReceiverId,
                    Description = command.Description,
                    Title = command.Title,
                    ProfilePictureId = command.ProfilePictureId
                }
            },
            EventType = ChatEvent.GroupUpdated
        };

        var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(kafkaEvent) };
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }
}