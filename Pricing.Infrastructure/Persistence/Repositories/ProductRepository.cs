using Microsoft.EntityFrameworkCore;
using Pricing.Application.Common.Models;
using Pricing.Application.Contracts;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PricingDbContext _context;

    public ProductRepository(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(Sku sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(
        PaginationRequest pagination,
        SortRequest? sort = null,
        string? searchTerm = null,
        string? name = null,
        string? sku = null,
        string? unitOfMeasure = null,
        string? hazardClass = null,
        bool? isHazardous = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPagination = pagination.Normalize();
        
        var baseQuery = _context.Products.AsNoTracking();
        var filteredQuery = ApplyFilters(baseQuery, searchTerm, name, sku, unitOfMeasure, hazardClass, isHazardous);
        
        var countTask = filteredQuery.CountAsync(cancellationToken);
        
        var sortedQuery = ApplySort(filteredQuery, sort);
        var itemsTask = sortedQuery
            .Skip(normalizedPagination.Skip)
            .Take(normalizedPagination.Take)
            .Select(p => new ProductDto(
                p.Id,
                p.Sku.Value,
                p.Name,
                p.UnitOfMeasure,
                p.HazardClass,
                !string.IsNullOrWhiteSpace(p.HazardClass)
            ))
            .ToListAsync(cancellationToken);
        
        await Task.WhenAll(countTask, itemsTask);
        
        var totalCount = await countTask;
        var items = await itemsTask;
        
        return PagedResult<ProductDto>.Create(
            items,
            totalCount,
            normalizedPagination.Page,
            normalizedPagination.PageSize);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Delete(Product product)
    {
        _context.Products.Remove(product);
    }

    private static IQueryable<Product> ApplyFilters(
        IQueryable<Product> query,
        string? searchTerm,
        string? name,
        string? sku,
        string? unitOfMeasure,
        string? hazardClass,
        bool? isHazardous)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTermLower) ||
                p.Sku.Value.ToLower().Contains(searchTermLower) ||
                p.UnitOfMeasure.ToLower().Contains(searchTermLower) ||
                (p.HazardClass != null && p.HazardClass.ToLower().Contains(searchTermLower)));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            query = query.Where(p => p.Sku.Value.ToLower().Contains(sku.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(unitOfMeasure))
        {
            query = query.Where(p => p.UnitOfMeasure.ToLower().Contains(unitOfMeasure.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(hazardClass))
        {
            query = query.Where(p => p.HazardClass != null && p.HazardClass.ToLower().Contains(hazardClass.ToLower()));
        }

        if (isHazardous.HasValue)
        {
            if (isHazardous.Value)
            {
                query = query.Where(p => !string.IsNullOrWhiteSpace(p.HazardClass));
            }
            else
            {
                query = query.Where(p => string.IsNullOrWhiteSpace(p.HazardClass));
            }
        }

        return query;
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, SortRequest? sort)
    {
        if (sort == null || string.IsNullOrWhiteSpace(sort.SortBy))
            return query.OrderBy(p => p.Name);

        return sort.SortBy.ToLowerInvariant() switch
        {
            "id" => sort.IsDescending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
            "name" => sort.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "sku" => sort.IsDescending ? query.OrderByDescending(p => p.Sku.Value) : query.OrderBy(p => p.Sku.Value),
            "unitofmeasure" => sort.IsDescending ? query.OrderByDescending(p => p.UnitOfMeasure) : query.OrderBy(p => p.UnitOfMeasure),
            "hazardclass" => sort.IsDescending ? query.OrderByDescending(p => p.HazardClass) : query.OrderBy(p => p.HazardClass),
            "ishazardous" => sort.IsDescending 
                ? query.OrderByDescending(p => !string.IsNullOrWhiteSpace(p.HazardClass))
                : query.OrderBy(p => !string.IsNullOrWhiteSpace(p.HazardClass)),
            _ => query.OrderBy(p => p.Name)
        };
    }
}