using Contract.Commands.Notification;
using Contract.Enums;
using Core.Utilities;
using MediatR;
using Repository.Notification;

namespace CommandApp.Handlers.Notification;

public class RegisterNotificationHandler : IRequestHandler<RegisterNotificationCommand>
{
    private readonly NotificationRepository _notificationRepository;


    public RegisterNotificationHandler(NotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Unit> Handle(RegisterNotificationCommand request, CancellationToken cancellationToken)
    {
        
        var notification =new Entities.Models.Notification.Notification();
        try
        {
             notification = new Entities.Models.Notification.Notification
            {
                Id = Guid.NewGuid().ToString(),
                Content = request.Content,
                Sender = request.Sender,
                Receivers = request.Receivers,
                MetaInfo = request.MetaInfo,
                Type = request.Type,
                TypeTitle = TypeDescription.GetDescriptionFromEnumName<NotificationType>(request.Type),
                RegDateTime = DateTime.Now,
                Category = request.Category,
                IsSeen = false,
                UniqueId = "",
                ParameterValues = "",
                Title = "",
                Description = "",
                ContentId = 0,
                SenderUsername = request.Sender,
                IsObsolete = false,
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        foreach (var receiver in request.Receivers)
        {
            await _notificationRepository.RegisterNotification(notification, receiver);
        }
        return Unit.Value;
    }
}