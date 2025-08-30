using Pricing.Application.Models.PriceLists;

namespace Pricing.Application.UseCases.PriceLists.Commands.ImportPricesFromCsv;

public record ImportPricesFromCsvCommand(Stream CsvStream, string FileName);

public record ImportPricesFromCsvResponse(
    bool Success,
    string Message,
    CsvValidationSummary ValidationSummary,
    int ImportedCount = 0);

public interface IImportPricesFromCsvHandler
{
    Task<ImportPricesFromCsvResponse> HandleAsync(ImportPricesFromCsvCommand command, CancellationToken cancellationToken = default);
}