using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Update_SellerCashDeliveryTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "UndeliveredCashAmount",
                table: "Users",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "SellerCashDeliveryTransactions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SellerCashDeliveryTransactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UndeliveredCashAmount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "SellerCashDeliveryTransactions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SellerCashDeliveryTransactions");
        }
    }
}
