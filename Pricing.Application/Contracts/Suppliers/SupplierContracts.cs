namespace Pricing.Application.Contracts;
public record SupplierModel(
    int Id,
    string Name,
    string? Country,
    bool Preferred,
    int LeadTimeDays);

public record SupplierDto(
    int Id,
    string Name,
    string Country,
    bool Preferred,
    int LeadTimeDays
);

