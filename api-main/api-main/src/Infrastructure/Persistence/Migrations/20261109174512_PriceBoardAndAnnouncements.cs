using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PriceBoardAndAnnouncements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppKey = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TextBody = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BannerStorageKey = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    ZoomStorageKey = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    HasLink = table.Column<bool>(type: "boolean", nullable: false),
                    LinkUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceBoardEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductName = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DailyInstallment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DownPayment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TargetProvinceId = table.Column<int>(type: "integer", nullable: false),
                    TargetProvinceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    UpdatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBoardEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_AppKey_IsEnabled_SortOrder_Id",
                table: "Announcements",
                columns: new[] { "AppKey", "IsEnabled", "SortOrder", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_UpdatedAtUtc_Id",
                table: "Announcements",
                columns: new[] { "UpdatedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceBoardEntries_TargetProvinceId_UpdatedAtUtc_Id",
                table: "PriceBoardEntries",
                columns: new[] { "TargetProvinceId", "UpdatedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceBoardEntries_UpdatedAtUtc_Id",
                table: "PriceBoardEntries",
                columns: new[] { "UpdatedAtUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "PriceBoardEntries");
        }
    }
}
