using Microsoft.EntityFrameworkCore;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Infrastructure.Persistence;

public class PricingDbContext : DbContext
{
    public PricingDbContext(DbContextOptions<PricingDbContext> options) : base(options) { }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PriceListEntry> PriceListEntries => Set<PriceListEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}