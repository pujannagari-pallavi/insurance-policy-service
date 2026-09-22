using PolicyService.Application.Abstractions.Validation;
using PolicyService.Application.Abstractions.Customers;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Application.Services;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Tests;

public sealed class CreatePolicyServiceTests
{
    private static readonly Guid HealthPolicyTypeId = Guid.Parse("4FBA5C78-2DE5-4857-9E29-0A091572A001");

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_PersistsPolicyAndReturnsResponse()
    {
        var repository = new FakePolicyRepository();
        var policyTypeRepository = new FakePolicyTypeRepository(TestPolicyFactory.CreatePolicyType());
        var unitOfWork = new FakeUnitOfWork();
        var service = new CreatePolicyService(
            repository,
            policyTypeRepository,
            unitOfWork,
            new PassThroughValidator<CreatePolicyRequest>(),
            new FakeCustomerAccessValidator(),
            new PremiumRatingService(),
            new PolicyResponseFactory());

        var response = await service.CreateAsync(TestRequests.CreatePolicyRequest(), TestActors.Default);

        Assert.NotNull(repository.AddedPolicy);
        Assert.StartsWith("SC-2026-", repository.AddedPolicy!.PolicyNumber);
        Assert.Single(repository.AddedPolicy.Coverages);
        Assert.NotEmpty(repository.AddedPolicy.History);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(repository.AddedPolicy.Id, response.Id);
        Assert.Equal("Health Standard", response.PolicyType.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenCustomerAccessIsDenied_DoesNotPersistPolicy()
    {
        var repository = new FakePolicyRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new CreatePolicyService(
            repository,
            new FakePolicyTypeRepository(TestPolicyFactory.CreatePolicyType()),
            unitOfWork,
            new PassThroughValidator<CreatePolicyRequest>(),
            new FakeCustomerAccessValidator(new ForbiddenException("You are not allowed to manage policies for this customer.")),
            new PremiumRatingService(),
            new PolicyResponseFactory());

        var action = () => service.CreateAsync(TestRequests.CreatePolicyRequest(), TestActors.Default);

        await Assert.ThrowsAsync<ForbiddenException>(action);
        Assert.Null(repository.AddedPolicy);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakePolicyRepository : IPolicyRepository
    {
        public Policy? PolicyById { get; init; }

        public Policy? PolicyByPolicyNumber { get; init; }

        public Policy? AddedPolicy { get; private set; }

        public Task<Policy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PolicyById);
        }

        public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PolicyByPolicyNumber);
        }

        public Task<IReadOnlyCollection<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Policy>>([]);
        }

        public Task<IReadOnlyCollection<Policy>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Policy>>([]);
        }

        public Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
        {
            AddedPolicy = policy;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePolicyTypeRepository(PolicyType? policyType) : IPolicyTypeRepository
    {
        public Task AddAsync(PolicyType policyType, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<PolicyType?> GetByIdAsync(Guid policyTypeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(policyType);
        }

        public Task<IReadOnlyCollection<PolicyType>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<PolicyType>>(policyType is null ? [] : [policyType]);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class PassThroughValidator<T> : IValidator<T>
    {
        public void Validate(T value)
        {
        }
    }
}

internal sealed class FakeCustomerAccessValidator(Exception? exception = null) : ICustomerAccessValidator
{
    public Task EnsureAccessAsync(
        Guid customerId,
        Guid identityUserId,
        bool canManageAnyPolicy,
        bool requireVerifiedKyc = false,
        CancellationToken cancellationToken = default)
    {
        return exception is null ? Task.CompletedTask : Task.FromException(exception);
    }
}

internal static class TestActors
{
    public static readonly PolicyActor Default = new(Guid.Parse("93CD493C-126D-494F-B955-3AE5C0239CFC"), false, false);
}

internal static class TestRequests
{
    private static readonly Guid HealthPolicyTypeId = Guid.Parse("4FBA5C78-2DE5-4857-9E29-0A091572A001");
    private static readonly Guid AutoPolicyTypeId = Guid.Parse("4FBA5C78-2DE5-4857-9E29-0A091572A002");

    public static CreatePolicyRequest CreatePolicyRequest()
    {
        return new CreatePolicyRequest(
            Guid.Parse("1B4AB7AA-C2D4-4A7A-9BE9-54EB46066001"),
            HealthPolicyTypeId,
            new DateOnly(2026, 8, 1),
            new DateOnly(2027, 7, 31),
            [new CoverageRequest("Hospitalization", "In-patient hospitalization coverage.", 500000m, 10000m)],
            "Initial policy creation.");
    }

    public static UpdatePolicyRequest UpdatePolicyRequest()
    {
        return new UpdatePolicyRequest(
            Guid.Parse("1B4AB7AA-C2D4-4A7A-9BE9-54EB46066002"),
            AutoPolicyTypeId,
            new DateOnly(2026, 9, 1),
            new DateOnly(2027, 8, 31),
            PolicyStatus.Active,
            [new CoverageRequest("Collision", "Collision protection coverage.", 650000m, 15000m)],
            "Policy activated after review.");
    }
}

internal static class TestPolicyFactory
{
    private static readonly Guid HealthPolicyTypeId = Guid.Parse("4FBA5C78-2DE5-4857-9E29-0A091572A001");

    public static PolicyType CreatePolicyType()
    {
        return new PolicyType(
            HealthPolicyTypeId,
            "HEALTH_STANDARD",
            "Health Standard",
            "Standard individual health policy.",
            12500m);
    }

    public static Policy CreatePolicy(string policyNumber = "POL-2026-0001")
    {
        var policyType = CreatePolicyType();
        var policy = new Policy(
            Guid.NewGuid(),
            policyNumber,
            Guid.NewGuid(),
            policyType.Id,
            new DateOnly(2026, 8, 1),
            new DateOnly(2027, 7, 31),
            14000m);
        policy.AttachPolicyType(policyType);
        policy.SetCoverages([new Coverage(Guid.NewGuid(), "Hospitalization", "In-patient hospitalization coverage.", 500000m, 10000m)]);
        policy.AddHistory("Created", PolicyStatus.Draft, "Initial policy creation.");
        return policy;
    }
}