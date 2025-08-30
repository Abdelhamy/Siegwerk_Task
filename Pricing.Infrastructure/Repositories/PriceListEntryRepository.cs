using Microsoft.EntityFrameworkCore;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Infrastructure.Repositories;

public class PriceListEntryRepository : IPriceListEntryRepository
{
    private readonly PricingDbContext _context;

    public PriceListEntryRepository(PricingDbContext context)
    {
        _context = context;
    }


    public async Task<IReadOnlyList<PriceListEntry>> GetOverlappingEntries(
        int supplierId, 
        string sku, 
        DateOnly from, 
        DateOnly? to, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.PriceListEntries
            .Include(p => p.Supplier)
            .Where(p => p.SupplierId == supplierId && p.Sku.Value == sku);

        // Check for overlapping date ranges
        var endDate = to ?? DateOnly.MaxValue;
        
        query = query.Where(p => 
            p.ValidityPeriod.From <= endDate && 
            (p.ValidityPeriod.To == null || p.ValidityPeriod.To >= from));

        return await query.ToListAsync(cancellationToken);
    }


    public async Task AddRangeAsync(IEnumerable<PriceListEntry> entries, CancellationToken cancellationToken = default)
    {
        await _context.PriceListEntries.AddRangeAsync(entries, cancellationToken);
    }

    public async Task<IReadOnlyList<PriceListEntry>> GetBySupplierAndSkuAsync(int supplierId, Sku sku, CancellationToken cancellationToken)
    {
        return await _context.PriceListEntries
            .Include(p => p.Supplier)
            .Where(p => p.SupplierId == supplierId && p.Sku == sku)
            .ToListAsync(cancellationToken);
    }
}