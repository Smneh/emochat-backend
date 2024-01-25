using Confluent.Kafka;
using Contract.Commands.Notification;
using Contract.DTOs.KafkaEvents;
using Contract.DTOs.PresenceEngine;
using Contract.Enums;
using Contract.Events;
using Core.Extensions;
using Core.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NotificationEvent = Contract.Enums.NotificationEvent;

namespace CommandWorker.Consumers.Notification;

public class NotificationConsumer : BackgroundService
{
    private readonly IProducer<Null, string> _producer;
    private readonly ConsumerConfig _consumerConfig;
    private readonly ILogger<NotificationConsumer> _logger;
    private readonly string _presenceEngineId;

    public NotificationConsumer(IProducer<Null, string> producer, ILogger<NotificationConsumer> logger)
    {
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = Settings.AllSettings.KafkaSettings.BootstrapServers,
            GroupId = "Notification",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        _producer = producer;
        _logger = logger;
        _presenceEngineId = "PresenceEngine";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeGroupEvents(stoppingToken), stoppingToken);
    }

    private async Task ConsumeGroupEvents(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(nameof(ChatEvent));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var data = JsonConvert.DeserializeObject<KafkaEvent<ChatEvent>>(result.Message.Value);

                switch (data?.EventType)
                {
                    case ChatEvent.MessageSent:
                        await GroupMessageSent(stoppingToken, data);
                        break;
                    case ChatEvent.MessageDeleted:
                        await GroupContentDeleted(stoppingToken, data);
                        break;
                    case ChatEvent.GroupVisitorAdded:
                        await GroupVisitorAdded(stoppingToken, data);
                        break;
                    case ChatEvent.IncomingCall:
                        await IncomingCall(stoppingToken, data);
                        break;
                    case ChatEvent.RejectedCall:
                        await RejectedCall(stoppingToken, data);
                        break;
                    case ChatEvent.AcceptedCall:
                        await AcceptedCall(stoppingToken, data);
                        break;
                    case ChatEvent.EndedCall:
                        await EndedCall(stoppingToken, data);
                        break;
                    case ChatEvent.CanceledCall:
                        await CanceledCall(stoppingToken, data);
                        break;
                    case ChatEvent.BusyCall:
                        await BusyCall(stoppingToken, data);
                        break;
                }
            }
            catch (ConsumeException exception)
            {
                _logger.LogError(
                    exception,
                    "An application exception occurred." + "{Path} {CustomObject}",
                    nameof(NotificationConsumer),
                    JsonConvert.SerializeObject(exception)
                );
                Console.WriteLine(exception);
                Environment.Exit(0);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An application exception occurred." + "{Path} {CustomObject}",
                    nameof(NotificationConsumer),
                    JsonConvert.SerializeObject(exception)
                );
                Console.WriteLine(exception);
                Environment.Exit(0);
            }
        }
    }


    private async Task GroupMessageSent(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var receivers = (JArray) data.Event.Data.Members;
        var members = receivers.ToObject<List<string>>();
        members.Remove(data.Event.Username);

        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = members ?? new List<string>(),
            MetaInfo = "",
            Type = nameof(ChatEvent.MessageSent),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command,NotificationEvent.MessageSentNotificationCreated);
        await _PushNotification(command);
    }

    private async Task GroupContentDeleted(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var receivers = (JArray) data.Event.Data.Members;
        var members = receivers.ToObject<List<string>>();

        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = members ?? new List<string>(),
            MetaInfo = "",
            Type = nameof(ChatEvent.MessageDeleted),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command,NotificationEvent.ContentDeletedNotificationCreated);
        await _PushNotification(command);
    }

    private async Task GroupMessagesVisited(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var receivers = (JArray) data.Event.Data.Members;
        var members = receivers.ToObject<List<string>>();

        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = members ?? new List<string>(),
            MetaInfo = "",
            Type = nameof(ChatEvent.MessagesVisited),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command,NotificationEvent.MessagesVisitedNotificationCreated);
        await _PushNotification(command);
    }

    private async Task GroupVisitorAdded(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var receivers = (JArray) data.Event.Data.Members;
        var members = receivers.ToObject<List<string>>();

        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = members ?? new List<string>(),
            MetaInfo = "",
            Type = nameof(ChatEvent.GroupVisitorAdded),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command,NotificationEvent.GroupVisitorAddedNotificationCreated);
        await _PushNotification(command);
    }

    private async Task IncomingCall(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = new List<string> {(string) data.Event.Data.Receiver},
            MetaInfo = "",
            Type = nameof(ChatEvent.IncomingCall),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command,NotificationEvent.IncomingCallNotificationCreated);
        await _PushNotification(command);
    }

    private async Task RejectedCall(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = new List<string> {(string) data.Event.Data.Receiver,data.Event.Username},
            MetaInfo = "",
            Type = nameof(ChatEvent.RejectedCall),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command,NotificationEvent.RejectedCallNotificationCreated);
        await _PushNotification(command);
    }

    private async Task AcceptedCall(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = new List<string> {(string) data.Event.Data.Receiver},
            MetaInfo = "",
            Type = nameof(ChatEvent.AcceptedCall),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command,NotificationEvent.AcceptedCallNotificationCreated);
        await _PushNotification(command);
    }

    private async Task EndedCall(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = new List<string> {(string) data.Event.Data.Receiver},
            MetaInfo = "",
            Type = nameof(ChatEvent.EndedCall),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command,NotificationEvent.EndedCallNotificationCreated);
        await _PushNotification(command);
    }

    private async Task CanceledCall(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = new List<string> {(string) data.Event.Data.Receiver},
            MetaInfo = "",
            Type = nameof(ChatEvent.CanceledCall),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command, NotificationEvent.CanceledCallNotificationCreated);
        await _PushNotification(command);
    }

    private async Task BusyCall(CancellationToken stoppingToken, KafkaEvent<ChatEvent> data)
    {
        var command = new RegisterNotificationCommand
        {
            Content = (string) data.Event.Data.Content,
            Sender = data.Event.Username,
            Receivers = new List<string> {(string) data.Event.Data.Receiver},
            MetaInfo = "",
            Type = nameof(ChatEvent.BusyCall),
            Category = nameof(ChatEvent),
            Username = data.Event.Username,
            Data = JsonConvert.SerializeObject(((JObject) data.Event.Data).ToCamelCase()),
        };

        await _Push(command , NotificationEvent.BusyCallNotificationCreated);
        await _PushNotification(command);
    }


    private async Task _Push(RegisterNotificationCommand command , NotificationEvent notificationEvent)
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
            EventType = notificationEvent
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
        await _producer.ProduceAsync(_presenceEngineId, message);
    }
}