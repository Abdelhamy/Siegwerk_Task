using Pricing.Application.Common.Models;
using Pricing.Application.Contracts;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(Sku sku, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    void Delete(Product product);
    
    Task<PagedResult<ProductDto>> GetPagedAsync(
        PaginationRequest pagination,
        SortRequest? sort = null,
        string? searchTerm = null,
        string? name = null,
        string? sku = null,
        string? unitOfMeasure = null,
        string? hazardClass = null,
        bool? isHazardous = null,
        CancellationToken cancellationToken = default);
}