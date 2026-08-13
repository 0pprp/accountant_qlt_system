using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Change_IsPaid_To_Status_In_InstallmentPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "InstallmentPayments");

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "InstallmentPayments",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "InstallmentPayments");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "InstallmentPayments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
