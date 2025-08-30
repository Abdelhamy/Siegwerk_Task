using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        // Configure Sku value object with value converter
        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                sku => sku.Value,
                value => Sku.Create(value));

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.UnitOfMeasure)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(p => p.HazardClass)
            .HasMaxLength(50);
        
        // Indexes for performance on direct properties only
        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("IX_Products_Sku");
        
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");
        
        builder.HasIndex(p => p.UnitOfMeasure)
            .HasDatabaseName("IX_Products_UnitOfMeasure");
        
        builder.HasIndex(p => p.HazardClass)
            .HasDatabaseName("IX_Products_HazardClass");
        
        // Add concurrency token
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .HasColumnName("RowVersion");
    }
}