
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.UseCases.Suppliers.Commands.CreateSupplier;

public class CreateSupplierHandler : ICreateSupplierHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<CreateSupplierHandler> _logger;

    public CreateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<CreateSupplierHandler> logger)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateSupplierResponse> HandleAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating new supplier: {SupplierName}", command.Name);

            var leadTime = LeadTime.Create(command.LeadTimeDays);
            var supplier = Supplier.Create(command.Name, command.Country, command.Preferred, leadTime);

            await _supplierRepository.AddAsync(supplier, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new supplier: {SupplierName} ({SupplierId})", 
                supplier.Name, supplier.Id);

            return new CreateSupplierResponse(
                supplier.Id,
                supplier.Name,
                supplier.Country,
                supplier.Preferred,
                supplier.LeadTime.Days
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier: {SupplierName}", command.Name);
            throw;
        }
    }
}