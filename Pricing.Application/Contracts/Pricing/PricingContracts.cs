namespace Pricing.Application.Contracts;

public record BestPriceRequest(string Sku, int Qty, string Currency, DateOnly OnDate);

public record BestPriceResponse(
    string Sku,
    int Qty,
    string Currency,
    decimal UnitPrice,
    decimal Total,
    int SupplierId,
    string SupplierName,
    bool SupplierPreferred,
    int SupplierLeadTimeDays,
    string Reason);

public record PriceCandidateDto(
    int Id,
    int SupplierId,
    string SupplierName,
    bool SupplierPreferred,
    int SupplierLeadTimeDays,
    string Sku,
    decimal PricePerUom,
    string Currency,
    int MinQty,
    DateOnly ValidFrom,
    DateOnly? ValidTo);

// Query Request Models for endpoints
public record BestPriceQueryRequest
{
    public string Sku { get; init; } = string.Empty;
    public int Qty { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
}

public record ListPricesQueryRequest
{
    public string? Sku { get; init; }
    public string? ValidOn { get; init; }
    public string? Currency { get; init; }
    public int? SupplierId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; }
    public string SortDirection { get; init; } = "asc";
}

// Currency DTO
public record CurrencyDto(string Code, string Name);