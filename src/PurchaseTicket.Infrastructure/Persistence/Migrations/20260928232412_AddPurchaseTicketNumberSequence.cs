using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseTicket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseTicketNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "purchase_ticket_number_seq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "purchase_ticket_number_seq");
        }
    }
}
