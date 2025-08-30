using Pricing.Domain.Common;

namespace Pricing.Domain.ValueObjects;

public sealed class DateRange : ValueObject
{
    public DateOnly From { get; private set; }
    public DateOnly? To { get; private set; }

    // Parameterless constructor for EF Core
    private DateRange() { }

    private DateRange(DateOnly from, DateOnly? to)
    {
        From = from;
        To = to;
    }

    public static DateRange Create(DateOnly from, DateOnly? to = null)
    {
        if (to.HasValue && to.Value <= from)
            throw new ArgumentException("End date must be after start date", nameof(to));

        return new DateRange(from, to);
    }

    public static DateRange FromDate(DateOnly from) => new(from, null);

    public static DateRange Between(DateOnly from, DateOnly to) => new(from, to);

    public bool Contains(DateOnly date)
    {
        return date >= From && (!To.HasValue || date <= To.Value);
    }

    public bool IsActive => Contains(DateOnly.FromDateTime(DateTime.UtcNow));

    public bool OverlapsWith(DateRange other)
    {
        if (other == null) return false;

        var thisEnd = To ?? DateOnly.MaxValue;
        var otherEnd = other.To ?? DateOnly.MaxValue;

        // No overlap conditions
        var noOverlap = thisEnd < other.From || otherEnd < From;

        return !noOverlap;
    }

   
    public bool IsWithin(DateRange other)
    {
        if (other == null) return false;

        var thisEnd = To ?? DateOnly.MaxValue;
        var otherEnd = other.To ?? DateOnly.MaxValue;

        return From >= other.From && thisEnd <= otherEnd;
    }

    public DateRange? GetOverlap(DateRange other)
    {
        if (!OverlapsWith(other)) return null;

        var overlapStart = From > other.From ? From : other.From;
        var thisEnd = To ?? DateOnly.MaxValue;
        var otherEnd = other.To ?? DateOnly.MaxValue;
        var overlapEnd = thisEnd < otherEnd ? thisEnd : otherEnd;

        // If overlap end is max value, it means open-ended
        var endDate = overlapEnd == DateOnly.MaxValue ? null : (DateOnly?)overlapEnd;

        return Create(overlapStart, endDate);
    }

  
    public bool IsValidAgainst(IEnumerable<DateRange> existingRanges)
    {
        return existingRanges?.All(range => !OverlapsWith(range)) ?? true;
    }

    public int DaysCount
    {
        get
        {
            var endDate = To ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)); 
            return endDate.DayNumber - From.DayNumber + 1;
        }
    }

    public bool IsOpenEnded => !To.HasValue;

  
    public bool HasExpired => To.HasValue && To.Value < DateOnly.FromDateTime(DateTime.UtcNow);

  
    public bool IsCurrent => Contains(DateOnly.FromDateTime(DateTime.UtcNow));

  
    public bool IsFuture => From > DateOnly.FromDateTime(DateTime.UtcNow);

    public override string ToString()
    {
        return To.HasValue ? $"{From:yyyy-MM-dd} to {To:yyyy-MM-dd}" : $"From {From:yyyy-MM-dd}";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}