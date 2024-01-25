using Contract.Queries.Profile;
using Core.Constants;
using MediatR;
using Newtonsoft.Json;
using Repository.Profile;
using StackExchange.Redis;

namespace QueryApp.Handlers.Profile;

public class
    GetProfilesByUsernamesHandler : IRequestHandler<GetProfilesByUsernamesQuery, List<Entities.Models.Profile.Profile>>
{
    private readonly ProfileRepository _profileRepository;
    private readonly IDatabase _redis;

    public GetProfilesByUsernamesHandler(ProfileRepository profileRepository, IConnectionMultiplexer redis)
    {
        _profileRepository = profileRepository;
        _redis = redis.GetDatabase();
    }

    public async Task<List<Entities.Models.Profile.Profile>> Handle(GetProfilesByUsernamesQuery request,
        CancellationToken cancellationToken)
    {
        var profiles = new List<Entities.Models.Profile.Profile>();
        var profilesNotInRedis = new List<string>();

        foreach (var username in request.Usernames)
        {
            var redisKey = GlobalConstants.KeyPrefix + username;
            var redisValue = await _redis.StringGetAsync(redisKey);
            if (!redisValue.HasValue)
            {
                profilesNotInRedis.Add(username);
            }
            else
            {
                profiles.Add(JsonConvert.DeserializeObject<Entities.Models.Profile.Profile>(redisValue));
            }
        }

        if (profilesNotInRedis.Count <= 0) return profiles.ToList();

        var profilesInElastic = await _profileRepository.GetProfilesByUsernames(profilesNotInRedis);

        profiles.AddRange(profilesInElastic);

        foreach (var pro in profilesInElastic)
        {
            var redisKey = GlobalConstants.KeyPrefix + pro.Username;
            await _redis.StringSetAsync(redisKey, JsonConvert.SerializeObject(pro));
        }

        return profiles.ToList();
    }
}