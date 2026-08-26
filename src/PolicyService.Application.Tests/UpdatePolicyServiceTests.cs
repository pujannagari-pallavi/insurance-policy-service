using PolicyService.Application.Abstractions.Validation;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Application.Services;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Tests;

public sealed class UpdatePolicyServiceTests
{
    private static readonly Guid AutoPolicyTypeId = Guid.Parse("4FBA5C78-2DE5-4857-9E29-0A091572A002");

    [Fact]
    public async Task UpdateAsync_WhenPolicyExists_UpdatesPolicyAndSavesChanges()
    {
        var policy = TestPolicyFactory.CreatePolicy();
        var repository = new FakePolicyRepository(policy);
        var policyTypeRepository = new FakePolicyTypeRepository(
            new PolicyType(
                AutoPolicyTypeId,
                "AUTO_COMPREHENSIVE",
                "Auto Comprehensive",
                "Comprehensive motor insurance coverage.",
                9300m));
        var unitOfWork = new FakeUnitOfWork();
        var service = new UpdatePolicyService(
            repository,
            policyTypeRepository,
            unitOfWork,
            new PassThroughValidator<UpdatePolicyRequest>(),
            new FakeCustomerAccessValidator(),
            new PolicyResponseFactory());

        var response = await service.UpdateAsync(policy.Id, TestRequests.UpdatePolicyRequest(), TestActors.Default);

        Assert.Equal(PolicyStatus.Active, policy.Status);
        Assert.Equal(16000m, policy.PremiumAmount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal("Auto Comprehensive", response.PolicyType.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenPolicyDoesNotExist_ThrowsNotFoundException()
    {
        var service = new UpdatePolicyService(
            new FakePolicyRepository(null),
            new FakePolicyTypeRepository(TestPolicyFactory.CreatePolicyType()),
            new FakeUnitOfWork(),
            new PassThroughValidator<UpdatePolicyRequest>(),
            new FakeCustomerAccessValidator(),
            new PolicyResponseFactory());

        var action = () => service.UpdateAsync(Guid.NewGuid(), TestRequests.UpdatePolicyRequest(), TestActors.Default);

        var exception = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal("Policy was not found.", exception.Message);
    }

    private sealed class FakePolicyRepository(Policy? policy) : IPolicyRepository
    {
        public Task<Policy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(policy);
        }

        public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Policy?>(null);
        }

        public Task<IReadOnlyCollection<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Policy>>(policy is null ? [] : [policy]);
        }

        public Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePolicyTypeRepository(PolicyType? policyType) : IPolicyTypeRepository
    {
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