using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CustomerGpsAndStaffRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Business_GpsLatitude",
                table: "Customers",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Business_GpsLongitude",
                table: "Customers",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerStaffRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    RatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerStaffRatings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStaffRatings_CustomerId_Role",
                table: "CustomerStaffRatings",
                columns: new[] { "CustomerId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStaffRatings_RatedAtUtc",
                table: "CustomerStaffRatings",
                column: "RatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerStaffRatings");

            migrationBuilder.DropColumn(
                name: "Business_GpsLatitude",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Business_GpsLongitude",
                table: "Customers");
        }
    }
}
