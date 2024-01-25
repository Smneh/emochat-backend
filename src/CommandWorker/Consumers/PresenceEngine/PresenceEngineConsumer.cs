using Confluent.Kafka;
using Contract.Commands.PresenceEngine;
using Contract.DTOs.PresenceEngine;
using Core.Settings;
using MediatR;
using Newtonsoft.Json;
using Repository.AeroSpike;

namespace CommandWorker.Consumers.PresenceEngine;

public class PresenceEngineConsumer : BackgroundService
{
    private readonly ConsumerConfig _kafkaConfig;
    private readonly AerospikeRepository _aerospikeRepository;
    private readonly IMediator _mediator;
    private readonly string _presenceEngineSubscribeId;

    public PresenceEngineConsumer(AerospikeRepository aerospikeRepository, IMediator mediator)
    {
        _aerospikeRepository = aerospikeRepository;

        const string presenceEngineId = "PresenceEngineHandler";
        _presenceEngineSubscribeId = "PresenceEngine";

        _kafkaConfig = new ConsumerConfig
        {
            BootstrapServers = Settings.AllSettings.KafkaSettings.BootstrapServers,
            GroupId = presenceEngineId,
            ClientId = presenceEngineId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        _mediator = mediator;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() => ConsumeEvents(stoppingToken), stoppingToken);
    }

    private async Task ConsumeEvents(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_kafkaConfig).Build();
        consumer.Subscribe(_presenceEngineSubscribeId);

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
        var activeSessions = await _aerospikeRepository.GetUserActiveSessions(data.Receivers);

        var usersInfo = await _aerospikeRepository.GetOnlineUsers(data.Receivers);

        if (usersInfo.Count > 0)
        {
            await _mediator.Publish(
                new SignalrCommand
                {
                    Message = data.Message, Method = data.Method, Receivers = usersInfo,
                    ActiveSessions = activeSessions
                }, stoppingToken);
        }
    }
}