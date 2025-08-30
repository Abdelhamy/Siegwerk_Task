using Pricing.Domain.Common;

namespace Pricing.Domain.ValueObjects;

public sealed class LeadTime : ValueObject
{
    public int Days { get; private set; }

    private LeadTime() { } // EF Core constructor

    private LeadTime(int days)
    {
        Days = days;
    }

    public static LeadTime Create(int days)
    {
        if (days < 0)
            throw new ArgumentException("Lead time cannot be negative", nameof(days));

        return new LeadTime(days);
    }

    public static LeadTime Zero => new(0);

    public static implicit operator int(LeadTime leadTime) => leadTime.Days;

    public static LeadTime operator +(LeadTime left, LeadTime right) => new(left.Days + right.Days);

    public static bool operator >(LeadTime left, LeadTime right) => left.Days > right.Days;

    public static bool operator <(LeadTime left, LeadTime right) => left.Days < right.Days;

    public static bool operator >=(LeadTime left, LeadTime right) => left.Days >= right.Days;

    public static bool operator <=(LeadTime left, LeadTime right) => left.Days <= right.Days;

    public DateTime EstimatedDeliveryDate => DateTime.UtcNow.AddDays(Days);

    public override string ToString() => Days == 1 ? "1 day" : $"{Days} days";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Days;
    }
}