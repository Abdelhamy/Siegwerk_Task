namespace Pricing.Application.Common.Models;

public abstract record FilterRequest
{
    public string? Search { get; init; }

    public virtual bool IsValid() => true;
}

public record DateRangeFilter
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public bool IsValid()
    {
        if (From.HasValue && To.HasValue)
            return From.Value <= To.Value;
        
        return true;
    }

    public bool Contains(DateOnly date)
    {
        if (From.HasValue && date < From.Value)
            return false;
        
        if (To.HasValue && date > To.Value)
            return false;
        
        return true;
    }
}

public record NumericRangeFilter<T> where T : struct, IComparable<T>
{
    public T? Min { get; init; }

    public T? Max { get; init; }

    public bool IsValid()
    {
        if (Min.HasValue && Max.HasValue)
            return Min.Value.CompareTo(Max.Value) <= 0;
        
        return true;
    }

    public bool Contains(T value)
    {
        if (Min.HasValue && value.CompareTo(Min.Value) < 0)
            return false;
        
        if (Max.HasValue && value.CompareTo(Max.Value) > 0)
            return false;
        
        return true;
    }
}