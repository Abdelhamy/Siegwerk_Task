using Pricing.Application.Common.Models;
using Pricing.Application.UseCases.Products.Queries.GetProductById;
using Pricing.Application.UseCases.Products.Queries.GetProductsPaged;
using Pricing.Application.UseCases.Products.Commands.CreateProduct;
using Pricing.Application.UseCases.Products.Commands.UpdateProduct;
using Pricing.Application.UseCases.Products.Commands.DeleteProduct;
using Pricing.Application.Contracts;

namespace Pricing.Api.Endpoints.Products;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products")
            .WithTags("Products")
            .WithOpenApi();

        // GET - Get paged products with filtering and sorting
        group.MapGet("/", GetProductsPaged)
            .WithName("GetProductsPaged");

        group.MapGet("/{id:int}", GetProductById)
            .WithName("GetProductById");

        // CREATE - Add new product
        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct");

        // UPDATE - Update product
        group.MapPut("/{id:int}", UpdateProduct)
            .WithName("UpdateProduct");

        // DELETE - Remove product
        group.MapDelete("/{id:int}", DeleteProduct)
            .WithName("DeleteProduct");
    }

    private static async Task<IResult> GetProductsPaged(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string sortDirection = "asc",
        string? searchTerm = null,
        string? name = null,
        string? sku = null,
        string? unitOfMeasure = null,
        string? hazardClass = null,
        bool? isHazardous = null,
        IGetProductsPagedHandler handler = null!,
        CancellationToken ct = default)
    {
        try
        {
            var pagination = new PaginationRequest { Page = page, PageSize = pageSize };
            var sort = !string.IsNullOrWhiteSpace(sortBy) 
                ? new SortRequest { SortBy = sortBy, SortDirection = sortDirection }
                : null;

            var query = new GetProductsPagedQuery(
                pagination,
                sort,
                searchTerm,
                name,
                sku,
                unitOfMeasure,
                hazardClass,
                isHazardous);
                
            var response = await handler.HandleAsync(query, ct);
            return Results.Ok(response.Result);
        }
        catch (Exception ex)
        {
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("ProductEndpoints");
            logger.LogError(ex, "Error retrieving paged products");
            return Results.Problem("Failed to retrieve products", statusCode: 500);
        }
    }
   
    private static async Task<IResult> GetProductById(
        int id,
        IGetProductByIdHandler handler,
        CancellationToken ct)
    {
        try
        {
            var query = new GetProductByIdQuery(id);
            var response = await handler.HandleAsync(query, ct);
            
            return response.Product == null 
                ? Results.NotFound() 
                : Results.Ok(response.Product);
        }
        catch (Exception ex)
        {
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("ProductEndpoints");
            logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return Results.Problem("Failed to retrieve product", statusCode: 500);
        }
    }

    private static async Task<IResult> CreateProduct(
        ProductModel productModel,
        ICreateProductHandler handler,
        ILogger<Program> logger)
    {
        try
        {
            var command = new CreateProductCommand(
                productModel.Sku,
                productModel.Name,
                productModel.UnitOfMeasure ?? "EA",
                productModel.HazardClass);

            var response = await handler.HandleAsync(command);

            logger.LogInformation("API: Created new product: {ProductName} ({Sku})", 
                response.Name, response.Sku);

            return Results.Created($"/products/{response.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product via API");
            return Results.Problem("Failed to create product", statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateProduct(
        int id,
        ProductModel productModel,
        IUpdateProductHandler handler,
        ILogger<Program> logger)
    {
        try
        {
            var command = new UpdateProductCommand(
                id,
                productModel.Sku,
                productModel.Name,
                productModel.UnitOfMeasure ?? "EA",
                productModel.HazardClass);

            var response = await handler.HandleAsync(command);

            if (response == null)
                return Results.NotFound();

            logger.LogInformation("API: Updated product: {ProductName} ({Sku})", 
                response.Name, response.Sku);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product {ProductId} via API", id);
            return Results.Problem("Failed to update product", statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteProduct(
        int id,
        IDeleteProductHandler handler,
        ILogger<Program> logger)
    {
        try
        {
            var command = new DeleteProductCommand(id);
            var response = await handler.HandleAsync(command);

            if (!response.Success)
                return Results.NotFound();

            logger.LogInformation("API: Deleted product ({ProductId})", id);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product {ProductId} via API", id);
            return Results.Problem("Failed to delete product", statusCode: 500);
        }
    }
}