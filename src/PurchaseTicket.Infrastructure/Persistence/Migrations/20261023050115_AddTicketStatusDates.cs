using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseTicket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketStatusDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "amount_registered_at",
                table: "purchase_tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "purchase_tickets",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_registered_at",
                table: "purchase_tickets");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "purchase_tickets");
        }
    }
}
