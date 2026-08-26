namespace PolicyService.Application.Contracts.Policies;

public sealed record PolicyActor(Guid IdentityUserId, bool CanManageAnyPolicy);