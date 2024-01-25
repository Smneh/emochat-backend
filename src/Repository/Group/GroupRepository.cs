using Contract.Commands.Group;
using Contract.DTOs.Chat;
using Core.Enums;
using Core.Exceptions;
using Core.Settings;
using Elasticsearch.Net;
using Entities.Models.Chat;
using Nest;

namespace Repository.Group;

public class GroupRepository
{
    private readonly IElasticClient _elasticClient =
        new ElasticClient(new ConnectionSettings(new Uri(Settings.AllSettings.ElasticSettings.Host)).BasicAuthentication(
            Settings.AllSettings.ElasticSettings.Username,
            Settings.AllSettings.ElasticSettings.Password)
        );

    private static string GetGroupsIndex() => "groups";
    private static string GetGroupUsersIndex() => "membership";
    private static string GetMessagesIndex() => "messages";

    public async Task<Entities.Models.Chat.Group?> GetChatGroup(string user2, string user1, CancellationToken cancellationToken)
    {
        var result = await _elasticClient.SearchAsync<Entities.Models.Chat.Group?>(
            s => s.Index(GetGroupsIndex())
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t.Field(f => f.Members.Suffix("keyword")).Value(user1)),
                            m => m.Term(t => t.Field(f => f.Members.Suffix("keyword")).Value(user2)),
                            m => m.Term(t => t.Field(f => f.Type.Suffix("keyword")).Value("User"))))
                ), cancellationToken);

        return result.IsValid ? result.Documents.FirstOrDefault() : null;
    }

    public async Task<GetChatInfoResponseDto?> GetGroupUser(string username, string groupId, CancellationToken cancellationToken)
    {
        var result = await _elasticClient.SearchAsync<GetChatInfoResponseDto>(
            s => s.Index(GetGroupUsersIndex())
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            mu => mu.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(groupId)),
                            mu => mu.Term(t => t.Field("username").Value(username))
                        )
                    )
                ), cancellationToken);

        return !result.IsValid ? null : result.Documents.FirstOrDefault();
    }

    public async Task CreateGroup(Entities.Models.Chat.Group group, CancellationToken cancellationToken)
    {
        var indexResponse = await _elasticClient.IndexAsync(group, i => i
                .Index(GetGroupsIndex())
                .Id(group.GroupId)
                .Refresh(Refresh.True),
            cancellationToken);

        if (!indexResponse.IsValid)
            throw new AppException(group, Messages.CouldNotBeAdded, group.GroupId);
    }


    public async Task<GetGroupByIdResponseDto?> GetGroupByGroupId(string groupId, CancellationToken cancellationToken)
    {
        var result = await _elasticClient.SearchAsync<GetGroupByIdResponseDto>(s => s
            .Index(GetGroupsIndex())
            .Query(q => q.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(groupId))
            ), cancellationToken);

        return result.Documents.FirstOrDefault();
    }

    public async Task CreateGroupUser(GroupUser groupUser, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString().Replace("-", "");

        var indexResponse = await _elasticClient.IndexAsync(groupUser, i => i
            .Index(GetGroupUsersIndex())
            .Id(id)
            .Refresh(Refresh.True), cancellationToken);

        if (!indexResponse.IsValid)
            throw new AppException(groupUser, Messages.CouldNotBeAdded);
    }


    public async Task CreateGroupUserForReceiver(GroupUser groupUser, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString().Replace("-", "");

        var indexResponse = await _elasticClient.IndexAsync(groupUser, i => i
                .Index(GetGroupUsersIndex())
                .Id(id)
                .Refresh(Refresh.True)
            , cancellationToken
        );

        if (!indexResponse.IsValid)
            throw new AppException(groupUser, Messages.CouldNotBeAdded);
    }


    public async Task<List<GetChatsListResponseDto>> GetAllGroupsList(string username, string type, long startRow, long rowCount,
        CancellationToken cancellationToken)
    {
        var result = await _elasticClient.SearchAsync<GetChatsListResponseDto>(
            s => s.Index(GetGroupUsersIndex())
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            mu => mu.Term(t => t.Field(f => f.Type.Suffix("keyword")).Value(type)),
                            mu => mu.Term(t => t.Field("username.keyword").Value(username))
                        )
                    )
                )
                .Sort(sort => sort
                    .Field(f => f
                        .Field(fi => fi.RegDateTime)
                        .Order(SortOrder.Descending)
                    )
                )
                .From((int?)startRow)
                .Size((int?)rowCount),
            cancellationToken);

        return !result.IsValid ? new List<GetChatsListResponseDto>() : result.Documents.ToList();
    }

    public async Task<List<GetUserCommunicatedUsernamesResponseDto>> GetUserCommunicationUsernames(string username, CancellationToken cancellationToken)
    {
        var allDocuments = new List<GetUserCommunicatedUsernamesResponseDto>();

        // Initial search request with the scroll parameter
        var initialResponse = await _elasticClient.SearchAsync<GetUserCommunicatedUsernamesResponseDto>(
            s => s.Index(GetGroupUsersIndex())
                .Size(200).Scroll("1m")
                .Query(q =>
                    q.Bool(b => b.Must(
                        m => m.Exists(e => e.Field(f => f.ReceiverId.Suffix("keyword"))),
                        m => m.Term(t => t.Field("type.keyword").Value("User")),
                        m => m.Term(t => t.Field("username").Value(username)) // Add the Username filter here
                    ))
                ), cancellationToken);

        if (initialResponse.IsValid)
        {
            allDocuments.AddRange(initialResponse.Documents);
        }

        // Use the scroll ID from the initial response to get the next batch of results
        var scrollId = initialResponse.ScrollId;
        while (true)
        {
            var scrollResponse = await _elasticClient.ScrollAsync<GetUserCommunicatedUsernamesResponseDto>("1m", scrollId, ct: cancellationToken);
            if (!scrollResponse.IsValid || !scrollResponse.Documents.Any())
            {
                break;
            }

            allDocuments.AddRange(scrollResponse.Documents);
            scrollId = scrollResponse.ScrollId;
        }

        // Clear the scroll ID when you're done with scrolling
        await _elasticClient.ClearScrollAsync(new ClearScrollRequest(scrollId), cancellationToken);

        return allDocuments;
    }

    public async Task<List<GetMessagesResponseDto>> GetGroupMessages(string groupId, long messageId, string dir, int count)
    {
        var result = await _elasticClient.SearchAsync<GetMessagesResponseDto>(s => s
            .Index(GetMessagesIndex())
            .Size(count)
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field(f => f.ReceiverId.Suffix("keyword")).Value(groupId)),
                        m => messageId != -1
                            ? (dir == "UP"
                                ? m.Range(r => r.Field(f => f.MessageId).LessThan(messageId))
                                : m.Range(r => r.Field(f => f.MessageId).GreaterThan(messageId)))
                            : m.MatchAll(), // Include all documents when messageId is -1
                        m => m.Term(t => t.Field(f => f.IsObsolete).Value(false))
                    )
                )
            )
            .Sort(sort => sort.Field(f => f.MessageId, dir == "UP" ? SortOrder.Descending : SortOrder.Ascending))
        );

        if (!result.IsValid)
            return new List<GetMessagesResponseDto>();

        return dir == "UP" ? result.Documents.Reverse().ToList() : result.Documents.ToList();
    }

    public async Task<Message?> GetLatestMessage(string groupId)
    {
        var result = await _elasticClient.SearchAsync<Message>(s => s
            .Index(GetMessagesIndex())
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field(f => f.ReceiverId.Suffix("keyword")).Value(groupId)),
                        m => m.Term(t => t.Field(f => f.IsObsolete).Value(false))
                    )
                )
            )
            .Sort(sort => sort.Field(f => f.MessageId, SortOrder.Descending))
        );

        return result.Documents.FirstOrDefault();
    }

    public async Task<List<SearchMessageResponseDto>> FullTextSearchInChat(string searchStr, string receiverId, int startRow, int rowCount)
    {
        var result = await _elasticClient.SearchAsync<SearchMessageResponseDto>(s => s
            .Index(GetMessagesIndex())
            .From(startRow).Size(rowCount)
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Match(p => p.Field(f => f.ReceiverId.Suffix("keyword")).Query(receiverId)),
                        m => m.Match(p => p.Field(f => f.Content).Query(searchStr))
                    )
                )
            )
        );

        return !result.IsValid ? new List<SearchMessageResponseDto>() : result.Documents.ToList();
    }

    public async Task RegisterMessageInChat(Message message, CancellationToken cancellationToken)
    {
        var indexResponse =
            await _elasticClient.IndexAsync<Message>(message, i => i.Index(GetMessagesIndex())
                    .Id(message.MessageId)
                    .Refresh(Refresh.True)
                , cancellationToken
            );

        if (!indexResponse.IsValid)
            throw new AppException(indexResponse, Messages.CouldNotBeAdded);
    }

    public async Task UpdateGroupUser(Message message, CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<GroupUser>(q => q
                .Index(GetGroupUsersIndex())
                .Query(q =>
                    q.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(message.ReceiverId)))
                .Script(s => s
                    .Source(@"                 
                        ctx._source.regDateTime = params.regDateTime;
                        ctx._source.content = params.content;
                        ctx._source.lastMessageId = params.lastMessageId;   
                        ctx._source.regUser = params.regUser;")
                    .Params(p => p
                        .Add("regDateTime", DateTime.Now)
                        .Add("content", message.Content)
                        .Add("lastMessageId", message.MessageId)
                        .Add("regUser", message.RegUser)))
                .Refresh()
            , cancellationToken);
    }


    public async Task RegisterGroup(Entities.Models.Chat.Group group, CancellationToken cancellationToken)
    {
        await CreateGroup(group, cancellationToken);
    }

    public async Task RegisterGroupUser(GroupUser groupUser, CancellationToken cancellationToken)
    {
        await CreateGroupUser(groupUser, cancellationToken);
    }

    public async Task RemoveGroupUser(string username, string groupId, CancellationToken cancellationToken)
    {
        var deleteResponse = await _elasticClient.DeleteByQueryAsync<object>(d => d
            .Index(GetGroupUsersIndex())
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field("groupId.keyword").Value(groupId)),
                        m => m.Term(t => t.Field("username").Value(username))
                    )
                )
            ), cancellationToken);


        if (!deleteResponse.IsValid)
            throw new AppException(deleteResponse, Messages.CouldNotBeDeleted, deleteResponse.DebugInformation);
    }

    public async Task AddGroupMember(string groupId, List<string> members, CancellationToken cancellationToken)
    {
        var indexResponse = await _elasticClient.UpdateByQueryAsync<Entities.Models.Chat.Group>(u => u
                .Index(GetGroupsIndex())
                .Query(q => q.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(groupId)))
                .Script(s => s
                    .Source("ctx._source.members = params.members")
                    .Params(p => p
                        .Add("members", members.Distinct().ToList())
                    )
                ).Refresh(),
            cancellationToken
        );

        if (!indexResponse.IsValid)
            throw new AppException(indexResponse, Messages.CouldNotBeAdded);
    }

    public async Task RemoveGroupMember(string groupId, List<string> members, CancellationToken cancellationToken)
    {
        var indexResponse = await _elasticClient.UpdateByQueryAsync<Entities.Models.Chat.Group>(u => u
                .Index(GetGroupsIndex())
                .Query(q => q.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(groupId)))
                .Script(s => s
                    .Source("ctx._source.members = params.members")
                    .Params(p => p.Add("members", members.Distinct().ToList()))
                ).Refresh(),
            cancellationToken
        );
        if (!indexResponse.IsValid)
            throw new AppException(indexResponse, Messages.CouldNotBeDeleted, members);
    }

    public async Task<GroupUser?> GetGroupUserByGroupId(string username, string groupId, CancellationToken cancellationToken)
    {
        var result = await _elasticClient.SearchAsync<GroupUser>(s => s
                .Index(GetGroupUsersIndex())
                .Query(q =>
                    q.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(groupId)) &&
                    q.Term(t => t.Field(f => f.Username.Suffix("keyword")).Value(username))
                )
            , cancellationToken
        );

        return !result.IsValid ? new GroupUser() : result.Documents.FirstOrDefault();
    }


    public async Task DeleteGroupById(string groupId, CancellationToken cancellationToken)
    {
        var deleteResponse = await _elasticClient.DeleteAsync<Entities.Models.Chat.Group>(groupId, d => d.Index(GetGroupsIndex()), cancellationToken);

        if (!deleteResponse.IsValid)
            throw new AppException(deleteResponse, Messages.CouldNotBeDeleted, deleteResponse.DebugInformation);
    }

    public async Task DeleteGroupUserById(string groupId, CancellationToken cancellationToken)
    {
        var deleteResponse = await _elasticClient.DeleteByQueryAsync<object>(d =>
            d.Index(GetGroupUsersIndex()).Query(q =>
                q.Term(t => t.Field("groupId.keyword").Value(groupId))), cancellationToken);

        if (!deleteResponse.IsValid)
            throw new AppException(deleteResponse, Messages.CouldNotBeDeleted, deleteResponse.DebugInformation);
    }

    public async Task DeleteGroupMessages(string groupId, CancellationToken cancellationToken)
    {
        var deleteResponse = await _elasticClient.DeleteByQueryAsync<object>(
            d => d
                .Index(GetMessagesIndex())
                .Query(q => q.Term(t => t.Field("receiverId.keyword").Value(groupId))), cancellationToken);

        if (!deleteResponse.IsValid)
            throw new AppException(deleteResponse, Messages.CouldNotBeDeleted, deleteResponse.DebugInformation);
    }


    public async Task UpdateUnReadMessages(string groupId, string exceptedUsername, long unReadMessageId, CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<object>(u => u
            .Index(GetGroupUsersIndex())
            .Script(s => s
                .Source("if (ctx._source.unReadMessages == null) { ctx._source.unReadMessages = []; } ctx._source.unReadMessages.add(params.value);")
                .Lang("painless")
                .Params(p => p.Add("value", unReadMessageId))
            ).Refresh()
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term("groupId.keyword", groupId)
                    )
                    .MustNot(mn => mn.Term("username", exceptedUsername))
                )
            ).Refresh(), cancellationToken);

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse, Messages.CouldNotBeUpdated, updateResponse.DebugInformation);
    }

    public async Task VisitAllMessages(string username, string groupId, long messageId, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateByQueryAsync<object>(u => u
                .Index(GetGroupUsersIndex())
                .Query(q =>
                    q.Term(t => t.Field("groupId.keyword").Value(groupId))
                    &&
                    q.Term(t => t.Field("username.keyword").Value(username)))
                .Script(ss => ss
                    .Source($@"
                    def updatedMessages = ctx._source.unReadMessages.findAll(message -> message > {messageId});
                    ctx._source.unReadMessages = updatedMessages;"
                    )
                )
                .Refresh()
            , cancellationToken);

        if (!response.IsValid)
            throw new AppException(response, Messages.CouldNotBeUpdated, response.DebugInformation);
    }

    public async Task RegisterNewGroupVisitor(List<long> messageIds, string regUser, GroupVisitorDto groupVisitorDto, CancellationToken cancellationToken)
    {
        var bulkDescriptor = new BulkDescriptor();

        foreach (var messageId in messageIds)
        {
            bulkDescriptor.Update<object, object>(op => op
                    .Index(GetMessagesIndex())
                    .Id(messageId)
                    .Script(script => script
                        .Source("if (!ctx._source.visitors.contains(params.value) && ctx._source.regUser != params.regUser) {" +
                                "ctx._source.visitors.add(params.value);" +
                                "}")
                        .Params(p => p
                            .Add("value", groupVisitorDto)
                            .Add("regUser", regUser)
                        )
                    )
                )
                .Refresh(Refresh.True);
        }

        var bulkResponse = await _elasticClient.BulkAsync(bulkDescriptor, cancellationToken);

        if (bulkResponse.Errors)
        {
            foreach (var itemWithError in bulkResponse.ItemsWithErrors)
            {
                throw new AppException(itemWithError, Messages.CouldNotBeAdded, itemWithError.Id);
            }
        }
    }

    public async Task<List<GetMessagesResponseDto>> GetGroupMessageById(long messageId, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.SearchAsync<GetMessagesResponseDto>(d => d
                .Index(GetMessagesIndex())
                .Query(q => q
                    .Term(t => t.Field(f => f.MessageId).Value(messageId))
                )
            , cancellationToken
        );

        return !response.IsValid ? new List<GetMessagesResponseDto>() : response.Documents.ToList();
    }

    public async Task DeleteMessageContent(long messageId, CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<Message>(q => q
                .Index(GetMessagesIndex())
                .Query(qs => qs.Term(t => t.Field(f => f.MessageId).Value(messageId)))
                .Script(s => s
                    .Source(
                        "ctx._source.isObsolete = params.obsolete;" +
                        " ctx._source.content = params.newContent"
                    )
                    .Lang("painless")
                    .Params(p => p
                        .Add("obsolete", true)
                        .Add("newContent", "deleted")
                    )
                )
                .Refresh()
            , cancellationToken
        );

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse, Messages.CouldNotBeDeleted, updateResponse.DebugInformation);
    }


    public async Task DeleteMessageContentFromGroupUser(string groupId, CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<GroupUser>(u =>
            u.Index(GetGroupUsersIndex()).Query(q => q.Term(t => t.Field(f =>
                f.GroupId.Suffix("keyword")).Value(groupId))).Script(s => s
                .Source(@"                 
                        ctx._source.regDateTime = params.regDateTime;
                        ctx._source.content = params.content;
                ").Params(p => p.Add("regDateTime", DateTime.Now).Add("content", "deleted"))).Refresh(), cancellationToken);

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse, Messages.CouldNotBeDeleted, updateResponse.DebugInformation);
    }

    public async Task AddGroupAdmin(string groupId, List<string> admins, CancellationToken cancellationToken)
    {
        var indexResponse = await _elasticClient.UpdateAsync<Entities.Models.Chat.Group, object>(groupId,
            u => u
                .Index(GetGroupsIndex())
                .Doc(new Entities.Models.Chat.Group { Admins = admins.Distinct().ToList(), })
            , cancellationToken);

        if (!indexResponse.IsValid)
            throw new AppException(indexResponse, Messages.CouldNotBeAdded, indexResponse.DebugInformation);
    }

    public async Task RemoveGroupAdmin(string groupId, List<string> admins, CancellationToken cancellationToken)
    {
        var indexResponse = await _elasticClient.UpdateAsync<Entities.Models.Chat.Group, object>(groupId,
            u => u
                .Index(GetGroupsIndex())
                .Doc(new Entities.Models.Chat.Group { Admins = admins.Distinct().ToList(), })
            , cancellationToken);

        if (!indexResponse.IsValid)
            throw new AppException(indexResponse, Messages.CouldNotBeDeleted, indexResponse.DebugInformation);
    }

    public async Task UpdateGroup(UpdateGroupCommand command, CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<Entities.Models.Chat.Group>(u => u
                .Index(GetGroupsIndex())
                .Query(q => q
                    .Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(command.ReceiverId))
                )
                .Script(s => s
                    .Source(@"                 
                        ctx._source.description = params.description;
                        ctx._source.profilePictureId = params.profilePictureId;
                        ctx._source.title = params.title;")
                    .Params(p => p
                        .Add("description", command.Description)
                        .Add("profilePictureId", command.ProfilePictureId)
                        .Add("title", command.Title))
                )
                .Refresh()
            , cancellationToken
        );

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse, Messages.CouldNotBeUpdated, updateResponse.DebugInformation);
    }

    public async Task UpdateGroupUserInfo(UpdateGroupCommand command, CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<GroupUser>(q => q
                .Index(GetGroupUsersIndex())
                .Query(q => q.Term(t => t.Field(f =>
                    f.GroupId.Suffix("keyword")).Value(command.ReceiverId)))
                .Script(s => s
                    .Source(@"                 
                            ctx._source.title = params.title;
                            ctx._source.profilePictureId = params.profilePictureId;")
                    .Params(p => p
                        .Add("title", command.Title)
                        .Add("profilePictureId", command.ProfilePictureId)
                    )
                )
                .Refresh(),
            cancellationToken);

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse, Messages.CouldNotBeUpdated, updateResponse.DebugInformation);
    }

    public async Task UpdateGroupUserSeen(string username, bool isSeen, string groupId, CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<GroupUser>(q =>
                q.Index(GetGroupUsersIndex())
                    .Query(q => q.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(groupId))
                                && q.Term(t => t.Field(f => f.Username.Suffix("keyword")).Value(username))
                    )
                    .Script(s => s.Source(@"ctx._source.isSeen = params.isSeen;")
                        .Params(p => p.Add("isSeen", isSeen))
                    )
                    .Refresh(),
            cancellationToken);

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse, Messages.CouldNotBeUpdated, updateResponse.DebugInformation);
    }


    private async Task<List<GetMessagesResponseDto>> GetGroupMessagesForSearch(string groupId, long messageId, string dir, int count)
    {
        var result = await _elasticClient.SearchAsync<GetMessagesResponseDto>(s =>
            s.Index(GetMessagesIndex())
                .Size(count).Query(q =>
                    q.Bool(b => b.Must(m =>
                            m.Term(t => t.Field(f => f.ReceiverId.Suffix("keyword")).Value(groupId)),
                        m => messageId != -1
                            ? (dir == "UP"
                                ? m.Range(r => r.Field(f => f.MessageId).LessThanOrEquals(messageId))
                                : m.Range(r => r.Field(f => f.MessageId).GreaterThan(messageId)))
                            : m.MatchAll() // Include all documents when messageId is -1
                    ))).Sort(s => s.Field(f => f.MessageId, dir == "UP" ? SortOrder.Descending : SortOrder.Ascending)));

        if (!result.IsValid)
            return new List<GetMessagesResponseDto>();

        return dir == "UP" ? result.Documents.Reverse().ToList() : result.Documents.ToList();
    }

    public async Task<List<GetMessagesResponseDto>> GetGroupMessagesSearchResult(string groupId, long messageId, int count)
    {
        var resultUp = await GetGroupMessagesForSearch(groupId, messageId, "UP", count);
        var resultDown = await GetGroupMessagesForSearch(groupId, messageId, "DOWN", count);
        var searchResult = resultUp.Concat(resultDown).ToList();
        return searchResult;
    }

    public async Task UpdateGroupUser(Message message, string username, bool isSeen, long unReadMessageId,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<GroupUser>(u => u
                .Index(GetGroupUsersIndex())
                .Query(q => q.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(message.ReceiverId)))
                .Script(s => s
                    .Source(@"                 
                                     ctx._source.regDateTime = params.regDateTime;
                                     ctx._source.content = params.content;
                                     ctx._source.lastMessageId = params.lastMessageId;  
                                     ctx._source.regUser = params.regUser;
                                     if (ctx._source.username == params.username) {
                                         ctx._source.isSeen = params.isSeen;
                                         }
                                     if (ctx._source.unReadMessages == null  ) 
                                      { 
                                         ctx._source.unReadMessages = []; 
                                      }
                                      if (ctx._source.username != params.username ) 
                                      {
                                         ctx._source.unReadMessages.add(params.unReadMessageId);
                                      }
                                  ")
                    .Params(p => p
                        .Add("regDateTime", DateTime.Now)
                        .Add("content", message.Content)
                        .Add("lastMessageId", message.MessageId)
                        .Add("regUser", message.RegUser)
                        .Add("isSeen", isSeen)
                        .Add("unReadMessageId", unReadMessageId)
                        .Add("username", username)
                    )
                )
                .Refresh()
            , cancellationToken);

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse, Messages.CouldNotBeUpdated, updateResponse.DebugInformation);
    }

    public async Task UpdateDeletedMessageInGroupUser(string receiverId, DateTime regDate, string content, long lastMessageId,
        string regUser, CancellationToken cancellationToken)
    {
        var updateResponse = await _elasticClient.UpdateByQueryAsync<GroupUser>(u => u
                .Index(GetGroupUsersIndex())
                .Query(q => q.Term(t => t.Field(f => f.GroupId.Suffix("keyword")).Value(receiverId)))
                .Script(s => s
                    .Source(@"                 
                                 ctx._source.regDateTime = params.regDateTime;
                                 ctx._source.content = params.content;
                                 ctx._source.lastMessageId = params.lastMessageId;  
                                 ctx._source.regUser = params.regUser;"
                    )
                    .Params(p => p
                        .Add("regDateTime", regDate)
                        .Add("content", content)
                        .Add("lastMessageId", lastMessageId)
                        .Add("regUser", regUser)
                    )
                )
                .Refresh()
            , cancellationToken);

        if (!updateResponse.IsValid)
            throw new AppException(updateResponse, Messages.CouldNotBeUpdated, updateResponse.DebugInformation);
    }

    public async Task<List<GetChatsListResponseDto>> GetUnReadsCount(string username, CancellationToken cancellationToken)
    {
        var result = await _elasticClient.SearchAsync<GetChatsListResponseDto>(s =>
            s.Index(GetGroupUsersIndex())
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                                .Exists(e => e
                                    .Field("unReadMessages")
                                ),
                            m => m
                                .Term(t => t
                                    .Field("username.keyword")
                                    .Value(username)
                                )
                        )
                    )
                )
                .Aggregations(aggs => aggs
                    .Sum("total_unread_messages", sum => sum
                        .Script(script => script
                            .Source("doc['unReadMessages'].size()")
                        )
                    )
                ), cancellationToken);
        return result.IsValid ? result.Documents.ToList() : new List<GetChatsListResponseDto>();
    }

    public async Task<long> GetNewMessageId()
    {
        var result = await _elasticClient.SearchAsync<GetMessagesResponseDto>(s => s
            .Index(GetMessagesIndex())
            .Size(1)
            .Sort(sort => sort
                .Field(f => f.MessageId, SortOrder.Descending))
        );

        var lastMessage = result.Documents.FirstOrDefault();

        if (lastMessage == null)
            return 1;

        return lastMessage.MessageId + 1;
    }
}