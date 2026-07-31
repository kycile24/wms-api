using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities;

namespace Wms.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Barcode);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.UnitOfMeasure)
            .HasMaxLength(20)
            .HasDefaultValue("PCS")
            .IsRequired();

        builder.Property(p => p.MinimumStockThreshold)
            .HasDefaultValue(10)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}