using Aerospike.Client;
using Core.Interfaces;
using Core.Jwt;

namespace Repository.AeroSpike;

public class UserRepository : ISingletonDependency
{
    private readonly IAerospikeClient _client;
    private const string Namespace = "IAM";

    public UserRepository(IAerospikeClient client)
    {
        _client = client;
        CreateSecondaryIndexIfNeeded();
    }


    private void CreateSecondaryIndexIfNeeded()
    {
        var setName = "Sessions";
        var binName = "username"; // bin that you want to index
        var indexName = "username_index"; // name of the index

        var policy = new Policy();
        var node = _client.Nodes[0];
        var response = Info.Request(node, "sindex");
        var indexExists = response.Contains(indexName);

        if (!indexExists)
        {
            _client.CreateIndex(policy, Namespace, setName, indexName, binName, IndexType.STRING);
        }
    }

    public async Task<UserMainInfoDto?> GetUserMainInfo(string username)
    {
        var result = _client.Get(null, new Key(Namespace, "Users", username));

        if (result == null) return null;
        
        var isObsolete = result.GetBool("isObsolete");
        
        if (isObsolete)
            return null;

        return new UserMainInfoDto
        {
            Username = username,
            Password = result.GetString("password")
        };
    }

    public async Task<Dictionary<string, object>?> GetUser(string username)
    {
        var result = _client.Get(null, new Key(Namespace, "Users", username));
        if (result == null) return null;
        var isObsolete = result.GetBool("isObsolete");
        return isObsolete
            ? null
            : result?.bins.ToDictionary(bin => bin.Key, bin => bin.Value) ?? new Dictionary<string, object>();
    }

    public async Task AddNewUser(List<Bin> bins, Key key)
    {
        var writePolicy = new WritePolicy
        {
            sendKey = true
        };

        _client.Put(writePolicy, key, bins.ToArray());
    }

    public async Task ActivateUser(string username)
    {
        var updatePolicy = new WritePolicy
        {
            recordExistsAction = RecordExistsAction.UPDATE_ONLY
        };

        var key = new Key(Namespace, "Users", username);

        var isObsolete = new Bin("isObsolete", false);

        _client.Put(updatePolicy, key, isObsolete);
    }

    public async Task DeleteUser(string username)
    {
        var updatePolicy = new WritePolicy
        {
            recordExistsAction = RecordExistsAction.UPDATE_ONLY
        };

        var key = new Key(Namespace, "Users", username);

        var isObsolete = new Bin("isObsolete", true);

        _client.Put(updatePolicy, key, isObsolete);
    }

    public async Task UpdateUser(List<Bin> bins, Key key)
    {
        var updatePolicy = new WritePolicy
        {
            recordExistsAction = RecordExistsAction.UPDATE_ONLY
        };

        _client.Put(updatePolicy, key, bins.ToArray());
    }

    public async Task UpdateUserSession(List<Bin> bins, Key key, WritePolicy policy)
    {
        _client.Put(policy, key, bins.ToArray());
    }

    public async Task DeleteUserSession(Key key, WritePolicy policy)
    {
        _client.Delete(policy, key);
    }

    public async Task<List<Dictionary<string, object>>> GetUserActiveSessions(string username)
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

        return activeSessions;
    }
}