using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Infrastructure.Persistence.Configurations;

public class PriceListEntryConfiguration : IEntityTypeConfiguration<PriceListEntry>
{
    public void Configure(EntityTypeBuilder<PriceListEntry> builder)
    {
        builder.ToTable("PriceListEntries");
        
        builder.HasKey(pe => pe.Id);
        
        builder.Property(pe => pe.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(pe => pe.SupplierId)
            .IsRequired();

        // Configure Sku value object with value converter
        builder.Property(pe => pe.Sku)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                sku => sku.Value,
                value => Sku.Create(value));

        // Configure DateRange value object
        builder.OwnsOne(pe => pe.ValidityPeriod, dateRange =>
        {
            dateRange.Property(dr => dr.From)
                .HasColumnName("ValidFrom")
                .IsRequired();
            
            dateRange.Property(dr => dr.To)
                .HasColumnName("ValidTo");
            
            // Indexes on the owned properties
            dateRange.HasIndex(dr => dr.From)
                .HasDatabaseName("IX_PriceListEntries_ValidFrom");
            
            dateRange.HasIndex(dr => dr.To)
                .HasDatabaseName("IX_PriceListEntries_ValidTo");
        });
        
        // Configure Money value object with Currency value converter
        builder.OwnsOne(pe => pe.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(18,4)")
                .IsRequired();
            
            // Use value converter for Currency instead of nested owned entity
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .IsRequired()
                .HasMaxLength(3)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.Create(code));
                
            // Index on currency
            money.HasIndex(m => m.Currency)
                .HasDatabaseName("IX_PriceListEntries_Currency");
        });
        
        // Configure Quantity value object
        builder.OwnsOne(pe => pe.MinimumQuantity, quantity =>
        {
            quantity.Property(q => q.Value)
                .HasColumnName("MinimumQuantity")
                .IsRequired();
            
            // Index on quantity
            quantity.HasIndex(q => q.Value)
                .HasDatabaseName("IX_PriceListEntries_MinQuantity");
        });
        
        // Configure relationship with Supplier
        builder.HasOne(pe => pe.Supplier)
            .WithMany()
            .HasForeignKey(pe => pe.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes for query performance on direct properties
        builder.HasIndex(pe => pe.SupplierId)
            .HasDatabaseName("IX_PriceListEntries_SupplierId");
        
        builder.HasIndex(pe => pe.Sku)
            .HasDatabaseName("IX_PriceListEntries_Sku");
        
        // Add concurrency token
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .HasColumnName("RowVersion");
    }
}