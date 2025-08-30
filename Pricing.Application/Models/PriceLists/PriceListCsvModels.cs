namespace Pricing.Application.Models.PriceLists;

public class PriceListCsvRow
{
    public int RowNumber { get; set; }
    public string SupplierId { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string ValidFrom { get; set; } = string.Empty;
    public string ValidTo { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string PricePerUom { get; set; } = string.Empty;
    public string MinQty { get; set; } = string.Empty;
}

public class PriceListImportResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int SupplierId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public decimal PriceAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int MinQty { get; set; }
    public int RowNumber { get; set; }
}

public class CsvValidationSummary
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public List<string> GlobalErrors { get; set; } = new();
    public List<PriceListImportResult> Results { get; set; } = new();
    public List<OverlapError> OverlapErrors { get; set; } = new();
}

public class OverlapError
{
    public int Row1 { get; set; }
    public int Row2 { get; set; }
    public int SupplierId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}