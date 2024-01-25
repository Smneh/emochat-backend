namespace Core.Settings;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string GroupId { get; set; } = default!;
}