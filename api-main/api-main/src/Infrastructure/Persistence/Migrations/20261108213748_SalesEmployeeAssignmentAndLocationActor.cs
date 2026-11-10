using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SalesEmployeeAssignmentAndLocationActor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "ActorKind",
                table: "SalesLocationPoints",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "SalesEmployeeAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    StartsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesEmployeeAssignments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesLocationPoints_ActorKind_UserId_RecordedAtUtc",
                table: "SalesLocationPoints",
                columns: new[] { "ActorKind", "UserId", "RecordedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesEmployeeAssignments_BranchId_EndsAtUtc_StartsAtUtc",
                table: "SalesEmployeeAssignments",
                columns: new[] { "BranchId", "EndsAtUtc", "StartsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesEmployeeAssignments_UserId_BranchId_EndsAtUtc",
                table: "SalesEmployeeAssignments",
                columns: new[] { "UserId", "BranchId", "EndsAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesEmployeeAssignments");

            migrationBuilder.DropIndex(
                name: "IX_SalesLocationPoints_ActorKind_UserId_RecordedAtUtc",
                table: "SalesLocationPoints");

            migrationBuilder.DropColumn(
                name: "ActorKind",
                table: "SalesLocationPoints");
        }
    }
}
