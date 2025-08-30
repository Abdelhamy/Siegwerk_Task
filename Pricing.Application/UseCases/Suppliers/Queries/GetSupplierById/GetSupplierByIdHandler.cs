using Pricing.Application.Interfaces.Repositories;
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Contracts;

namespace Pricing.Application.UseCases.Suppliers.Queries.GetSupplierById;

public class GetSupplierByIdHandler : IGetSupplierByIdHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAppLogger<GetSupplierByIdHandler> _logger;

    public GetSupplierByIdHandler(
        ISupplierRepository supplierRepository,
        IAppLogger<GetSupplierByIdHandler> logger)
    {
        _supplierRepository = supplierRepository;
        _logger = logger;
    }

    public async Task<GetSupplierByIdResponse> HandleAsync(
        GetSupplierByIdQuery query, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving supplier by ID: {SupplierId}", query.SupplierId);

            var supplier = await _supplierRepository.GetByIdAsync(query.SupplierId, cancellationToken);
            
            if (supplier == null)
            {
                _logger.LogWarning("Supplier not found: {SupplierId}", query.SupplierId);
                return new GetSupplierByIdResponse(null);
            }

            var supplierDto = new SupplierDto(
                supplier.Id,
                supplier.Name,
                supplier.Country,
                supplier.Preferred,
                supplier.LeadTime.Days
            );

            return new GetSupplierByIdResponse(supplierDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier by ID: {SupplierId}", query.SupplierId);
            throw;
        }
    }
}