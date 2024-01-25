using System.ComponentModel.DataAnnotations;

namespace Core.Enums;

public enum Messages
{
    [Display(Name = nameof(AuthenticationError), Description = "Authentication Error")]
    AuthenticationError = 401,
    
    [Display(Name = nameof(ServerError), Description = "Server Error")]
    ServerError = 500,

    [Display(Name = nameof(NotFound), Description = "Not Found!")]
    NotFound = 14001,

    [Display(Name = nameof(AccessError), Description = "Access Error")]
    AccessError = 14002,

    [Display(Name = nameof(IncorrectInput), Description = "Incorrect Input")]
    IncorrectInput = 14003,

    [Display(Name = nameof(GroupCreatorCanNotBeRemoved), Description = "Group Creator Can Not Be Removed")]
    GroupCreatorCanNotBeRemoved = 14004,

    [Display(Name = nameof(OperationCanceledException), Description = "Operation Canceled Exception")]
    OperationCanceledException = 14005,

    [Display(Name = nameof(Exception), Description = "Exception")]
    Exception = 14006,

    [Display(Name = nameof(CouldNotBeAdded), Description = "Could Not Be Added")]
    CouldNotBeAdded = 14007,

    [Display(Name = nameof(CouldNotBeDeleted), Description = "Could Not Be Deleted")]
    CouldNotBeDeleted = 14008,

    [Display(Name = nameof(CouldNotBeUpdated), Description = "Could Not Be Updated")]
    CouldNotBeUpdated = 14009,
    
    [Display(Name = nameof(DuplicateGroupId), Description = "Duplicate GroupId")]
    DuplicateGroupId = 14010,
    
    [Display(Name = nameof(WrongUsername), Description = "Wrong Username")]
    WrongUsername = 14011,    
    
    [Display(Name = nameof(WrongPassword), Description = "Wrong Password")]
    WrongPassword = 14012,
    
    [Display(Name = nameof(EmailExist), Description = "Email Exist")]
    EmailExist = 14013,
    
    [Display(Name = nameof(UsernameExist), Description = "Username Exist")]
    UsernameExist = 14014,
    
    [Display(Name = nameof(UserNotFound), Description = "User Not Found!")]
    UserNotFound = 14015,
    
    [Display(Name = nameof(UploadFailed), Description = "Upload Failed")]
    UploadFailed = 14016,
    
    [Display(Name = nameof(FileSizeExceedsLimit), Description = "File Size Exceeds Limit")]
    FileSizeExceedsLimit = 14017,
    
    [Display(Name = nameof(EmptyFiles), Description = "Empty Files")]
    EmptyFiles = 14018,
    
    [Display(Name = nameof(UpdateProfilePictureFailed), Description = "Update Profile Picture Failed")]
    UpdateProfilePictureFailed = 14019,

    [Display(Name = nameof(UpdateWallpaperFailed), Description = "Update Wallpaper Failed")]
    UpdateWallpaperFailed = 14020,

    [Display(Name = nameof(DeleteWallpaperFailed), Description = "Delete Wallpaper Failed")]
    DeleteWallpaperFailed = 14021,

    [Display(Name = nameof(DeleteProfilePictureFailed), Description = "Delete Profile Picture Failed")]
    DeleteProfilePictureFailed = 14022,
    
    [Display(Name = nameof(MessageNotFound), Description = "Message Not Found")]
    MessageNotFound = 14023,
    
    [Display(Name = nameof(GroupNotFound), Description = "Group Not Found")]
    GroupNotFound = 14024,
}