using Confluent.Kafka;
using Contract.DTOs.KafkaEvents;
using Contract.Enums;
using Core.Settings;
using Entities.Models.Chat;
using Newtonsoft.Json;
using Repository.Group;

namespace CommandWorker.Consumers.Chat;

public class ChatConsumer : BackgroundService
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly GroupRepository _groupRepository;


    public ChatConsumer(GroupRepository groupRepository)
    {
        _groupRepository = groupRepository;

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = Settings.AllSettings.KafkaSettings.BootstrapServers,
            GroupId = Settings.AllSettings.KafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() => GroupEventConsumer(stoppingToken), stoppingToken);
    }

    private async Task GroupEventConsumer(CancellationToken stoppingToken)
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
                        await RegisterMessage(data, stoppingToken);
                        break;
                }
            }
            catch (ConsumeException exception)
            {
                Console.WriteLine(exception);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }

    private async Task RegisterMessage(KafkaEvent<ChatEvent> groupData, CancellationToken stoppingToken)
    {
        var jsonData = JsonConvert.SerializeObject(groupData.Event.Data);
        var Message = JsonConvert.DeserializeObject<Message>(jsonData);
        await _groupRepository.RegisterMessageInChat(Message, stoppingToken);
        await _groupRepository.UpdateGroupUser(Message, groupData.Event.Username, false, Message.MessageId, stoppingToken);
    }
}