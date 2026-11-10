using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InstallmentPaymentClientPaymentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientPaymentId",
                table: "InstallmentPayments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_InstallmentPayments_ClientPaymentId",
                table: "InstallmentPayments",
                column: "ClientPaymentId",
                unique: true,
                filter: "\"ClientPaymentId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_InstallmentPayments_ClientPaymentId",
                table: "InstallmentPayments");

            migrationBuilder.DropColumn(
                name: "ClientPaymentId",
                table: "InstallmentPayments");
        }
    }
}
