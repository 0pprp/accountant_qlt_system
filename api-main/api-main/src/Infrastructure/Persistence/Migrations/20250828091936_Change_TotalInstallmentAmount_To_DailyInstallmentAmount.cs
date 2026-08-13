using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Change_TotalInstallmentAmount_To_DailyInstallmentAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalInstallmentAmount",
                table: "Products",
                newName: "DailyInstallmentAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DailyInstallmentAmount",
                table: "Products",
                newName: "TotalInstallmentAmount");
        }
    }
}
