using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SalesFollowupAppBlockAndBoardRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectorSalesEmployeeBoardRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeUserId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    RatedByUserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectorSalesEmployeeBoardRatings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesEmployeeAppBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeUserId = table.Column<int>(type: "integer", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    UpdatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesEmployeeAppBlocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectorSalesEmployeeBoardRatings_EmployeeUserId_RatedAtUtc",
                table: "DirectorSalesEmployeeBoardRatings",
                columns: new[] { "EmployeeUserId", "RatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesEmployeeAppBlocks_EmployeeUserId",
                table: "SalesEmployeeAppBlocks",
                column: "EmployeeUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectorSalesEmployeeBoardRatings");

            migrationBuilder.DropTable(
                name: "SalesEmployeeAppBlocks");
        }
    }
}
