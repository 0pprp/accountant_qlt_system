using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CustomerProfileTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    DocumentType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    FileKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImportKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDocuments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<byte>(type: "smallint", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    AccuracyMeters = table.Column<double>(type: "double precision", nullable: true),
                    Confidence = table.Column<double>(type: "double precision", nullable: true),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImportKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerLocations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerRatingSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Origin = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RatingLabel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Score = table.Column<double>(type: "double precision", nullable: true),
                    CaptureDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImportKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRatingSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRatingSnapshots_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerShopPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImportKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerShopPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerShopPhotos_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerShopProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    ShopName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BusinessType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StockEstimatedValue = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedDailyRevenue = table.Column<double>(type: "double precision", nullable: true),
                    ShopLength = table.Column<double>(type: "double precision", nullable: true),
                    ShopWidth = table.Column<double>(type: "double precision", nullable: true),
                    ShopArea = table.Column<double>(type: "double precision", nullable: true),
                    ShopImageKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImportKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerShopProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerShopProfiles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_CustomerId",
                table: "CustomerDocuments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UX_CustomerDocuments_ImportKey",
                table: "CustomerDocuments",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLocations_CustomerId_Source",
                table: "CustomerLocations",
                columns: new[] { "CustomerId", "Source" });

            migrationBuilder.CreateIndex(
                name: "UX_CustomerLocations_ImportKey",
                table: "CustomerLocations",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRatingSnapshots_CustomerId_CaptureDate",
                table: "CustomerRatingSnapshots",
                columns: new[] { "CustomerId", "CaptureDate" });

            migrationBuilder.CreateIndex(
                name: "UX_CustomerRatingSnapshots_ImportKey",
                table: "CustomerRatingSnapshots",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerShopPhotos_CustomerId",
                table: "CustomerShopPhotos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UX_CustomerShopPhotos_ImportKey",
                table: "CustomerShopPhotos",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerShopProfiles_CustomerId",
                table: "CustomerShopProfiles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UX_CustomerShopProfiles_ImportKey",
                table: "CustomerShopProfiles",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDocuments");

            migrationBuilder.DropTable(
                name: "CustomerLocations");

            migrationBuilder.DropTable(
                name: "CustomerRatingSnapshots");

            migrationBuilder.DropTable(
                name: "CustomerShopPhotos");

            migrationBuilder.DropTable(
                name: "CustomerShopProfiles");
        }
    }
}
