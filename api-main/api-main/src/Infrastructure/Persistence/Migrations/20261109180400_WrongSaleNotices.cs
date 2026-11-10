using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WrongSaleNotices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WrongSaleNotices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    FollowerUserId = table.Column<int>(type: "integer", nullable: false),
                    FollowerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ShopStockEstimatedValue = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    EstimatedDailyRevenue = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    ReasonBlock = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DelegateReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DelegateReadByUserId = table.Column<int>(type: "integer", nullable: true),
                    BranchManagerReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BranchManagerReviewedByUserId = table.Column<int>(type: "integer", nullable: true),
                    BranchManagerReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WrongSaleNotices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WrongSaleNotices_BranchId_Status_CreatedAtUtc",
                table: "WrongSaleNotices",
                columns: new[] { "BranchId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WrongSaleNotices_DelegateReadAtUtc_CreatedAtUtc",
                table: "WrongSaleNotices",
                columns: new[] { "DelegateReadAtUtc", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WrongSaleNotices_OrderId",
                table: "WrongSaleNotices",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WrongSaleNotices");
        }
    }
}
