using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(supplier => supplier.Id);

        builder.Property(supplier => supplier.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(supplier => supplier.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(supplier => supplier.Name)
            .IsUnique();

        builder.Property(supplier => supplier.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(15);

        builder.Property(supplier => supplier.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
    }
}