using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Infrastructure.Persistence.Configurations;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("materials");

        builder.HasKey(material => material.Id);

        builder.Property(material => material.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(material => material.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();
    }
}