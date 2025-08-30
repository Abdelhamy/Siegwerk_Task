using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using System.Threading.Tasks;

namespace Pricing.Application.Interfaces.Repositories;

public interface IPriceListEntryRepository
{
    Task AddRangeAsync(IEnumerable<PriceListEntry> entries, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceListEntry>> GetBySupplierAndSkuAsync(int supplierId, Sku sku, CancellationToken cancellationToken);
    Task<IReadOnlyList<PriceListEntry>> GetOverlappingEntries(
        int supplierId,
        string sku,
        DateOnly from,
        DateOnly? to,
        CancellationToken cancellationToken = default);
}
