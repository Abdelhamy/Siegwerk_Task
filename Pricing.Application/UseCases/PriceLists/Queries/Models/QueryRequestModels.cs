
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