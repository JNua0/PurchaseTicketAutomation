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
    }
}