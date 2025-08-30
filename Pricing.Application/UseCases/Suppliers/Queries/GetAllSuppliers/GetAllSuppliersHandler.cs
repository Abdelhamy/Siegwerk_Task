
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Application.Common.Models;

namespace Pricing.Application.UseCases.Suppliers.Queries;

public class GetSuppliersPagedHandler : IGetSuppliersPagedHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAppLogger<GetSuppliersPagedHandler> _logger;

    public GetSuppliersPagedHandler(
        ISupplierRepository supplierRepository,
        IAppLogger<GetSuppliersPagedHandler> logger)
    {
        _supplierRepository = supplierRepository;
        _logger = logger;
    }

    public async Task<GetSuppliersPagedResponse> HandleAsync(GetSuppliersPagedQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving paginated suppliers with filters");

            var allowedSortFields = new[] { "id", "name", "country", "preferred", "leadtimedays" };
            if (query.Sort != null && !query.Sort.IsValid(allowedSortFields))
            {
                throw new ArgumentException($"Invalid sort field. Allowed fields: {string.Join(", ", allowedSortFields)}");
            }

            var result = await _supplierRepository.GetPagedAsync(
                query.Pagination,
                query.Sort,
                query.SearchTerm,
                query.Name,
                query.Country,
                query.Preferred,
                query.MinLeadTime,
                query.MaxLeadTime,
                cancellationToken);

          
            _logger.LogDebug("Retrieved {Count} suppliers from page {Page}", result.Count, result.Page);

            return new GetSuppliersPagedResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated suppliers");
            throw;
        }
    }
}