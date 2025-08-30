namespace Pricing.Application.UseCases.Suppliers.Commands.DeleteSupplier;

public record DeleteSupplierCommand(int Id);

public record DeleteSupplierResponse(bool Success);