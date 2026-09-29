using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseTicket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseTicketCompletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "purchase_tickets",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "purchase_tickets");
        }
    }
}
