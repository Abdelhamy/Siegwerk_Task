namespace Pricing.Application.UseCases.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand(
    int Id,
    string Name,
    string? Country,
    bool Preferred,
    int LeadTimeDays
);

public record UpdateSupplierResponse(
    int Id,
    string Name,
    string? Country,
    bool Preferred,
    int LeadTimeDays
);