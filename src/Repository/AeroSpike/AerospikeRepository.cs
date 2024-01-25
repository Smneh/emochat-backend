using Aerospike.Client;
using Contract.Enums;
using Core.Interfaces;
using Core.Settings;

namespace Repository.AeroSpike;

public class AerospikeRepository : ISingletonDependency
{
    private readonly AerospikeClient _client;
    private readonly string _namespace;
    private readonly string _setName;
    private QueryPolicy? _policy;

    public AerospikeRepository()
    {
        var host = Settings.AllSettings.AerospikeSettings.Host;
        var port = Settings.AllSettings.AerospikeSettings.Port;

        var clientPolicy = new ClientPolicy
        {
            failIfNotConnected = true,
        };
        _client = new AerospikeClient(clientPolicy, host, port);
        _namespace = Settings.AllSettings.AerospikeSettings.DefaultNamespace;
        _setName = Settings.AllSettings.AerospikeSettings.SetName;

        UpdateBinForAllRecords();
        InitializeQueryPolicy();
    }

    private void InitializeQueryPolicy()
    {
        _policy = new QueryPolicy();
        var node = _client.Nodes[0];
        var response = Info.Request(node, "sindex");
        var indexExists = response.Contains("status_index");

        if (!indexExists)
        {
            _client.CreateIndex(_policy, _namespace, _setName, "status_index", "status", IndexType.STRING);
        }
    }

    public async Task WriteAsync(string set, string key, Dictionary<string, object> bins)
    {
        if (!IsConnected)
            return;

        var keyObject = new Key(_namespace, set, key);
        var writePolicy = new WritePolicy {sendKey = true};

        var binObjects = new List<Bin>();
        foreach (var bin in bins)
        {
            binObjects.Add(new Bin(bin.Key, bin.Value));
        }

        await Task.Run(() => _client.Put(writePolicy, keyObject, binObjects.ToArray()));
    }

    public async Task<Dictionary<string, object>> ReadAsync(string setName, string key)
    {
        if (!IsConnected)
            return new Dictionary<string, object>();

        var keyObject = new Key(_namespace, setName, key);
        var record = await Task.Run(() => _client.Get(null, keyObject));

        return record?.bins.ToDictionary(bin => bin.Key, bin => bin.Value) ?? new Dictionary<string, object>();
    }

    public async Task<List<Record>> GetOnlineUsers(List<string> usernames)
    {
        var stmt = new Statement();
        stmt.SetNamespace(_namespace);
        stmt.SetSetName(_setName);
        stmt.SetBinNames("username", "status", "connectionsInfo");

        var onlineUsers = new List<Record>();
        await Task.Run(() =>
        {
            foreach (var username in usernames)
            {
                if (_policy == null) continue;
                _policy.filterExp = Exp.Build(
                    Exp.Let(
                        Exp.Def("statusBin", Exp.StringBin("status")),
                        Exp.Def("usernameBin", Exp.StringBin("username")),
                        Exp.And(
                            Exp.EQ(Exp.Var("statusBin"), Exp.Val("online")),
                            Exp.EQ(Exp.Var("usernameBin"), Exp.Val(username))
                        )
                    )
                );

                var recordSet = _client.Query(_policy, stmt);
                while (recordSet.Next())
                {
                    onlineUsers.Add(recordSet.Record);
                }
            }
        });

        return onlineUsers;
    }
    
    public async Task<List<Record>> GetUsers(List<string> usernames)
    {
        var stmt = new Statement();
        stmt.SetNamespace(_namespace);
        stmt.SetSetName(_setName);

        var userInfos = new List<Record>();
        await Task.Run(() =>
        {
            foreach (var username in usernames)
            {
                if (_policy == null) continue;
                _policy.filterExp = Exp.Build(
                    Exp.EQ(Exp.StringBin("username"), Exp.Val(username))
                );
                var recordSet = _client.Query(_policy, stmt);
                while (recordSet.Next())
                {
                    userInfos.Add(recordSet.Record);
                }
            }
        });

        return userInfos;
    }
    
    public async Task<List<Dictionary<string, object>>> GetUserActiveSessions(List<string> usernames)
    {
        var allActiveSessions = new List<Dictionary<string, object>>();

        foreach (var username in usernames)
        {
            // Create a statement to query all records with the given username
            var stmt = new Statement();
            stmt.SetNamespace("IAM");
            stmt.SetSetName("Sessions");
            stmt.SetFilter(Filter.Equal("username", username));

            // Execute the query and store the result in a list
            var recordSet = _client.Query(null, stmt);
            var activeSessions = new List<Dictionary<string, object>>();

            while (recordSet.Next())
            {
                var record = recordSet.Record;
                var activeSessionObject = record.GetValue("activeSession");

                if (activeSessionObject is not Dictionary<object, object> activeSessionObj) continue;
                var activeSession = activeSessionObj.ToDictionary(k => k.Key.ToString(), v => v.Value);
                activeSessions.Add(activeSession);
            }

            allActiveSessions.AddRange(activeSessions);
        }

        return allActiveSessions;
    }
    
    private void DeleteAllDataInDataset()
    {
        try
        {
            _client.Truncate(new InfoPolicy(), _namespace, _setName, null);
        }
        catch (AerospikeException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    private void UpdateBinForAllRecords()
    {
        var policy = new ScanPolicy();
        _client.ScanAll(policy, _namespace, _setName, UpdateConnectionIds);
    }
    
    private void UpdateConnectionIds(Key key, Record record)
    {
        Dictionary<string, object> bins = new()
        {
            { "connectionsInfo", new List<Dictionary<string, string>>() },
            { "callStatus", (int)CallStatus.NotAvailable },
            { "inCallWith", new List<string>() }
        };
        var binObjects = bins.Select(bin => new Bin(bin.Key, bin.Value)).ToArray();
        _client.Put(null, key, binObjects);
    }

    public async Task UpdateUsersCallStatus(CallStatus status, List<string> usernames)
    {
        var bins = new Dictionary<string, object>
        {
            {"callStatus", (int) status},
            {"inCallWith", status == CallStatus.Available ? new List<string>() : usernames}
        };

        foreach (var username in usernames)
        {
            await WriteAsync(_setName, username, bins);
        }
    }

    public async Task<Record> GetUserFromAerospike(string receiver)
    {
        var stmt = new Statement();
        stmt.SetNamespace(_namespace);
        stmt.SetSetName(_setName);

        Record userInfo = null;
        await Task.Run(() =>
        {
            if (_policy == null) return;
            _policy.filterExp = Exp.Build(
                Exp.EQ(Exp.StringBin("username"), Exp.Val(receiver))
            );
            var recordSet = _client.Query(_policy, stmt);
            while (recordSet.Next())
            {
                userInfo = recordSet.Record;
            }
        });

        return userInfo;
    }

    private bool IsConnected => _client.Connected;
}