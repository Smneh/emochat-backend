using Contract.DTOs.Notification;
using Core.Enums;
using Core.Exceptions;
using Core.Settings;
using Nest;

namespace Repository.Notification;

public class NotificationRepository
{
    private readonly ElasticClient _elasticClient = new(
        new ConnectionSettings(new Uri(Settings.AllSettings.ElasticSettings.Host))
            .BasicAuthentication(
                Settings.AllSettings.ElasticSettings.Username,
                Settings.AllSettings.ElasticSettings.Password
            )
    );

    private static string _getNotificationIndex()
    {
        return $"notification";
    }

    public async Task RegisterNotification(Entities.Models.Notification.Notification notification, string username)
    {
        notification.Username = username;

        var indexResponse = await _elasticClient.IndexAsync(notification, i => i
            .Index(_getNotificationIndex())
            .Id(Guid.NewGuid())
        );

        if (indexResponse.Result != Result.Created)
            throw new AppException(Messages.ServerError);
    }

    public async Task SeenNotifications(string username)
    {
        var t = await _elasticClient.UpdateByQueryAsync<Entities.Models.Notification.Notification>(u => u
            .Index(_getNotificationIndex())
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Username.Suffix("keyword"))
                    .Value(username)
                )
            )
            .Script(s => s
                .Source("ctx._source.isSeen = true; ctx._source.seenDate = params.seenDate;")
                .Params(p => p.Add("seenDate", DateTime.Now)))
        );
    }

    public async Task SetEventsAsSeenByType(string type, string username)
    {
        if (string.IsNullOrEmpty(type) || type == "%%") return;

        var scriptSource = $@"if (ctx._source.type.keyword == params.type) 
                             {{ ctx._source.isSeen = true; ctx._source.seenDate = params.seenDate; }}";

        await _elasticClient.UpdateByQueryAsync<Entities.Models.Notification.Notification>(u => u
            .Index(_getNotificationIndex())
            .Query(q => q
                .Bool(b => b
                    .Must(mu => mu
                            .MatchAll(),
                        mu => mu
                            .Term(t => t
                                .Field(f => f.Username.Suffix("keyword"))
                                .Value(username)
                            )
                    )
                )
            )
            .Script(s => s
                .Source(scriptSource)
                .Params(p => p
                    .Add("type", type)
                    .Add("username", username)
                    .Add("seenDate", DateTime.Now)
                )
            )
        );
    }


    public async Task<List<Entities.Models.Notification.Notification>> GetNotifications(string username, int minRow,
        int maxRow)
    {
        var result = await _elasticClient.SearchAsync<Entities.Models.Notification.Notification>(s => s
            .Index(_getNotificationIndex())
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Username.Suffix("keyword"))
                    .Value(username)
                )
            )
            .From(minRow)
            .Size(maxRow)
            .Sort(sort => sort
                    .Descending(f => f.RegDateTime) // replace 'FieldName' with the name of your field
            )
        );

        return result.IsValid ? result.Documents.ToList() : new List<Entities.Models.Notification.Notification>();
    }

    public async Task<UnseenNotificationCountDto> GetUnseenNotificationCount(string username)
    {
        var response = await _elasticClient.CountAsync<Entities.Models.Notification.Notification>(u => u
            .Index(_getNotificationIndex())
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m1 => m1.Term(t => t.Field(f => f.IsSeen).Value(false)),
                        m2 => m2.Term(t => t.Field(f => f.Username.Suffix("keyword")).Value(username))
                    )
                )
            )
        );

        var unseenNotificationCount = new UnseenNotificationCountDto
        {
            UnreadCounts = (int)response.Count
        };

        return unseenNotificationCount;
    }
}