using Confluent.Kafka;
using Contract.Commands.PresenceEngine;
using Contract.DTOs.KafkaEvents;
using Contract.Enums;
using Contract.Events;
using MediatR;
using Newtonsoft.Json;

namespace CommandWorker.Handlers;

public class SendUserStatusHandler : IRequestHandler<SendUserStatusCommand>
{
    private readonly IProducer<Null, string> _producer;

    public SendUserStatusHandler(IProducer<Null, string> producer)
    {
        _producer = producer;
    }

    public async Task<Unit> Handle(SendUserStatusCommand request, CancellationToken cancellationToken)
    {
        await _push(request);
        return Unit.Value;
    }

    private async Task _push(SendUserStatusCommand command)
    {
        var newEvent = new KafkaEvent<PresenceEngineEvent>
        {
            Event = new Event
            {
                Username = command.Username,
                Data = new OnlineStatusEvent
                {
                    Username = command.Username,
                    Status = command.Status,
                    LastActionTime = command.LastActionTime
                }
            },
            EventType = PresenceEngineEvent.UserStatus 
        };

        var message = new Message<Null, string> {Value = JsonConvert.SerializeObject(newEvent)};
        await _producer.ProduceAsync(nameof(PresenceEngineEvent), message);
    }
}