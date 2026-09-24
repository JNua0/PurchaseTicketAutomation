using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Infrastructure.Persistence.Configurations;

public class PurchaseTicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("purchase_tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(ticket => ticket.TicketNumber)
            .HasColumnName("ticket_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(ticket => ticket.TicketNumber)
            .IsUnique();

        builder.Property(ticket => ticket.SupplierId)
            .HasColumnName("supplier_id")
            .IsRequired();

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(ticket => ticket.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ticket => ticket.MaterialId)
            .HasColumnName("material_id")
            .IsRequired();

        builder.HasOne<Material>()
            .WithMany()
            .HasForeignKey(ticket => ticket.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ticket => ticket.LicensePlate)
            .HasColumnName("license_plate")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(ticket => ticket.DriverName)
            .HasColumnName("driver_name")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(ticket => ticket.GrossWeight)
            .HasColumnName("gross_weight")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(ticket => ticket.TareWeight)
            .HasColumnName("tare_weight")
            .HasPrecision(10, 2);

        builder.Property(ticket => ticket.NetWeight)
            .HasColumnName("net_weight")
            .HasPrecision(10, 2);

        builder.Property(ticket => ticket.Discount)
            .HasColumnName("discount")
            .HasPrecision(10, 2);

        builder.Property(ticket => ticket.DiscountWeight)
            .HasColumnName("discount_weight")
            .HasPrecision(10, 2);

        builder.Property(ticket => ticket.NetWeightAfterDiscount)
            .HasColumnName("net_weight_after_discount")
            .HasPrecision(10, 2);

        builder.Property(ticket => ticket.PricePerKg)
            .HasColumnName("price_per_kg")
            .HasPrecision(10, 2);

        builder.Property(ticket => ticket.Amount)
            .HasColumnName("amount")
            .HasPrecision(10, 2);

        builder.Property(ticket => ticket.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(ticket => ticket.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}