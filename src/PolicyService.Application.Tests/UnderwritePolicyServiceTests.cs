using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Application.Services;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Tests;

public sealed class UnderwritePolicyServiceTests
{
    [Fact]
    public async Task TransitionAsync_WhenUnderwriterSubmitsDraft_MovesPolicyToPendingApproval()
    {
        var policy = TestPolicyFactory.CreatePolicy();
        var unitOfWork = new FakeUnitOfWork();
        var service = new UnderwritePolicyService(new FakePolicyRepository(policy), unitOfWork, new PolicyResponseFactory());

        var response = await service.TransitionAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(PolicyStatus.PendingApproval, "Submitted for underwriting review."),
            new PolicyActor(Guid.NewGuid(), true));

        Assert.Equal(PolicyStatus.PendingApproval, response.Status);
        Assert.Contains(response.History, item => item.Status == PolicyStatus.PendingApproval);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task TransitionAsync_WhenCallerIsNotUnderwriter_ThrowsForbiddenException()
    {
        var policy = TestPolicyFactory.CreatePolicy();
        var service = new UnderwritePolicyService(new FakePolicyRepository(policy), new FakeUnitOfWork(), new PolicyResponseFactory());

        var action = () => service.TransitionAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(PolicyStatus.PendingApproval, "Submitted for review."),
            TestActors.Default);

        await Assert.ThrowsAsync<ForbiddenException>(action);
    }

    private sealed class FakePolicyRepository(Policy policy) : IPolicyRepository
    {
        public Task<Policy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default) => Task.FromResult<Policy?>(policy);
        public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default) => Task.FromResult<Policy?>(null);
        public Task<IReadOnlyCollection<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Policy>>([]);
        public Task<IReadOnlyCollection<Policy>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Policy>>([]);
        public Task AddAsync(Policy value, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
}