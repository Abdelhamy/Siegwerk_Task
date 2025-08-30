
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.UseCases.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierHandler : IUpdateSupplierHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<UpdateSupplierHandler> _logger;

    public UpdateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<UpdateSupplierHandler> logger)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateSupplierResponse?> HandleAsync(UpdateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating supplier: {SupplierId}", command.Id);

            var supplier = await _supplierRepository.GetByIdAsync(command.Id, cancellationToken);
            if (supplier == null)
            {
                _logger.LogWarning("Supplier not found for update: {SupplierId}", command.Id);
                return null;
            }

            // Update domain entity
            var leadTime = LeadTime.Create(command.LeadTimeDays);
            supplier.UpdateDetails(command.Name, command.Country, command.Preferred, leadTime);

            // Persist changes
            _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated supplier: {SupplierName} ({SupplierId})", 
                supplier.Name, supplier.Id);

            // Return DTO
            return new UpdateSupplierResponse(
                supplier.Id,
                supplier.Name,
                supplier.Country,
                supplier.Preferred,
                supplier.LeadTime.Days
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier: {SupplierId}", command.Id);
            throw;
        }
    }
}