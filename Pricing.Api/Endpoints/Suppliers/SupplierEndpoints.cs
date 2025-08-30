using Pricing.Application.Common.Models;
using Pricing.Application.UseCases.Suppliers.Queries;
using Pricing.Application.UseCases.Suppliers.Queries.GetSupplierById;
using Pricing.Application.UseCases.Suppliers.Commands.CreateSupplier;
using Pricing.Application.UseCases.Suppliers.Commands.UpdateSupplier;
using Pricing.Application.UseCases.Suppliers.Commands.DeleteSupplier;
using Pricing.Application.Contracts;

namespace Pricing.Api.Endpoints.Suppliers;

public static class SupplierEndpoints
{
    public static void MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/suppliers")
            .WithTags("Suppliers")
            .WithOpenApi();

        group.MapGet("/", GetSuppliers)
            .WithName("GetSuppliers");

        group.MapGet("/{id:int}", GetSupplierById)
            .WithName("GetSupplierById");

        group.MapPost("/", CreateSupplier)
            .WithName("CreateSupplier");

        group.MapPut("/{id:int}", UpdateSupplier)
            .WithName("UpdateSupplier");

        group.MapDelete("/{id:int}", DeleteSupplier)
            .WithName("DeleteSupplier");
    }

    private static async Task<IResult> GetSuppliers(
        IGetSuppliersPagedHandler handler,
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string sortDirection = "asc",
        string? search = null,
        string? name = null,
        string? country = null,
        bool? preferred = null,
        int? minLeadTime = null,
        int? maxLeadTime = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetSuppliersPagedQuery(
                new PaginationRequest { Page = page, PageSize = pageSize },
                new SortRequest { SortBy = sortBy, SortDirection = sortDirection },
                search,
                name,
                country,
                preferred,
                minLeadTime,
                maxLeadTime
            );

            var response = await handler.HandleAsync(query, ct);

            return Results.Ok(new
            {
                suppliers = response.Result.Items,
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
                    hasFilters = !string.IsNullOrEmpty(search) || !string.IsNullOrEmpty(name) || 
                                !string.IsNullOrEmpty(country) || preferred.HasValue ||
                                minLeadTime.HasValue || maxLeadTime.HasValue,
                    appliedFilters = new
                    {
                        search,
                        name,
                        country,
                        preferred,
                        minLeadTime,
                        maxLeadTime
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
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("SupplierEndpoints");
            logger.LogError(ex, "Error retrieving suppliers with pagination and filtering");
            return Results.Problem("Failed to retrieve suppliers", statusCode: 500);
        }
    }

    private static async Task<IResult> GetSupplierById(
        int id,
        IGetSupplierByIdHandler handler,
        CancellationToken ct)
    {
        try
        {
            var query = new GetSupplierByIdQuery(id);
            var response = await handler.HandleAsync(query, ct);

            return response.Supplier == null 
                ? Results.NotFound() 
                : Results.Ok(response.Supplier);
        }
        catch (Exception ex)
        {
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("SupplierEndpoints");
            logger.LogError(ex, "Error retrieving supplier {SupplierId}", id);
            return Results.Problem("Failed to retrieve supplier", statusCode: 500);
        }
    }

    private static async Task<IResult> CreateSupplier(
        SupplierModel supplierDto,
        ICreateSupplierHandler handler,
        ILogger<Program> logger)
    {
        try
        {
            var command = new CreateSupplierCommand(
                supplierDto.Name,
                supplierDto.Country,
                supplierDto.Preferred,
                supplierDto.LeadTimeDays
            );

            var response = await handler.HandleAsync(command);

            logger.LogInformation("API: Created new supplier: {SupplierName} ({SupplierId})", 
                response.Name, response.Id);

            return Results.Created($"/suppliers/{response.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating supplier via API");
            return Results.Problem("Failed to create supplier", statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateSupplier(
        int id,
        SupplierModel supplierDto,
        IUpdateSupplierHandler handler,
        ILogger<Program> logger)
    {
        try
        {
            var command = new UpdateSupplierCommand(
                id,
                supplierDto.Name,
                supplierDto.Country,
                supplierDto.Preferred,
                supplierDto.LeadTimeDays
            );

            var response = await handler.HandleAsync(command);

            if (response == null)
                return Results.NotFound();

            logger.LogInformation("API: Updated supplier: {SupplierName} ({SupplierId})", 
                response.Name, response.Id);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating supplier {SupplierId} via API", id);
            return Results.Problem("Failed to update supplier", statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteSupplier(
        int id,
        IDeleteSupplierHandler handler,
        ILogger<Program> logger)
    {
        try
        {
            var command = new DeleteSupplierCommand(id);
            var response = await handler.HandleAsync(command);

            if (!response.Success)
                return Results.NotFound();

            logger.LogInformation("API: Deleted supplier ({SupplierId})", id);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting supplier {SupplierId} via API", id);
            return Results.Problem("Failed to delete supplier", statusCode: 500);
        }
    }
}