using Pricing.Application.Contracts;

namespace Pricing.Application.UseCases.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id);

public record GetProductByIdResponse(
    ProductDto? Product
);

public interface IGetProductByIdHandler
{
    Task<GetProductByIdResponse> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default);
}