using Confluent.Kafka;
using Contract.DTOs.KafkaEvents;
using Contract.Enums;
using Core.Enums;
using Core.Exceptions;
using Core.Settings;
using Newtonsoft.Json;
using Repository.AeroSpike;
using CallStatus = Contract.Enums.CallStatus;

namespace CommandWorker.Consumers.Chat;

public class PresenceEngineGroupConsumer : BackgroundService
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly AerospikeRepository _aerospikeRepository;


    public PresenceEngineGroupConsumer(AerospikeRepository aerospikeRepository)
    {
        _aerospikeRepository = aerospikeRepository;

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = Settings.AllSettings.KafkaSettings.BootstrapServers,
            GroupId = Settings.AllSettings.KafkaSettings.GroupId,
            //ClientId = Settings.AllSettings.KafkaSettings.ClientId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
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

                switch (data?.EventType)
                {
                    case PresenceEngineEvent.UserStatus:
                        var status = (string) data.Event.Data.Status;
                        if (status == "offline")
                            await CheckInCallWithOnDisconnectedStatus(stoppingToken, data);

                        break;
                }
            }
            catch (OperationCanceledException exception)
            {
                throw new AppException(exception, Messages.OperationCanceledException, "وضعیت افلاین کاربر");
            }
            catch (Exception exception)
            {
                throw new AppException(exception, Messages.Exception);
            }
        }
    }

    private async Task CheckInCallWithOnDisconnectedStatus(CancellationToken stoppingToken, KafkaEvent<PresenceEngineEvent> data)
    {
        var userInfo = await _aerospikeRepository.GetUserFromAerospike((string) data.Event.Data.Username);
        var record = userInfo?.bins.ToDictionary(bin => bin.Key, bin => bin.Value) ?? new Dictionary<string, object>();
        
        var inCallWith = RetrieveInCallWith(record);

        if (inCallWith.Count == 2)
        {
            await _aerospikeRepository.UpdateUsersCallStatus(CallStatus.Available, inCallWith);
        }
    }

    private List<string> RetrieveInCallWith(IReadOnlyDictionary<string, object> record)
    {
        var inCallWithUsernames = new List<string>();
        if (!record.ContainsKey("inCallWith")) return inCallWithUsernames;

        var inCallWithUsernamesObject = (List<object>) record["inCallWith"];

        // Convert each object in the list to a Dictionary<string, string>
        for (var index = 0; index < inCallWithUsernamesObject.Count; index++)
        {
            var username = inCallWithUsernamesObject[index];
            inCallWithUsernames.Add((string)username);
        }

        return inCallWithUsernames;
    }
    
}