using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FollowerCustomerRatingSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowerCustomerRatingSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    OrderListId = table.Column<int>(type: "integer", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RatingLabel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    CaptureDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowerCustomerRatingSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerCustomerRatingSnapshots_CustomerId_CaptureDate",
                table: "FollowerCustomerRatingSnapshots",
                columns: new[] { "CustomerId", "CaptureDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowerCustomerRatingSnapshots_OrderListId_CaptureDate",
                table: "FollowerCustomerRatingSnapshots",
                columns: new[] { "OrderListId", "CaptureDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowerCustomerRatingSnapshots");
        }
    }
}
