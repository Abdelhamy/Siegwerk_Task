using Pricing.Domain.Common;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Entities;

public class Supplier : AggregateRoot<int>
{
    public string Name { get; private set; }
    public string? Country { get; private set; }
    public bool Preferred { get; private set; }
    public LeadTime LeadTime { get; private set; }
    
    private readonly List<PriceListEntry> _priceListEntries = new();
    public IReadOnlyCollection<PriceListEntry> PriceListEntries => _priceListEntries.AsReadOnly();

    private Supplier() { }

    private Supplier(int id, string name, string? country, bool preferred, LeadTime leadTime) : base(id)
    {
        Name = name;
        Country = country;
        Preferred = preferred;
        LeadTime = leadTime;
    }

    public static Supplier Create(string name, string? country = null, bool preferred = false, LeadTime? leadTime = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name cannot be null or empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Supplier name cannot exceed 200 characters", nameof(name));

        return new Supplier(
            0,
            name.Trim(),
            country?.Trim(),
            preferred,
            leadTime ?? LeadTime.Zero);
    }

    public void UpdateDetails(string name, string? country, bool preferred, LeadTime leadTime)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name cannot be null or empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Supplier name cannot exceed 200 characters", nameof(name));

        Name = name.Trim();
        Country = country?.Trim();
        Preferred = preferred;
        LeadTime = leadTime;
    }

    public void MarkAsPreferred()
    {
        Preferred = true;
    }

    public void UnmarkAsPreferred()
    {
        Preferred = false;
    }

    public void UpdateLeadTime(LeadTime leadTime)
    {
        LeadTime = leadTime;
    }

    public PriceListEntry AddPriceEntry(string sku, DateRange validityPeriod, Money price, Quantity minimumQuantity)
    {
        var priceEntry = PriceListEntry.Create(Id, sku, validityPeriod, price, minimumQuantity);
        _priceListEntries.Add(priceEntry);
        
        return priceEntry;
    }

    public void RemovePriceEntry(int priceEntryId)
    {
        var entry = _priceListEntries.FirstOrDefault(e => e.Id == priceEntryId);
        if (!ReferenceEquals(entry, null))
        {
            _priceListEntries.Remove(entry);
        }
    }

    public IEnumerable<PriceListEntry> GetActivePriceEntries(DateOnly date)
    {
        return _priceListEntries.Where(entry => entry.IsValidOn(date));
    }

    public IEnumerable<PriceListEntry> GetPriceEntriesForSku(string sku)
    {
        return _priceListEntries.Where(entry => entry.Sku == sku);
    }
}