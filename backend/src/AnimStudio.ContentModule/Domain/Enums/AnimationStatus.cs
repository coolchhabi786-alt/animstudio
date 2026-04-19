namespace AnimStudio.ContentModule.Domain.Enums;

public enum AnimationStatus
{
    PendingApproval = 0,
    Approved        = 1,
    Running         = 2,
    Completed       = 3,
    Failed          = 4,
    Cancelled       = 5,
}
