using Pricing.Api.Endpoints.Suppliers;
using Pricing.Api.Endpoints.Products;
using Pricing.Api.Endpoints.Development;
using Pricing.Api.Endpoints.Pricing;

namespace Pricing.Api.Extensions;

public static class EndpointExtensions
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
       
        app.MapSupplierEndpoints();
        app.MapProductEndpoints();
        app.MapPricingEndpoints();

        app.MapDevelopmentEndpoints();
    }
}