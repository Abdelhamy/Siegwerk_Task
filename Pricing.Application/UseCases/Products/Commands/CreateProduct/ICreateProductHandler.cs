namespace Pricing.Application.UseCases.Products.Commands.CreateProduct;

public interface ICreateProductHandler
{
    Task<CreateProductResponse> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default);
}