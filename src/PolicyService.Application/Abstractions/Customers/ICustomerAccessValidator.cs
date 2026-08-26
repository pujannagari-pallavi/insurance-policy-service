namespace PolicyService.Application.Abstractions.Customers;

public interface ICustomerAccessValidator
{
    Task EnsureAccessAsync(
        Guid customerId,
        Guid identityUserId,
        bool canManageAnyPolicy,
        CancellationToken cancellationToken = default);
}