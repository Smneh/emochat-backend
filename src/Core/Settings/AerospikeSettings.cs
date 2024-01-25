namespace Core.Settings;

public class AerospikeSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string DefaultNamespace { get; set; }
    public string SetName { get; set; }
}