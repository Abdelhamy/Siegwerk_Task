using Microsoft.AspNetCore.Mvc;
using Pricing.Application.Common.Models;
using Pricing.Application.UseCases.PriceLists.Queries.GetPricesPagedQuery;
using Pricing.Application.UseCases.PriceLists.Commands.ImportPricesFromCsv;
using Pricing.Application.UseCases.Pricing.Queries.GetBestPrice;
using System.Text;

namespace Pricing.Api.Endpoints.Pricing;

public static class PricingEndpoints
{
    public static void MapPricingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/pricing")
            .WithTags("Pricing")
            .WithOpenApi();

        group.MapGet("/best", GetBestPrice)
            .WithName("GetBestPrice");

        group.MapGet("/prices", ListPrices)
            .WithName("ListPrices");

        group.MapPost("/prices/upload-csv", UploadPricesCsv)
            .WithName("UploadPricesCsv")
            .DisableAntiforgery();

        group.MapGet("/prices/csv-template", GetCsvTemplate)
            .WithName("GetCsvTemplate");
    }

  
    private static async Task<IResult> GetBestPrice(
        [FromQuery] string sku,
        [FromQuery] int qty,
        [FromQuery] string currency,
        [FromQuery] string date,
        IGetBestPriceHandler handler,
        ILogger<Program> logger,
        CancellationToken ct = default)
    {
        try
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                return Results.BadRequest(new { error = "Invalid date format. Expected YYYY-MM-DD." });
            }

            var query = new GetBestPriceQuery(sku, qty, currency, parsedDate);

            logger.LogInformation("Processing best price request for SKU: {Sku}, Qty: {Quantity}, Currency: {Currency}, Date: {Date}",
                sku, qty, currency, parsedDate);

            var response = await handler.HandleAsync(query, ct);

            if (response.BestPrice == null)
            {
                logger.LogInformation("No price found for SKU: {Sku}, Qty: {Quantity}, Currency: {Currency}, Date: {Date}",
                    sku, qty, currency, parsedDate);
                
                return Results.NotFound(new 
                { 
                    message = "No valid price found for the specified SKU, quantity, currency and date",
                    sku,
                    quantity = qty,
                    currency,
                    date = parsedDate
                });
            }

            logger.LogInformation("Best price found for SKU: {Sku} - Supplier: {SupplierName} ({SupplierId}), Unit Price: {UnitPrice} {Currency}",
                sku, response.BestPrice.SupplierName, response.BestPrice.SupplierId, response.BestPrice.UnitPrice, currency);

            return Results.Ok(response.BestPrice);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Invalid request parameters: {Message}", ex.Message);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing best price request for SKU: {Sku}, Qty: {Quantity}, Currency: {Currency}",
                sku, qty, currency);
            return Results.Problem("Failed to process best price request", statusCode: 500);
        }
    }


    
    private static async Task<IResult> ListPrices(
        IGetPricesPagedHandler handler,
        ILogger<Program> logger,
        [FromQuery] string? sku = null,
        [FromQuery] int? quantity = null,
        [FromQuery] string? validOn = null,
        [FromQuery] string? currency = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        CancellationToken ct = default)
    {
        try
        {
            // Parse validOn date if provided
            DateOnly? validOnDate = null;
            if (!string.IsNullOrEmpty(validOn))
            {
                if (!DateOnly.TryParse(validOn, out var parsedValidOn))
                {
                    return Results.BadRequest(new { error = "Invalid validOn date format. Expected YYYY-MM-DD." });
                }
                validOnDate = parsedValidOn;
            }

            var query = new GetPricesPagedQuery(
                new PaginationRequest { Page = page, PageSize = pageSize },
                new SortRequest { SortBy = sortBy, SortDirection = sortDirection },
                sku,
                quantity,
                validOnDate,
                currency,
                supplierId
            );

            logger.LogInformation("Listing prices with filters - SKU: {Sku}, Quantity: {Quantity}, ValidOn: {ValidOn}, Currency: {Currency}, SupplierId: {SupplierId}, Page: {Page}, PageSize: {PageSize}",
                sku, quantity, validOn, currency, supplierId, page, pageSize);

            var response = await handler.HandleAsync(query, ct);

            return Results.Ok(new
            {
                prices = response.Result.Items,
                pagination = new
                {
                    response.Result.Page,
                    response.Result.PageSize,
                    response.Result.TotalCount,
                    response.Result.TotalPages,
                    response.Result.HasPreviousPage,
                    response.Result.HasNextPage,
                    response.Result.Count
                },
                filtering = new
                {
                    hasFilters = !string.IsNullOrEmpty(sku) || quantity.HasValue || validOnDate.HasValue ||
                                !string.IsNullOrEmpty(currency) || supplierId.HasValue,
                    appliedFilters = new
                    {
                        sku,
                        quantity,
                        validOn = validOnDate,
                        currency,
                        supplierId
                    }
                }
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing prices");
            return Results.Problem("Failed to list prices", statusCode: 500);
        }
    }

    private static async Task<IResult> UploadPricesCsv(
        IFormFile file,
        IImportPricesFromCsvHandler handler,
        ILogger<Program> logger,
        CancellationToken ct = default)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "No file provided or file is empty." });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { error = "Only CSV files are supported." });
            }

            if (file.Length > 10 * 1024 * 1024) // 10MB limit
            {
                return Results.BadRequest(new { error = "File size exceeds 10MB limit." });
            }

            logger.LogInformation("Processing CSV upload: {FileName}, Size: {Size} bytes", 
                file.FileName, file.Length);

            // Process the CSV file
            using var stream = file.OpenReadStream();
            var command = new ImportPricesFromCsvCommand(stream, file.FileName);
            var response = await handler.HandleAsync(command, ct);

            if (response.Success)
            {
                logger.LogInformation("CSV import successful: {ImportedCount} entries imported from {FileName}", 
                    response.ImportedCount, file.FileName);

                return Results.Ok(new
                {
                    success = true,
                    message = response.Message,
                    importedCount = response.ImportedCount,
                    summary = new
                    {
                        response.ValidationSummary.TotalRows,
                        response.ValidationSummary.ValidRows,
                        response.ValidationSummary.InvalidRows,
                        OverlapErrorsCount = response.ValidationSummary.OverlapErrors.Count
                    },
                    validationDetails = response.ValidationSummary.InvalidRows > 0 ? new
                    {
                        errors = response.ValidationSummary.Results
                            .Where(r => !r.IsValid)
                            .Select(r => new
                            {
                                r.RowNumber,
                                r.Errors,
                                r.Warnings
                            })
                            .ToList(),
                        overlapErrors = response.ValidationSummary.OverlapErrors
                    } : null
                });
            }
            else
            {
                logger.LogWarning("CSV import failed for {FileName}: {Message}", 
                    file.FileName, response.Message);

                return Results.BadRequest(new
                {
                    success = false,
                    message = response.Message,
                    summary = new
                    {
                        response.ValidationSummary.TotalRows,
                        response.ValidationSummary.ValidRows,
                        response.ValidationSummary.InvalidRows,
                        OverlapErrorsCount = response.ValidationSummary.OverlapErrors.Count
                    },
                    validationDetails = new
                    {
                        globalErrors = response.ValidationSummary.GlobalErrors,
                        errors = response.ValidationSummary.Results
                            .Where(r => !r.IsValid)
                            .Select(r => new
                            {
                                r.RowNumber,
                                r.Errors,
                                r.Warnings
                            })
                            .ToList(),
                        overlapErrors = response.ValidationSummary.OverlapErrors
                    }
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing CSV upload: {FileName}", file?.FileName ?? "unknown");
            return Results.Problem("Failed to process CSV upload", statusCode: 500);
        }
    }

    private static async Task<IResult> GetCsvTemplate(
        ILogger<Program> logger,
        CancellationToken ct = default)
    {
        try
        {
            var csvContent = """
            SupplierId,Sku,ValidFrom,ValidTo,Currency,PricePerUom,MinQty
            1,SKU-1001,2025-01-01,2025-12-31,USD,25.50,10
            1,SKU-1002,2025-01-01,,EUR,18.75,5
            2,SKU-1001,2025-02-01,2025-11-30,USD,24.00,15
            2,SKU-1003,2025-01-15,2025-06-30,EGP,750.00,20
            """;

            var bytes = Encoding.UTF8.GetBytes(csvContent);
            var fileResult = Results.File(bytes, "text/csv", "price-list-template.csv");

            logger.LogInformation("CSV template downloaded");

            return fileResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating or retrieving CSV template");
            return Results.Problem("Failed to download CSV template", statusCode: 500);
        }
    }
}