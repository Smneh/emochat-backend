using Aerospike.Client;
using Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Extensions;

public static class AerospikeExtension
{
    public static void AddAerospikeClient(this IServiceCollection services)
    {
        var clientPolicy = new ClientPolicy
        {
            timeout = 5000
        };
        var client = new AerospikeClient(clientPolicy, Settings.AllSettings.AerospikeSettings.Host, Settings.AllSettings.AerospikeSettings.Port);

        client.CreateIndex(new Policy(), "IAM", "Profiles", "profileIndex", "profile", IndexType.STRING, IndexCollectionType.MAPVALUES);

        services.AddSingleton<IAerospikeClient>(client);
    }
}