using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FollowerTrackingLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AccuracyMeters",
                table: "SalesLocationPoints",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientEventId",
                table: "SalesLocationPoints",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReceivedAtUtc",
                table: "SalesLocationPoints",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """UPDATE "SalesLocationPoints" SET "ReceivedAtUtc" = "RecordedAtUtc" WHERE "ReceivedAtUtc" IS NULL;""");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ReceivedAtUtc",
                table: "SalesLocationPoints",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                table: "SalesLocationPoints",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesLocationPoints_UserId_ClientEventId",
                table: "SalesLocationPoints",
                columns: new[] { "UserId", "ClientEventId" },
                unique: true,
                filter: "\"ClientEventId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesLocationPoints_UserId_ClientEventId",
                table: "SalesLocationPoints");

            migrationBuilder.DropColumn(
                name: "AccuracyMeters",
                table: "SalesLocationPoints");

            migrationBuilder.DropColumn(
                name: "ClientEventId",
                table: "SalesLocationPoints");

            migrationBuilder.DropColumn(
                name: "ReceivedAtUtc",
                table: "SalesLocationPoints");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "SalesLocationPoints");
        }
    }
}
