using Microsoft.EntityFrameworkCore;
using Pricing.Application.Common.Models;
using Pricing.Application.Contracts;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Infrastructure.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly PricingDbContext _context;

    public SupplierRepository(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .Include(s => s.PriceListEntries)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PriceCandidateDto>> GetValidPriceCandidatesAsync(
        Sku? sku = null, 
        Quantity? quantity = null, 
        DateOnly? date = null, 
        string? currency = null, 
        int? supplierId = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.PriceListEntries
            .Include(p => p.Supplier)
            .AsNoTracking();

        if (sku is not null)
        {
            query = query.Where(p => p.Sku == sku);
        }

        if (quantity is not null)
        {
            query = query.Where(p => p.MinimumQuantity.Value <= quantity.Value);
        }

        if (date is not null)
        {
            query = query.Where(p => p.ValidityPeriod.From <= date &&
                                   (p.ValidityPeriod.To == null || p.ValidityPeriod.To >= date));
        }

        if (!string.IsNullOrEmpty(currency))
        {
            query = query.Where(p => p.Price.Currency.Code == currency);
        }

        if (supplierId is not null)
        {
            query = query.Where(p => p.SupplierId == supplierId);
        }

        var candidates = await query
            .Select(p => new PriceCandidateDto(
                p.Id,
                p.SupplierId,
                p.Supplier!.Name,
                p.Supplier.Preferred,
                p.Supplier.LeadTime.Days,
                p.Sku.Value,
                p.Price.Amount,
                p.Price.Currency.Code,
                p.MinimumQuantity.Value,
                p.ValidityPeriod.From,
                p.ValidityPeriod.To))
            .ToListAsync(cancellationToken);

        return candidates.AsReadOnly();
    }

    public async Task<PagedResult<SupplierDto>> GetPagedAsync(
        PaginationRequest pagination,
        SortRequest? sort = null,
        string? searchTerm = null,
        string? name = null,
        string? country = null,
        bool? preferred = null,
        int? minLeadTime = null,
        int? maxLeadTime = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPagination = pagination.Normalize();

        var baseQuery = _context.Suppliers.AsNoTracking();
        var filteredQuery = ApplyFilters(baseQuery, searchTerm, name, country, preferred, minLeadTime, maxLeadTime);

        var countTask = filteredQuery.CountAsync(cancellationToken);

        var sortedQuery = ApplySort(filteredQuery, sort);

        var itemsTask = sortedQuery
            .Skip(normalizedPagination.Skip)
            .Take(normalizedPagination.Take)
            .Select(s => new SupplierDto(
                s.Id,
                s.Name,
                s.Country,
                s.Preferred,
                s.LeadTime.Days
            ))
            .ToListAsync(cancellationToken);

        await Task.WhenAll(countTask, itemsTask);

        var totalCount = await countTask;
        var items = await itemsTask;

        return PagedResult<SupplierDto>.Create(
            items,
            totalCount,
            normalizedPagination.Page,
            normalizedPagination.PageSize);
    }

    public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await _context.Suppliers.AddAsync(supplier, cancellationToken);
    }

    public void Update(Supplier supplier)
    {
        _context.Suppliers.Update(supplier);
    }

    public void Delete(Supplier supplier)
    {
        _context.Suppliers.Remove(supplier);
    }

    private static IQueryable<Supplier> ApplyFilters(
        IQueryable<Supplier> query,
        string? searchTerm,
        string? name,
        string? country,
        bool? preferred,
        int? minLeadTime,
        int? maxLeadTime)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(searchTermLower) ||
                (s.Country != null && s.Country.ToLower().Contains(searchTermLower)));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(s => s.Name.ToLower().Contains(name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(s => s.Country != null && s.Country.ToLower().Contains(country.ToLower()));
        }

        if (preferred.HasValue)
        {
            query = query.Where(s => s.Preferred == preferred.Value);
        }

        if (minLeadTime.HasValue)
        {
            query = query.Where(s => s.LeadTime.Days >= minLeadTime.Value);
        }

        if (maxLeadTime.HasValue)
        {
            query = query.Where(s => s.LeadTime.Days <= maxLeadTime.Value);
        }

        return query;
    }

    private static IQueryable<Supplier> ApplySort(IQueryable<Supplier> query, SortRequest? sort)
    {
        if (sort == null || string.IsNullOrWhiteSpace(sort.SortBy))
            return query.OrderBy(s => s.Name);

        return sort.SortBy.ToLowerInvariant() switch
        {
            "id" => sort.IsDescending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
            "name" => sort.IsDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "country" => sort.IsDescending ? query.OrderByDescending(s => s.Country) : query.OrderBy(s => s.Country),
            "preferred" => sort.IsDescending ? query.OrderByDescending(s => s.Preferred) : query.OrderBy(s => s.Preferred),
            "leadtimedays" => sort.IsDescending ? query.OrderByDescending(s => s.LeadTime.Days) : query.OrderBy(s => s.LeadTime.Days),
            _ => query.OrderBy(s => s.Name)
        };
    }

}