namespace Core.Settings;

public class Settings : ISettings
{
    public static Settings AllSettings { get; private set; }
    public KafkaSettings KafkaSettings { get; set; }
    public ElasticSettings ElasticSettings { get; set; }
    public AerospikeSettings AerospikeSettings { get; set; }
    public JwtSettings JwtSettings { get; set; }
    public RedisSettings RedisSettings { get; set; }
    public MinioSettings MinioSettings { get; set; }
    public long DefaultMaxFileSizeInMB { get; set; }

    public static void Set(Settings settings)
    {
        AllSettings = settings;
    }
}