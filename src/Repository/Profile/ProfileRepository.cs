using Core.Enums;
using Core.Exceptions;
using Core.Extensions;
using Core.Interfaces;
using Core.Settings;
using Elasticsearch.Net;
using Entities.Models.Profile;
using Nest;
using Newtonsoft.Json;

namespace Repository.Profile;

public class ProfileRepository : ISingletonDependency
{
    private readonly IElasticClient _elasticClient = new ElasticClient(
        new ConnectionSettings(new Uri(Settings.AllSettings.ElasticSettings.Host)).BasicAuthentication(
            Settings.AllSettings.ElasticSettings.Username, Settings.AllSettings.ElasticSettings.Password));

    private static string GetProfilesIndex() => "profiles";

    public async Task IndexProfile(User profile)
    {
        var indexResponse = await _elasticClient.IndexAsync(profile, i => i
            .Index(GetProfilesIndex())
            .Id(profile.Id)
            .Refresh(Refresh.True)
        );

        if (!indexResponse.IsValid)
            throw new AppException(indexResponse.DebugInformation, Messages.CouldNotBeAdded, profile.Id);
    }

    public async Task UpdateProfile(string username, string fullname)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<Entities.Models.Profile.Profile>(u => u
            .Index(GetProfilesIndex())
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Username.Suffix("keyword")).Value(username)
                )
            ).Script(script => script
                .Source("ctx._source.fullname = params.fullname;"
                )
                .Params(p => p
                    .Add("fullname", fullname)
                )
            )
            .Refresh()
        );

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse.DebugInformation, Messages.ServerError, username);
    }

    public async Task UpdateProfileFullname(Entities.Models.Profile.Profile profile)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<Entities.Models.Profile.Profile>(u => u
            .Index(GetProfilesIndex())
            .Query(q => q.Term(t => t.Field(f => f.Username.Suffix("keyword")).Value(profile.Username))).Script(
                script =>
                    script.Source("ctx._source.fullname = params.fullname;")
                        .Params(p => p
                            .Add("fullname", profile.Fullname))));

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse.DebugInformation, Messages.ServerError, profile.Username);
    }

    public async Task UpdatePictureAsync(string pictureData, string username, string fieldName)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<Entities.Models.Profile.Profile>(u => u
            .Index(GetProfilesIndex())
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Username.Suffix("keyword")).Value(username)))
            .Script(
                script => script.Source($"ctx._source.{fieldName} = params.pictureData;")
                    .Params(p => p.Add("pictureData", pictureData))));

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse.DebugInformation, Messages.ServerError, pictureData, fieldName);
    }

    public async Task<List<Entities.Models.Profile.Profile>> GetProfilesByUsernames(IEnumerable<string> usernames)
    {
        var distinctUsernames = usernames.Distinct().ToList();

        var searchResponse = await _elasticClient.SearchAsync<Entities.Models.Profile.Profile>(s => s
            .Index(GetProfilesIndex())
            .Query(q => q
                .Terms(t => t
                    .Field(f => f.Username.Suffix("keyword"))
                    .Terms(distinctUsernames)
                )
            )
            .Size(distinctUsernames.Count));

        if (!searchResponse.IsValid)
            throw new AppException(searchResponse.DebugInformation, Messages.NotFound);

        return searchResponse.Documents.ToList();
    }

    public async Task<List<Entities.Models.Profile.Profile>> GetAllUsers(string currentUser, string? requestSearchText, int limit = 50, int offset = 0)
    {
        var searchRequest = new SearchRequest<Entities.Models.Profile.Profile>(GetProfilesIndex())
            { From = offset, Size = limit };

        if (!string.IsNullOrEmpty(requestSearchText) && requestSearchText != "%%")
        {
            requestSearchText = requestSearchText.Replace("%", "");
            searchRequest.Query = new BoolQuery
            {
                Should = new QueryContainer[]
                {
                    new WildcardQuery { Field = "username.keyword", Value = $"*{requestSearchText}*".ToLower() },
                    new WildcardQuery { Field = "fullname.keyword", Value = $"*{requestSearchText}*".ToLower() }
                }
            };
        }

        searchRequest.Query = new BoolQuery
        {
            Must = new[]
            {
                searchRequest.Query,
            },
            MustNot = new QueryContainer[]
            {
                new TermQuery { Field = "username.keyword", Value = currentUser }
            }
        };

        var searchResponse = await _elasticClient.SearchAsync<Entities.Models.Profile.Profile>(searchRequest);

        if (!searchResponse.IsValid)
            throw new AppException(searchResponse.DebugInformation, Messages.NotFound);

        var profiles = searchResponse.Documents.ToList();

        return profiles;
    }

    public async Task<List<string>> GetAllUsersByPriority(string? requestSearchText, IEnumerable<string> priority,
        IEnumerable<string> exclude,
        string currentUser, int limit = 50, int offset = 0)
    {
        var searchRequest = new SearchRequest<Entities.Models.Profile.Profile>(GetProfilesIndex())
            { From = offset, Size = limit };

        if (!string.IsNullOrEmpty(requestSearchText) && requestSearchText != "%%")
        {
            requestSearchText = requestSearchText.Replace("%", "");
            searchRequest.Query = new BoolQuery
            {
                Should = new QueryContainer[]
                {
                    new WildcardQuery { Field = "username.keyword", Value = $"*{requestSearchText}*".ToLower() },
                    new WildcardQuery { Field = "fullname.keyword", Value = $"*{requestSearchText}*".ToLower() }
                }
            };
        }

        searchRequest.Query = new BoolQuery
        {
            Must = new[]
            {
                searchRequest.Query,
                new TermsQuery() { Field = "username.keyword", Terms = priority }
            },
            MustNot = new QueryContainer[]
            {
                new TermsQuery() { Field = "username.keyword", Terms = exclude },
                new TermQuery { Field = "username.keyword", Value = currentUser }
            }
        };

        var searchResponse = await _elasticClient.SearchAsync<Entities.Models.Profile.Profile>(searchRequest);

        if (!searchResponse.IsValid)
            throw new AppException(searchResponse.DebugInformation, Messages.NotFound);

        return searchResponse.Documents.Select(JsonConvert.SerializeObject).ToList();
    }

    public async Task<List<Entities.Models.Profile.Profile>> GetAllUsersExceptPriority(string? requestSearchText, IEnumerable<string> exclude, string currentUser,
        int limit = 50, int offset = 0)
    {
        var searchRequest = new SearchRequest<Entities.Models.Profile.Profile>(GetProfilesIndex())
            { From = offset, Size = limit };

        if (!string.IsNullOrEmpty(requestSearchText) && requestSearchText != "%%")
        {
            requestSearchText = requestSearchText.Replace("%", "");
            searchRequest.Query = new BoolQuery
            {
                Should = new QueryContainer[]
                {
                    new WildcardQuery { Field = "username.keyword", Value = $"*{requestSearchText}*".ToLower() },
                    new WildcardQuery { Field = "fullname.keyword", Value = $"*{requestSearchText}*".ToLower() }
                }
            };
        }

        searchRequest.Query = new BoolQuery
        {
            Must = new[]
            {
                searchRequest.Query
            },
            MustNot = new QueryContainer[]
            {
                new TermsQuery { Field = "username.keyword", Terms = exclude },
                new TermQuery { Field = "username.keyword", Value = currentUser }
            }
        };

        var searchResponse = await _elasticClient.SearchAsync<Entities.Models.Profile.Profile>(searchRequest);

        if (!searchResponse.IsValid)
            throw new AppException(searchResponse.DebugInformation, Messages.NotFound);

        return searchResponse.Documents.ToList();
    }

    public async Task<List<Entities.Models.Profile.Profile>> GetUsers(string currentUer, string? username,
        string? fullname, int startRow = 50, int rowCount = 0)
    {
        var searchRequest = new SearchRequest<Entities.Models.Profile.Profile>(GetProfilesIndex())
            { From = startRow, Size = rowCount };

        var shouldQueries = new List<QueryContainer>();

        if (username != "%%" && !string.IsNullOrEmpty(username))
            shouldQueries.Add(new WildcardQuery { Field = "username", Value = $"*{username}*".ToLower() });

        if (fullname != "%%" && !string.IsNullOrEmpty(fullname))
            shouldQueries.Add(new WildcardQuery { Field = "fullname", Value = $"*{fullname}*".ToLower() });

        searchRequest.Query = new BoolQuery
        {
            Should = shouldQueries,
            MustNot = new QueryContainer[]
            {
                new TermQuery { Field = "username.keyword", Value = currentUer },
            }
        };

        var searchResponse = await _elasticClient.SearchAsync<Entities.Models.Profile.Profile>(searchRequest);

        if (!searchResponse.IsValid)
            throw new AppException(searchResponse.DebugInformation, Messages.NotFound);

        var profiles = searchResponse.Documents.ToList();

        return profiles;
    }

    public async Task<Entities.Models.Profile.Profile?> GetProfile(string username)
    {
        var searchResponse = await _elasticClient.SearchAsync<Entities.Models.Profile.Profile>(u => u
            .Index(GetProfilesIndex())
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Username.Suffix("keyword")).Value(username))));

        return searchResponse.Documents.Any() ? searchResponse.Documents.FirstOrDefault() : null;
    }

    public async Task<User?> GetUser(string username)
    {
        var searchResponse = await _elasticClient.SearchAsync<User>(u => u
            .Index(GetProfilesIndex())
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Username.Suffix("keyword")).Value(username)
                )
            )
        );

        return searchResponse.Documents.Any() ? searchResponse.Documents.FirstOrDefault() : null;
    }
    
    public async Task<bool> IsEmailExist(string email)
    {
        var searchResponse = await _elasticClient.SearchAsync<User>(u => u
            .Index(GetProfilesIndex())
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Email.Suffix("keyword")).Value(email)
                )
            )
        );

        return searchResponse.Documents.Any();
    }

    public async Task DeleteProfile(string username, string workspace)
    {
        var indexName = $"{workspace}_profiles".IndexFormat();

        var response = await _elasticClient.DeleteByQueryAsync<Entities.Models.Profile.Profile>(u => u
            .Index(indexName)
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Username.Suffix("keyword")).Value(username))));

        if (!response.IsValid)
            throw new AppException(response.DebugInformation, Messages.ServerError, username, workspace);
    }
}