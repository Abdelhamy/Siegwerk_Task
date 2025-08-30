using Pricing.Application.Common.Models;
using Pricing.Application.Contracts;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.Interfaces.Repositories;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default );
    
    Task<PagedResult<SupplierDto>> GetPagedAsync(
        PaginationRequest pagination,
        SortRequest? sort = null,
        string? searchTerm = null,
        string? name = null,
        string? country = null,
        bool? preferred = null,
        int? minLeadTime = null,
        int? maxLeadTime = null,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<PriceCandidateDto>> GetValidPriceCandidatesAsync(
        Sku? sku = null, 
        Quantity? quantity = null, 
        DateOnly? date = null, 
        string? currency = null, 
        int? supplierId = null, 
        CancellationToken cancellationToken = default);
    
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    void Update(Supplier supplier);
    void Delete(Supplier supplier);
}