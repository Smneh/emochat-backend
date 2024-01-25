using Confluent.Kafka;
using Contract.Commands.Notification;
using Contract.DTOs.KafkaEvents;
using Contract.DTOs.PresenceEngine;
using Contract.Enums;
using Contract.Events;
using Contract.Queries.Chat;
using Core.Extensions;
using Core.Settings;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommandWorker.Consumers.Notification;

public class PresenceEngineUserStatusConsumer : BackgroundService
{
    private readonly IProducer<Null, string> _producer;
    private readonly ConsumerConfig _consumerConfig;
    private readonly string _presenceEngineUserStatusId;
    private readonly ILogger<PresenceEngineUserStatusConsumer> _logger;
    private readonly ISender _sender;

    public PresenceEngineUserStatusConsumer(IProducer<Null, string> producer, ILogger<PresenceEngineUserStatusConsumer> logger, ISender sender)
    {
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = Settings.AllSettings.KafkaSettings.BootstrapServers,
            GroupId = "Notification",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        _producer = producer;
        _presenceEngineUserStatusId = "PresenceEngineUserStatus";
        _logger = logger;
        _sender = sender;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() => ConsumePresenceEngineEvents(stoppingToken), stoppingToken);
        return Task.CompletedTask;
    }

    private async Task ConsumePresenceEngineEvents(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(nameof(PresenceEngineEvent));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var data = JsonConvert.DeserializeObject<KafkaEvent<PresenceEngineEvent>>(result.Message.Value);

                var getUserCommunicatedUsernames = new GetUserCommunicatedUsernamesQuery
                {
                    Username = data.Event.Username,
                };
                var response = await _sender.Send(getUserCommunicatedUsernames, stoppingToken);
                var receivers = response!.Select(u => u.ReceiverId).ToList();
                receivers.Add(data.Event.Username);

                var command = new RegisterNotificationCommand
                {
                    Content = (string) data.Event.Data.Content,
                    Sender = data.Event.Username,
                    Receivers = receivers,
                    MetaInfo = "",
                    Type = nameof(PresenceEngineEvent.UserStatus),
                    Category = nameof(ChatEvent),
                    Username = data.Event.Username,
                    Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
                };
                await _Push(command);
                await _PushNotification(command);
            }
            catch (ConsumeException exception)
            {
                Console.WriteLine(exception);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An application exception occurred." + "{Path} {CustomObject}",
                    nameof(PresenceEngineUserStatusConsumer),
                    JsonConvert.SerializeObject(exception)
                );
                Console.WriteLine(exception);
            }
        }
    }


    private async Task _Push(RegisterNotificationCommand command)
    {
        var KafkaEvent = new KafkaEvent<NotificationEvent>
        {
            Event = new Event
            {
                Username = command.Username,
                DateTime = DateTime.Now,
                Data = new RegisterNotificationEvent
                {
                    Content = command.Content,
                    Sender = command.Sender,
                    Receivers = command.Receivers,
                    MetaInfo = command.MetaInfo,
                    Type = command.Type,
                    Category = command.Category
                }
            },
            EventType = NotificationEvent.PresenceEngineUserStatusNotificationCreated
        };
        var message = new Message<Null, string> {Value = JsonConvert.SerializeObject(KafkaEvent)};
        await _producer.ProduceAsync(nameof(NotificationEvent), message);
    }

    private async Task _PushNotification(RegisterNotificationCommand command)
    {
        var pushNotification = new PeMessage
        {
            Method = command.Type,
            Message = command.Data,
            Sender = command.Sender,
            Receivers = command.Receivers,
        };
        var message = new Message<Null, string> {Value = JsonConvert.SerializeObject(pushNotification)};
        //todo: the topic name should not be configurable in this project, it should comes from PresenceEngine's contract
        await _producer.ProduceAsync(_presenceEngineUserStatusId, message);
    }
}