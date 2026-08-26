namespace PolicyService.Domain.Entities;

public sealed class Coverage
{
    private Coverage()
    {
    }

    public Coverage(Guid id, string name, string description, decimal sumInsured, decimal deductible)
    {
        Id = id;
        Name = name.Trim();
        Description = description.Trim();
        SumInsured = sumInsured;
        Deductible = deductible;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal SumInsured { get; private set; }

    public decimal Deductible { get; private set; }
}