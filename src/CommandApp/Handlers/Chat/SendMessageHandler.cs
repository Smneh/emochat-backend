using Confluent.Kafka;
using Contract.Commands.Group;
using Contract.DTOs.Chat;
using Contract.DTOs.KafkaEvents;
using Contract.Enums;
using Contract.Events;
using Contract.Queries.Profile;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using Entities.Models.Chat;
using MediatR;
using Newtonsoft.Json;
using Repository.Group;

namespace CommandApp.Handlers.Chat;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, SendMessageResponseDto>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly IProducer<Null, string> _producer;
    private readonly ISender _sender;

    public SendMessageHandler(IdentityService identityService, GroupRepository groupRepository, IProducer<Null, string> producer, ISender sender)
    {
        _identityService = identityService;
        _groupRepository = groupRepository;
        _producer = producer;
        _sender = sender;
    }
    
    public async Task<SendMessageResponseDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Set RegDate and RegTime
        request.RegDate = DateTime.Now.ToString();
        request.RegTime = DateTime.Now.TimeOfDay.ToString(); //TODO : check if its okay
            
        // Check IsMember
        var group = await _groupRepository.GetGroupByGroupId(request.ReceiverId, cancellationToken);
        if (!group.Members.Contains(_identityService.Username))
            throw new AppException(group, Messages.AccessError);

        // Register  Message in Elastic For All Groups Contents
        var newMessage = await RegisterMessageInElastic(request, cancellationToken);

        return new SendMessageResponseDto
        {
            MessageId = newMessage.MessageId,
            UniqueId = newMessage.UniqueId,
            ReceiverId = newMessage.ReceiverId,
            Content = newMessage.Content,
            Type = newMessage.Type
        };
    }

    private async Task<Message> RegisterMessageInElastic(SendMessageCommand command, CancellationToken cancellationToken)
    {
        var groupInfo = await _groupRepository.GetGroupByGroupId(command.ReceiverId, cancellationToken);

        var messageId = await _groupRepository.GetNewMessageId();
        
        var Message = new Message
        {
            MessageId = messageId,
            Content = command.Content,
            ReceiverId = command.ReceiverId,
            UniqueId = Guid.NewGuid().ToString(),
            Type = "Group",
            GroupTypeId = groupInfo.TypeId,
            IsObsolete = false,
            RegUser = _identityService.Username,
            Visitors = new List<GroupVisitor>(),
            MessageTypeId = command.MessageTypeId,
            Attachments = command.Attachments,
            RegDateTime = DateTime.Now,
            RegDate = DateTime.Now,
            ParentId = command.ParentId,
            IsFirst = command.IsFirst
        };

        // Fill ReplyMessageDto
        if (command.ReplyMessage != null)
        {
            var replyMessage = new ReplyMessage
            {
                MessageId = command.ReplyMessage.MessageId,
                Attachments = command.ReplyMessage.Attachments,
                MessageTypeId = command.ReplyMessage.MessageTypeId,
                UniqueId = command.ReplyMessage.UniqueId,
                Content = command.ReplyMessage.Content,
                ReceiverId = command.ReplyMessage.ReceiverId,
                GroupTypeId = command.ReplyMessage.GroupTypeId,
                RegUser = command.ReplyMessage.RegUser,
            };

            Message.ReplyMessage = replyMessage;
        }

        // Push MessageSend Event
        await PushRegisterMessageEvent(Message, groupInfo.Members);

        return Message;
    }

    private async Task PushRegisterMessageEvent(Message newMessage, List<string> members)
    {
        // Handle Reply Message
        ReplyMessageDto? replyMessage = null;
        if (newMessage.ReplyMessage != null)
        {
            replyMessage = new ReplyMessageDto();
            replyMessage.MessageId = newMessage.ReplyMessage.MessageId;
            replyMessage.Attachments = newMessage.ReplyMessage.Attachments;
            replyMessage.MessageTypeId = newMessage.ReplyMessage.MessageTypeId;
            replyMessage.UniqueId = newMessage.ReplyMessage.UniqueId;
            replyMessage.Content = newMessage.ReplyMessage.Content;
            replyMessage.ReceiverId = newMessage.ReplyMessage.ReceiverId;
            replyMessage.GroupTypeId = newMessage.ReplyMessage.GroupTypeId;
            replyMessage.RegUser = newMessage.ReplyMessage.RegUser;
        }

        // Handle Visitors
        var visitors = new List<GroupVisitorDto>();
        newMessage.Visitors.ForEach(visitor =>
        {
            var groupVisitor = new GroupVisitorDto
            {
                Username = visitor.Username,
                DateTime = visitor.DateTime
            };

            visitors.Add(groupVisitor);
        });


        var regUserProfile = await _getProfileInfo(newMessage.RegUser);

        var kafkaEvent = new KafkaEvent<ChatEvent>
        {
            Event = new Event
            {
                Username = _identityService.Username,
                Data = new MessageSentEvent
                {
                    MessageId = newMessage.MessageId,
                    Content = newMessage.Content,
                    ReceiverId = newMessage.ReceiverId,
                    UniqueId = newMessage.UniqueId,
                    Type = newMessage.Type,
                    RegUser = _identityService.Username,
                    MessageTypeId = newMessage.MessageTypeId,
                    Attachments = newMessage.Attachments,
                    RegDateTime = DateTime.Now,
                    RegDate = DateTime.Now,
                    Members = members,
                    ReplyMessage = replyMessage,
                    FullName = regUserProfile.Fullname,
                    ProfileAddress = regUserProfile.ProfilePictureId,
                    GroupTypeId = newMessage.GroupTypeId,
                    Visitors = visitors,
                    IsFirst = newMessage.IsFirst
                },
                DateTime = DateTime.Now,
                
            },
            EventType = ChatEvent.MessageSent
        };

        var message = new Message<Null, string> {Value = JsonConvert.SerializeObject(kafkaEvent)};
        await _producer.ProduceAsync(nameof(ChatEvent), message);
    }

    private async Task<Entities.Models.Profile.Profile> _getProfileInfo(string username)
    {
        var getProfilesByUsernamesRequest = new GetProfilesByUsernamesQuery
        {
            Usernames = new List<string>
            {
                username
            }
        };
        var userProfiles = await _sender.Send(getProfilesByUsernamesRequest);
        return userProfiles.FirstOrDefault()!;
    }
}