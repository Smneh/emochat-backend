namespace Contract.Enums;

public enum NotificationEvent: byte
{
    ReminderNotificationCreated = 1,
    VideoConferenceMemberAddedNotificationCreated,
    OfficeAutomationMailArchivedNotificationCreated,
    PresenceEngineUserStatusNotificationCreated,
    ProjectTaskMemberAddedNotificationCreated,
    SessionMemberRemovedNotificationCreated,
    SessionMemberAddedNotificationCreated,
    TaskForceRequesterRemovedNotificationCreated,
    TodoListMemberDeletedNotificationCreated,
    TodoListMemberAddedNotificationCreated,
    BusyCallNotificationCreated,
    CanceledCallNotificationCreated,
    EndedCallNotificationCreated,
    AcceptedCallNotificationCreated,
    RejectedCallNotificationCreated,
    IncomingCallNotificationCreated,
    GroupVisitorAddedNotificationCreated,
    MessagesVisitedNotificationCreated,
    ContentDeletedNotificationCreated,
    MessageSentNotificationCreated
}