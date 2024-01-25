using Confluent.Kafka;
using Contract.Commands.PresenceEngine;
using Contract.DTOs.PresenceEngine;
using Core.Settings;
using MediatR;
using Newtonsoft.Json;
using Repository.AeroSpike;

namespace CommandWorker.Consumers.PresenceEngine;

public class PresenceEngineUserStatusConsumer : BackgroundService
{
    private readonly ConsumerConfig _kafkaConfig;
    private readonly AerospikeRepository _aerospikeRepository;
    private readonly ISender _sender;
    private readonly string _presenceEngineUserStatusSubscribeId;

    public PresenceEngineUserStatusConsumer(AerospikeRepository aerospikeRepository, ISender sender)
    {
        _aerospikeRepository = aerospikeRepository;

        var presenceEngineUserStatusId = "PresenceEngineUserStatusHandler";
        _presenceEngineUserStatusSubscribeId = "PresenceEngineUserStatus";

        _kafkaConfig = new ConsumerConfig
        {
            BootstrapServers = Settings.AllSettings.KafkaSettings.BootstrapServers,
            GroupId = presenceEngineUserStatusId,
            ClientId = presenceEngineUserStatusId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        _sender = sender;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() => ConsumeEvents(stoppingToken), stoppingToken);
    }

    private async Task ConsumeEvents(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_kafkaConfig).Build();
        consumer.Subscribe(_presenceEngineUserStatusSubscribeId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var peMessage = JsonConvert.DeserializeObject<PeMessage>(result.Message.Value);
                if (peMessage != null) await PublishMessage(peMessage, stoppingToken);
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

    private async Task PublishMessage(PeMessage data, CancellationToken stoppingToken)
    {
        var usersInfo = await _aerospikeRepository.GetUsers(data.Receivers);
        if (usersInfo.Count > 0)
        {
            await _sender.Send(
                new SignalrUsersStatus
                    { Message = data.Message, Method = data.Method, Sender = data.Sender, UsersInfo = usersInfo },
                stoppingToken);
        }
    }
}