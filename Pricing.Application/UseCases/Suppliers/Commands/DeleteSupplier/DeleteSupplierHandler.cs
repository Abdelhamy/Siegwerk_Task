using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;

namespace Pricing.Application.UseCases.Suppliers.Commands.DeleteSupplier;

public class DeleteSupplierHandler : IDeleteSupplierHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<DeleteSupplierHandler> _logger;

    public DeleteSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<DeleteSupplierHandler> logger)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeleteSupplierResponse> HandleAsync(DeleteSupplierCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting supplier: {SupplierId}", command.Id);

            var supplier = await _supplierRepository.GetByIdAsync(command.Id, cancellationToken);
            if (supplier is null)
            {
                _logger.LogWarning("Supplier not found for deletion: {SupplierId}", command.Id);
                return new DeleteSupplierResponse(false);
            }
            var supplierName = supplier.Name;
            _supplierRepository.Delete(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted supplier: {SupplierName} ({SupplierId})", 
                supplierName, command.Id);

            return new DeleteSupplierResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier: {SupplierId}", command.Id);
            throw;
        }
    }
}