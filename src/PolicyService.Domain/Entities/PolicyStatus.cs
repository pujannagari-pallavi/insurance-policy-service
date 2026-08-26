namespace PolicyService.Domain.Entities;

public enum PolicyStatus
{
    Draft = 1,
    PendingApproval = 2,
    Active = 3,
    Lapsed = 4,
    Cancelled = 5,
    Expired = 6
}