using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Application.Models.PriceLists;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Pricing.Application.UseCases.PriceLists.Commands.ImportPricesFromCsv;

public class ImportPricesFromCsvHandler : IImportPricesFromCsvHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPriceListEntryRepository _priceListEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<ImportPricesFromCsvHandler> _logger;

    public ImportPricesFromCsvHandler(
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IPriceListEntryRepository priceListEntryRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<ImportPricesFromCsvHandler> logger)
    {
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _priceListEntryRepository = priceListEntryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ImportPricesFromCsvResponse> HandleAsync(
        ImportPricesFromCsvCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting CSV import for file: {FileName}", command.FileName);

            // Parse CSV
            var csvRows = await ParseCsvAsync(command.CsvStream);

            if (csvRows.Count == 0)
            {
                return new ImportPricesFromCsvResponse(
                    false,
                    "CSV file is empty or contains no valid data rows.",
                    new CsvValidationSummary { TotalRows = 0 });
            }

            var validationSummary = await ValidateCsvDataAsync(csvRows, cancellationToken);

            if (validationSummary.ValidRows == 0)
            {
                return new ImportPricesFromCsvResponse(
                    false,
                    "No valid rows found in CSV file.",
                    validationSummary);
            }

            // check ovarlaping with data base

            var importedCount = 0;
            if (validationSummary.ValidRows > 0)
            {
                importedCount = await ImportValidEntriesAsync(validationSummary, cancellationToken);
            }

            var success = importedCount > 0;
            var message = success
                ? $"Successfully imported {importedCount} price entries. {validationSummary.InvalidRows} rows had errors."
                : "Import failed. Please check the validation errors.";

            _logger.LogInformation("CSV import completed: {ImportedCount} entries imported, {InvalidCount} errors",
                importedCount, validationSummary.InvalidRows);

            return new ImportPricesFromCsvResponse(success, message, validationSummary, importedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CSV import for file: {FileName}", command.FileName);
            return new ImportPricesFromCsvResponse(
                false,
                $"Import failed due to an error: {ex.Message}",
                new CsvValidationSummary());
        }
    }

    private async Task<List<PriceListCsvRow>> ParseCsvAsync(Stream csvStream)
    {
        var rows = new List<PriceListCsvRow>();

        using var reader = new StreamReader(csvStream, Encoding.UTF8);

        // Skip header row
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return rows;

        int rowNumber = 1; // Start from 1 (header is row 0)

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            rowNumber++;

            var fields = ParseCsvLine(line);
            if (fields.Length >= 7) // Minimum required fields
            {
                rows.Add(new PriceListCsvRow
                {
                    RowNumber = rowNumber,
                    SupplierId = fields[0]?.Trim() ?? string.Empty,
                    Sku = fields[1]?.Trim() ?? string.Empty,
                    ValidFrom = fields[2]?.Trim() ?? string.Empty,
                    ValidTo = fields[3]?.Trim() ?? string.Empty,
                    Currency = fields[4]?.Trim() ?? string.Empty,
                    PricePerUom = fields[5]?.Trim() ?? string.Empty,
                    MinQty = fields[6]?.Trim() ?? string.Empty
                });
            }
        }

        return rows;
    }

    private string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }

    private async Task<CsvValidationSummary> ValidateCsvDataAsync(
        List<PriceListCsvRow> csvRows,
        CancellationToken cancellationToken)
    {
        var summary = new CsvValidationSummary
        {
            TotalRows = csvRows.Count
        };

        var validResults = new List<PriceListImportResult>();

        foreach (var row in csvRows)
        {
            var result = await ValidateRowAsync(row, cancellationToken);
            summary.Results.Add(result);

            if (result.IsValid)
            {
                validResults.Add(result);
                summary.ValidRows++;
            }
            else
            {
                summary.InvalidRows++;
            }
        }

        summary.OverlapErrors = await DetectOverlappingPeriods(validResults);

        // Mark overlapping entries as invalid
        foreach (var overlapError in summary.OverlapErrors)
        {
            var affectedResults = summary.Results.Where(r =>
                r.RowNumber == overlapError.Row1 || r.RowNumber == overlapError.Row2).ToList();

            foreach (var result in affectedResults)
            {
                if (result.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.Add(overlapError.Message);
                    summary.ValidRows--;
                    summary.InvalidRows++;
                }
            }
        }

        return summary;
    }

    private async Task<PriceListImportResult> ValidateRowAsync(
        PriceListCsvRow row,
        CancellationToken cancellationToken)
    {
        var result = new PriceListImportResult
        {
            RowNumber = row.RowNumber,
            IsValid = true
        };

        // Validate SupplierId
        if (!int.TryParse(row.SupplierId, out var supplierId))
        {
            result.Errors.Add("Invalid supplier ID format");
            result.IsValid = false;
        }
        else
        {
            result.SupplierId = supplierId;

            // Check if supplier exists
            var supplierExists = await _supplierRepository.ExistsAsync(supplierId, cancellationToken);
            if (!supplierExists)
            {
                result.Errors.Add($"Supplier with ID {supplierId} does not exist");
                result.IsValid = false;
            }
        }

        // Validate SKU
        if (string.IsNullOrWhiteSpace(row.Sku))
        {
            result.Errors.Add("SKU is required");
            result.IsValid = false;
        }
        else
        {
            result.Sku = row.Sku;

            // Check if product exists
            try
            {
                var sku = Sku.Create(row.Sku);
                var productExists = await _productRepository.ExistsBySkuAsync(sku, cancellationToken);
                if (!productExists)
                {
                    result.Warnings.Add($"Product with SKU {row.Sku} does not exist in the system");
                }
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add($"Invalid SKU format: {ex.Message}");
                result.IsValid = false;
            }
        }

        // Validate ValidFrom
        if (!DateOnly.TryParseExact(row.ValidFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var validFrom))
        {
            result.Errors.Add("Invalid ValidFrom date format. Expected yyyy-MM-dd");
            result.IsValid = false;
        }
        else
        {
            result.ValidFrom = validFrom;
        }

        // Validate ValidTo (optional)
        if (!string.IsNullOrWhiteSpace(row.ValidTo))
        {
            if (!DateOnly.TryParseExact(row.ValidTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var validTo))
            {
                result.Errors.Add("Invalid ValidTo date format. Expected yyyy-MM-dd");
                result.IsValid = false;
            }
            else
            {
                result.ValidTo = validTo;

                if (result.IsValid && validTo <= validFrom)
                {
                    result.Errors.Add("ValidTo date must be after ValidFrom date");
                    result.IsValid = false;
                }
            }
        }

        // Validate Currency
        if (string.IsNullOrWhiteSpace(row.Currency))
        {
            result.Errors.Add("Currency is required");
            result.IsValid = false;
        }
        else
        {
            try
            {
                var currency = Currency.FromCode(row.Currency);
                result.CurrencyCode = currency.Code;
            }
            catch (ArgumentException)
            {
                result.Errors.Add($"Unsupported currency code: {row.Currency}");
                result.IsValid = false;
            }
        }

        // Validate PricePerUom
        if (!decimal.TryParse(row.PricePerUom, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            result.Errors.Add("Invalid price format or price must be greater than 0");
            result.IsValid = false;
        }
        else
        {
            result.PriceAmount = price;
        }

        // Validate MinQty
        if (!int.TryParse(row.MinQty, out var minQty) || minQty <= 0)
        {
            result.Errors.Add("Invalid minimum quantity format or quantity must be greater than 0");
            result.IsValid = false;
        }
        else
        {
            result.MinQty = minQty;
        }

        return result;
    }

    private async Task<List<OverlapError>> DetectOverlappingPeriods(List<PriceListImportResult> validResults)
    {
        var overlapErrors = new List<OverlapError>();

        for (int i = 0; i < validResults.Count; i++)
        {
            for (int j = i + 1; j < validResults.Count; j++)
            {
                var result1 = validResults[i];
                var result2 = validResults[j];

                // Check if same supplier and SKU
                if (result1.SupplierId == result2.SupplierId && result1.Sku == result2.Sku)
                {
                    // Check for date overlap
                    var range1End = result1.ValidTo ?? DateOnly.MaxValue;
                    var range2End = result2.ValidTo ?? DateOnly.MaxValue;


                    bool overlap = result1.ValidFrom <= range2End && result2.ValidFrom <= range1End;

                    if (overlap)
                    {
                        overlapErrors.Add(new OverlapError
                        {
                            Row1 = result1.RowNumber,
                            Row2 = result2.RowNumber,
                            SupplierId = result1.SupplierId,
                            Sku = result1.Sku,
                            Message = $"Date ranges overlap for supplier {result1.SupplierId} and SKU {result1.Sku} between rows {result1.RowNumber} and {result2.RowNumber}"
                        });
                    }
                }
                var existingEntries = await _priceListEntryRepository
                       .GetBySupplierAndSkuAsync(result1.SupplierId, Sku.Create(result1.Sku), cancellationToken: CancellationToken.None);
                var newRange = DateRange.Create(result1.ValidFrom, result1.ValidTo);
                foreach (var entry in existingEntries)
                {
                    if (entry.ValidityPeriod.OverlapsWith(newRange))
                    {
                        overlapErrors.Add(new OverlapError
                        {
                            Row1 = result1.RowNumber,
                            Row2 = -1,
                            SupplierId = result1.SupplierId,
                            Sku = result1.Sku,
                            Message = $"Date range overlaps with existing entry for supplier {result1.SupplierId} and SKU {result1.Sku} at row {result1.RowNumber}"
                        });
                    }



                }
            }
        }

        return overlapErrors;
    }

    private async Task<int> ImportValidEntriesAsync(
        CsvValidationSummary validationSummary,
        CancellationToken cancellationToken)
    {
        var validResults = validationSummary.Results.Where(r => r.IsValid).ToList();
        if (validResults.Count == 0) return 0;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var entriesToAdd = new List<PriceListEntry>();

            foreach (var result in validResults)
            {
                var dateRange = DateRange.Create(result.ValidFrom, result.ValidTo);
                var currency = Currency.FromCode(result.CurrencyCode);
                var money = Money.Create(result.PriceAmount, currency);
                var quantity = Quantity.Create(result.MinQty);

                var entry = PriceListEntry.Create(
                    result.SupplierId,
                    result.Sku,
                    dateRange,
                    money,
                    quantity);

                entriesToAdd.Add(entry);
            }

            await _priceListEntryRepository.AddRangeAsync(entriesToAdd, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully imported {Count} price list entries", entriesToAdd.Count);
            return entriesToAdd.Count;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error importing price list entries");
            throw;
        }
    }
}