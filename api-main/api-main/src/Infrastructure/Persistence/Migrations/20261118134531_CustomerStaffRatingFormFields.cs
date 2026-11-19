using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CustomerStaffRatingFormFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ContextAmount",
                table: "CustomerStaffRatings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedDailyRevenue",
                table: "CustomerStaffRatings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occasion",
                table: "CustomerStaffRatings",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShopStockEstimatedValue",
                table: "CustomerStaffRatings",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContextAmount",
                table: "CustomerStaffRatings");

            migrationBuilder.DropColumn(
                name: "EstimatedDailyRevenue",
                table: "CustomerStaffRatings");

            migrationBuilder.DropColumn(
                name: "Occasion",
                table: "CustomerStaffRatings");

            migrationBuilder.DropColumn(
                name: "ShopStockEstimatedValue",
                table: "CustomerStaffRatings");
        }
    }
}
