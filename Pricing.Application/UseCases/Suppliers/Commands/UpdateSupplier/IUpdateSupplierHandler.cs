namespace Pricing.Application.UseCases.Suppliers.Commands.UpdateSupplier;

public interface IUpdateSupplierHandler
{
    Task<UpdateSupplierResponse?> HandleAsync(UpdateSupplierCommand command, CancellationToken cancellationToken = default);
}