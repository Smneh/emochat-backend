using System.ComponentModel.DataAnnotations;

namespace Contract.Enums;

public enum NotificationType
{

    [Display(Name = nameof(ReminderNotificationSent), Description = "یادآور ")]
    ReminderNotificationSent,
    
    [Display(Name = nameof(DashboardTaskForceRemoved), Description = "حذف وظیفه")]
    DashboardTaskForceRemoved,

    [Display(Name = nameof(DashboardTaskForceUpdated), Description = "بروز رسانی وظیفه")]
    DashboardTaskForceUpdated,

    [Display(Name = nameof(TodoListMemberAdded), Description = "افزوده شدن به وظیفه")]
    TodoListMemberAdded,
    
    [Display(Name = nameof(TodoListMemberDeleted), Description = "حذف از وظیفه")]
    TodoListMemberDeleted,
    
    [Display(Name = nameof(TaskForceRequesterRemoved), Description = "حذف وظیفه")]
    TaskForceRequesterRemoved,
    
    [Display(Name = nameof(SessionMemberAdded), Description = "افزوده شدن به جلسه")]
    SessionMemberAdded,
    
    [Display(Name = nameof(SessionMemberRemoved), Description = "حذف از جلسه")]
    SessionMemberRemoved,
    
    [Display(Name = nameof(VideoConferenceMemberAdded), Description = "افزوده شدن به ویدئو کنفرانس")]
    VideoConferenceMemberAdded,
    
    [Display(Name = nameof(ProjectTaskMemberAdded), Description = "  وظیفه جدید در پروژه ")]
    ProjectTaskMemberAdded,
    
    [Display(Name = nameof(MailArchived), Description = " بایگانی نامه ")]
    MailArchived,
    
}