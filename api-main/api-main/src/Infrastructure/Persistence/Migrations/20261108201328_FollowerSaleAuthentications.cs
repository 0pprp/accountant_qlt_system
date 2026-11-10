using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FollowerSaleAuthentications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowerSaleAuthentications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    OrderListId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    FollowerUserId = table.Column<int>(type: "integer", nullable: false),
                    FollowerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ShopStockEstimatedValue = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    EstimatedDailyRevenue = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    AuthenticatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowerSaleAuthentications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerSaleAuthentications_BranchId_OrderListId",
                table: "FollowerSaleAuthentications",
                columns: new[] { "BranchId", "OrderListId" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerSaleAuthentications_FollowerUserId_AuthenticatedAtU~",
                table: "FollowerSaleAuthentications",
                columns: new[] { "FollowerUserId", "AuthenticatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerSaleAuthentications_OrderId",
                table: "FollowerSaleAuthentications",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowerSaleAuthentications_OrderListId_AuthenticatedAtUtc",
                table: "FollowerSaleAuthentications",
                columns: new[] { "OrderListId", "AuthenticatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowerSaleAuthentications");
        }
    }
}
