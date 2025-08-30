using Pricing.Domain.Common;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Entities;

public class PriceListEntry : Entity<int>
{
    public int SupplierId { get; private set; }
    public Sku Sku { get; private set; }
    public DateRange ValidityPeriod { get; private set; }
    public Money Price { get; private set; }
    public Quantity MinimumQuantity { get; private set; }
    
    public virtual Supplier? Supplier { get; private set; }

    private PriceListEntry() { } // EF Constructor

    private PriceListEntry(
        int id,
        int supplierId, 
        Sku sku, 
        DateRange validityPeriod, 
        Money price, 
        Quantity minimumQuantity) : base(id)
    {
        SupplierId = supplierId;
        Sku = sku;
        ValidityPeriod = validityPeriod;
        Price = price;
        MinimumQuantity = minimumQuantity;
    }

    public static PriceListEntry Create(
        int supplierId,
        string sku,
        DateRange validityPeriod,
        Money price,
        Quantity minimumQuantity)
    {
        return new PriceListEntry(
            0, // Let database generate ID
            supplierId,
            Sku.Create(sku),
            validityPeriod,
            price,
            minimumQuantity);
    }

    
    public bool IsValidOn(DateOnly date) => ValidityPeriod.Contains(date);

    
    public bool SupportsQuantity(Quantity quantity) => quantity.MeetsMinimum(MinimumQuantity);

    
    public bool IsApplicableFor(Quantity quantity, DateOnly date)
    {
        return IsValidOn(date) && SupportsQuantity(quantity);
    }

    
    public bool IsActiveNow => ValidityPeriod.IsCurrent;

    
    public bool HasExpired => ValidityPeriod.HasExpired;

    
    public bool IsFuture => ValidityPeriod.IsFuture;

    
    public bool ValidateNoOverlapWith(IEnumerable<PriceListEntry> existingEntries)
    {
        var sameSupplierSkuEntries = existingEntries?
            .Where(entry => entry.SupplierId == SupplierId && 
                           entry.Sku == Sku && 
                           entry.Id != Id) // Exclude self when updating
            .Select(entry => entry.ValidityPeriod);

        return ValidityPeriod.IsValidAgainst(sameSupplierSkuEntries ?? Enumerable.Empty<DateRange>());
    }

    
    public IEnumerable<PriceListEntry> GetOverlappingEntries(IEnumerable<PriceListEntry> existingEntries)
    {
        return existingEntries?
            .Where(entry => entry.SupplierId == SupplierId && 
                           entry.Sku == Sku && 
                           entry.Id != Id &&
                           ValidityPeriod.OverlapsWith(entry.ValidityPeriod)) ?? 
               Enumerable.Empty<PriceListEntry>();
    }

    
    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Currency != Price.Currency)
            throw new InvalidOperationException("Cannot change currency of existing price entry");

        if (newPrice.Amount <= 0)
            throw new ArgumentException("Price must be greater than zero");

        Price = newPrice;
    }

    
    public void UpdateValidityPeriod(DateRange newValidityPeriod, IEnumerable<PriceListEntry>? existingEntries = null)
    {
        var oldPeriod = ValidityPeriod;
        ValidityPeriod = newValidityPeriod;

        // Validate no overlaps if existing entries provided
        if (existingEntries != null && !ValidateNoOverlapWith(existingEntries))
        {
            ValidityPeriod = oldPeriod; // Rollback
            throw new InvalidOperationException("New validity period overlaps with existing price entries for the same supplier and SKU");
        }
    }

    
    public void UpdateMinimumQuantity(Quantity newMinimumQuantity)
    {
        MinimumQuantity = newMinimumQuantity;
    }

    
    public Money CalculateTotal(Quantity quantity)
    {
        if (!SupportsQuantity(quantity))
            throw new InvalidOperationException($"Quantity {quantity} is below minimum quantity {MinimumQuantity}");

        return Price * quantity.Value;
    }

 
    public int GetPriorityScore()
    {
        var score = 0;
        
        if (IsFuture) score += 1000;
        
        if (ValidityPeriod.IsOpenEnded) score -= 100;
        else score += ValidityPeriod.DaysCount > 365 ? -50 : 0;
        
        return score;
    }

   
    public ValidationResult Validate(IEnumerable<PriceListEntry>? existingEntries = null)
    {
        var errors = new List<string>();

        // Price validation
        if (Price.Amount <= 0)
            errors.Add("Price must be greater than zero");

        // Minimum quantity validation
        if (MinimumQuantity.Value <= 0)
            errors.Add("Minimum quantity must be greater than zero");

        // Date validation
        if (ValidityPeriod.HasExpired && ValidityPeriod.From < DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)))
            errors.Add("Validity period is too far in the past");

        // Overlap validation
        if (existingEntries != null && !ValidateNoOverlapWith(existingEntries))
        {
            var overlappingEntries = GetOverlappingEntries(existingEntries);
            errors.Add($"Price entry overlaps with {overlappingEntries.Count()} existing entries");
        }

        return new ValidationResult(errors.Count == 0, errors);
    }

    // For backward compatibility with existing code
    public DateOnly ValidFrom => ValidityPeriod.From;
    public DateOnly? ValidTo => ValidityPeriod.To;
    public string Currency => Price.Currency.Code;
    public decimal PricePerUom => Price.Amount;
    public int MinQty => MinimumQuantity.Value;
}


public record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);
