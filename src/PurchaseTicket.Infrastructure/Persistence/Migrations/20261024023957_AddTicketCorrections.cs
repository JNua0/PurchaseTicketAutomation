using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PurchaseTicket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_ticket_corrections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_ticket_id = table.Column<int>(type: "integer", nullable: false),
                    corrected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_ticket_corrections", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_ticket_corrections_purchase_tickets_purchase_ticke~",
                        column: x => x.purchase_ticket_id,
                        principalTable: "purchase_tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_ticket_corrections_purchase_ticket_id",
                table: "purchase_ticket_corrections",
                column: "purchase_ticket_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_ticket_corrections");
        }
    }
}
