namespace Contract.Enums;

public enum ChatEvent
{
    MessageSent,
    GroupCreated,
    GroupMemberAdded,
    GroupMemberRemoved,
    GroupDeleted,
    MessagesVisited,
    GroupVisitorAdded,
    GroupMemberLeft,
    MessageDeleted,
    GroupAdminAdded,
    GroupAdminRemoved,
    GroupUpdated,
    GroupLinkStatusUpdated,
    GroupSendFileStatusUpdated,
    GroupCopyStatusUpdated,
    GroupUserSeenUpdated,
    IncomingCall,
    AcceptedCall,
    RejectedCall,
    CanceledCall,
    BusyCall,
    EndedCall,
    AcceptedWithAnotherAccount,
    RejectedWithAnotherAccount
}