using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Infrastructure.Persistence.Configurations;

// Una fila por correccion; los cambios van juntos en una columna jsonb
// porque siempre se leen y se escriben completos
public class TicketCorrectionConfiguration
    : IEntityTypeConfiguration<TicketCorrection>
{
    public void Configure(
        EntityTypeBuilder<TicketCorrection> builder)
    {
        builder.ToTable("purchase_ticket_corrections");

        builder.HasKey(correction => correction.Id);

        builder.Property(correction => correction.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(correction => correction.PurchaseTicketId)
            .HasColumnName("purchase_ticket_id")
            .IsRequired();

        builder.HasIndex(correction => correction.PurchaseTicketId);

        builder.Property(correction => correction.CorrectedAt)
            .HasColumnName("corrected_at")
            .IsRequired();

        builder.Property(correction => correction.Reason)
            .HasColumnName("reason")
            .HasMaxLength(TicketCorrection.ReasonMaxLength);

        builder.OwnsMany(correction => correction.Changes, changes =>
        {
            changes.ToJson("changes");

            changes.Property(change => change.Field)
                .HasJsonPropertyName("field")
                .HasConversion<string>();

            changes.Property(change => change.Before)
                .HasJsonPropertyName("before");

            changes.Property(change => change.After)
                .HasJsonPropertyName("after");

            changes.Property(change => change.IsRecalculated)
                .HasJsonPropertyName("is_recalculated");
        });

        builder.Navigation(correction => correction.Changes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
