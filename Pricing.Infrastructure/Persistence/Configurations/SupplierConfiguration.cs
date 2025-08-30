using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Domain.Entities;

namespace Pricing.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(s => s.Country)
            .HasMaxLength(100);
        
        builder.Property(s => s.Preferred)
            .IsRequired();
        
        // Configure LeadTime value object
        builder.OwnsOne(s => s.LeadTime, leadTime =>
        {
            leadTime.Property(lt => lt.Days)
                .HasColumnName("LeadTimeDays")
                .IsRequired();
        });
        
        // Configure relationship with PriceListEntry using the public property
        builder.HasMany(s => s.PriceListEntries)
            .WithOne(pe => pe.Supplier)
            .HasForeignKey(pe => pe.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes for performance
        builder.HasIndex(s => s.Name)
            .HasDatabaseName("IX_Suppliers_Name");
        
        builder.HasIndex(s => s.Preferred)
            .HasDatabaseName("IX_Suppliers_Preferred");
        
        builder.HasIndex(s => s.Country)
            .HasDatabaseName("IX_Suppliers_Country");
        
        // Add concurrency token
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .HasColumnName("RowVersion");
    }
}