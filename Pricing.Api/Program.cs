using Pricing.Infrastructure.Persistence;
using Pricing.Infrastructure.Extensions;
using Pricing.Api.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Pricing API", 
        Version = "v1",
        Description = "API for finding the best price for products across multiple suppliers with currency conversion support",
    });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DefaultConnection string is required");
}

builder.Services.AddDbContext<PricingDbContext>(opt =>
    opt.UseSqlServer(connectionString));

builder.Services.AddHealthChecks();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddInfrastructureServices();
builder.Services.AddInfrastructureRepositories();
builder.Services.AddApplicationHandlers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {   
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pricing API v1");
        c.RoutePrefix = string.Empty;
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
        c.DisplayRequestDuration();
    });
}

app.UseCors();

app.MapApiEndpoints();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Pricing API started successfully on {Environment} environment", app.Environment.EnvironmentName);

app.Run();
