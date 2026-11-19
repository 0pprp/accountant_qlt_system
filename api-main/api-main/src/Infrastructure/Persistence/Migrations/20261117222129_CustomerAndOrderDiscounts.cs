using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CustomerAndOrderDiscounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DailyInstallmentDiscountApplied",
                table: "Orders",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SellAmountDiscountApplied",
                table: "Orders",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DailyInstallmentDiscount",
                table: "Customers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SellAmountDiscount",
                table: "Customers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyInstallmentDiscountApplied",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SellAmountDiscountApplied",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DailyInstallmentDiscount",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SellAmountDiscount",
                table: "Customers");
        }
    }
}
