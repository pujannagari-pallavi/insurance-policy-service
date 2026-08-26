namespace PolicyService.Domain.Entities;

public sealed class PolicyHistory
{
    private PolicyHistory()
    {
    }

    public PolicyHistory(Guid id, string action, PolicyStatus status, string remarks)
    {
        Id = id;
        Action = action.Trim();
        Status = status;
        Remarks = remarks.Trim();
        ChangedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public PolicyStatus Status { get; private set; }

    public string Remarks { get; private set; } = string.Empty;

    public DateTime ChangedAtUtc { get; private set; }
}