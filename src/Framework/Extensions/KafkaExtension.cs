using Confluent.Kafka;
using Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Extensions;

public static class KafkaExtension
{
    private static readonly ProducerConfig ProducerConfig = new()
    {
        BootstrapServers = Settings.AllSettings.KafkaSettings.BootstrapServers,
        ClientId = Settings.AllSettings.KafkaSettings.ClientId,
        MessageSendMaxRetries = 3,
        MessageTimeoutMs = 5000,
        CompressionType = CompressionType.Lz4,
        EnableIdempotence = false
    };

    public static void AddKafkaProducer(this IServiceCollection services)
    {
        // CheckHealthAsync();

        var producer = new ProducerBuilder<Null, string>(ProducerConfig).Build();

        services.AddSingleton(producer);
    }

    private static void CheckHealthAsync()
    {
        using var p = new ProducerBuilder<Null, string>(ProducerConfig).Build();
        
        var result = p.ProduceAsync("KafkaHealthCheck", new Message<Null, string> { Value = $"Kafka is healthy on {DateTime.UtcNow}" }).Result;
        
        if (result.Status == PersistenceStatus.NotPersisted)
            throw new Exception("Kafka is down!");
    }
}