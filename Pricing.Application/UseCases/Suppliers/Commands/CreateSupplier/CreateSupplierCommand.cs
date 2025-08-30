namespace Pricing.Application.UseCases.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand(
    string Name,
    string? Country,
    bool Preferred,
    int LeadTimeDays
);

public record CreateSupplierResponse(
    int Id,
    string Name,
    string? Country,
    bool Preferred,
    int LeadTimeDays
);