using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FieldPromoters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldPromoterAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    SinceUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RaisedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SeenByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPromoterAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldPromoterAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldPromoterDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkMinutes = table.Column<int>(type: "integer", nullable: false),
                    TargetMinutes = table.Column<int>(type: "integer", nullable: false),
                    Pins = table.Column<int>(type: "integer", nullable: false),
                    Orders = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPromoterDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldPromoterDays_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldPromoterOrderShops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesRequestId = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Photo = table.Column<byte[]>(type: "bytea", nullable: true),
                    PhotoContentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPromoterOrderShops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldPromoterOrderShops_SalesRequests_SalesRequestId",
                        column: x => x.SalesRequestId,
                        principalTable: "SalesRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldPromoterPins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Interest = table.Column<byte>(type: "smallint", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AgentLatitude = table.Column<double>(type: "double precision", nullable: true),
                    AgentLongitude = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPromoterPins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldPromoterPins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FieldPromoterProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IsSuspended = table.Column<bool>(type: "boolean", nullable: false),
                    SuspendReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DailyWorkMinutesTarget = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPromoterProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldPromoterProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldPromoterSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DefaultDailyWorkMinutes = table.Column<int>(type: "integer", nullable: false),
                    StationaryRadiusMeters = table.Column<int>(type: "integer", nullable: false),
                    StationaryMinutes = table.Column<int>(type: "integer", nullable: false),
                    SupportPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SupportWhatsapp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastFrozenDay = table.Column<DateOnly>(type: "date", nullable: true),
                    LastRetentionRunDay = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPromoterSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldPromoterAlerts_RaisedAtUtc_SeenAtUtc",
                table: "FieldPromoterAlerts",
                columns: new[] { "RaisedAtUtc", "SeenAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldPromoterAlerts_UserId_ResolvedAtUtc",
                table: "FieldPromoterAlerts",
                columns: new[] { "UserId", "ResolvedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldPromoterDays_Day",
                table: "FieldPromoterDays",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_FieldPromoterDays_UserId_Day",
                table: "FieldPromoterDays",
                columns: new[] { "UserId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldPromoterOrderShops_SalesRequestId",
                table: "FieldPromoterOrderShops",
                column: "SalesRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldPromoterPins_Latitude_Longitude",
                table: "FieldPromoterPins",
                columns: new[] { "Latitude", "Longitude" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FieldPromoterPins_UserId_CreatedAtUtc",
                table: "FieldPromoterPins",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldPromoterProfiles_UserId",
                table: "FieldPromoterProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldPromoterAlerts");

            migrationBuilder.DropTable(
                name: "FieldPromoterDays");

            migrationBuilder.DropTable(
                name: "FieldPromoterOrderShops");

            migrationBuilder.DropTable(
                name: "FieldPromoterPins");

            migrationBuilder.DropTable(
                name: "FieldPromoterProfiles");

            migrationBuilder.DropTable(
                name: "FieldPromoterSettings");
        }
    }
}
