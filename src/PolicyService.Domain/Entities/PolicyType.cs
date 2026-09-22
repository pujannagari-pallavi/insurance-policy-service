namespace PolicyService.Domain.Entities;

public sealed class PolicyType
{
    private PolicyType()
    {
    }

    public PolicyType(Guid id, string code, string name, string description, decimal basePremium, bool isAvailable = true)
    {
        Id = id;
        Code = code.Trim();
        Name = name.Trim();
        Description = description.Trim();
        BasePremium = basePremium;
        IsAvailable = isAvailable;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal BasePremium { get; private set; }

    public bool IsAvailable { get; private set; }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }
}