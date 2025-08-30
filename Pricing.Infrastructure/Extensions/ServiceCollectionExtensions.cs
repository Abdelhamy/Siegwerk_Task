using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Application.Services;
using Pricing.Application.UseCases.Products.Commands.CreateProduct;
using Pricing.Application.UseCases.Products.Commands.UpdateProduct;
using Pricing.Application.UseCases.Products.Commands.DeleteProduct;
using Pricing.Application.UseCases.Products.Queries.GetProductById;
using Pricing.Application.UseCases.Products.Queries.GetProductsPaged;
using Pricing.Application.UseCases.Suppliers.Commands.CreateSupplier;
using Pricing.Application.UseCases.Suppliers.Commands.DeleteSupplier;
using Pricing.Application.UseCases.Suppliers.Commands.UpdateSupplier;
using Pricing.Application.UseCases.Suppliers.Queries;
using Pricing.Application.UseCases.Suppliers.Queries.GetSupplierById;
using Pricing.Application.UseCases.PriceLists.Queries.GetPricesPagedQuery;
using Pricing.Application.UseCases.PriceLists.Commands.ImportPricesFromCsv;
using Pricing.Application.UseCases.Pricing.Queries.GetBestPrice;
using Pricing.Infrastructure.Logging;
using Pricing.Infrastructure.Persistence;
using Pricing.Infrastructure.Rates;
using Pricing.Infrastructure.Repositories;

namespace Pricing.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));
        services.AddScoped<IRateProvider, InMemoryRateProvider>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPriceListEntryRepository, PriceListEntryRepository>();

        services.AddSingleton<IRateProvider, InMemoryRateProvider>();

        return services;
    }

    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        // Supplier use cases
        services.AddScoped<IGetSuppliersPagedHandler, GetSuppliersPagedHandler>();
        services.AddScoped<IGetSupplierByIdHandler, GetSupplierByIdHandler>();
        services.AddScoped<ICreateSupplierHandler, CreateSupplierHandler>();
        services.AddScoped<IUpdateSupplierHandler, UpdateSupplierHandler>();
        services.AddScoped<IDeleteSupplierHandler, DeleteSupplierHandler>();

        // Product use cases
        services.AddScoped<IGetProductByIdHandler, GetProductByIdHandler>();
        services.AddScoped<IGetProductsPagedHandler, GetProductsPagedHandler>();
        services.AddScoped<ICreateProductHandler, CreateProductHandler>();
        services.AddScoped<IUpdateProductHandler, UpdateProductHandler>();
        services.AddScoped<IDeleteProductHandler, DeleteProductHandler>();

        // Pricing use cases
        services.AddScoped<IGetBestPriceHandler, GetBestPriceHandler>();
        services.AddScoped<IGetPricesPagedHandler, GetPricesPagedHandler>();
        
        // Price list use cases
        services.AddScoped<IImportPricesFromCsvHandler, ImportPricesFromCsvHandler>();

        services.AddScoped<ICurrencyService, CurrencyService>();

        return services;
    }
}