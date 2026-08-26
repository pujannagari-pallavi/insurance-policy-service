namespace PolicyService.Domain.Entities;

public sealed class Policy
{
    private readonly List<Coverage> _coverages = [];
    private readonly List<PolicyHistory> _history = [];

    private Policy()
    {
    }

    public Policy(
        Guid id,
        string policyNumber,
        Guid customerId,
        Guid policyTypeId,
        DateOnly startDate,
        DateOnly endDate,
        decimal premiumAmount)
    {
        Id = id;
        PolicyNumber = policyNumber.Trim();
        CustomerId = customerId;
        PolicyTypeId = policyTypeId;
        StartDate = startDate;
        EndDate = endDate;
        PremiumAmount = premiumAmount;
        Status = PolicyStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string PolicyNumber { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

    public Guid PolicyTypeId { get; private set; }

    public PolicyType? PolicyType { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public decimal PremiumAmount { get; private set; }

    public PolicyStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Coverage> Coverages => _coverages;

    public IReadOnlyCollection<PolicyHistory> History => _history;

    public void AttachPolicyType(PolicyType policyType)
    {
        PolicyType = policyType;
    }

    public void SetCoverages(IEnumerable<Coverage> coverages)
    {
        _coverages.Clear();
        _coverages.AddRange(coverages);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Update(Guid customerId, Guid policyTypeId, DateOnly startDate, DateOnly endDate, decimal premiumAmount, PolicyStatus status, string remarks)
    {
        CustomerId = customerId;
        PolicyTypeId = policyTypeId;
        StartDate = startDate;
        EndDate = endDate;
        PremiumAmount = premiumAmount;
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
        AddHistory("Updated", status, remarks);
    }

    public void AddHistory(string action, PolicyStatus status, string remarks)
    {
        _history.Add(new PolicyHistory(Guid.NewGuid(), action, status, remarks));
        UpdatedAtUtc = DateTime.UtcNow;
    }
}