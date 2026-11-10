using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Branch31ComplaintsVisitsAndOrderRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_BranchId",
                table: "Orders");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BranchManagerReadAtUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SalesCompletedAtUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BranchVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    OpenedByUserId = table.Column<int>(type: "integer", nullable: false),
                    OpenedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ArrivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DepartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DepartNote = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchVisits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceApp = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SenderUserId = table.Column<int>(type: "integer", nullable: false),
                    SenderDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SenderRole = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    BranchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadByUserId = table.Column<int>(type: "integer", nullable: true),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BranchVisitRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitId = table.Column<int>(type: "integer", nullable: false),
                    OrderListId = table.Column<int>(type: "integer", nullable: false),
                    ListName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    RatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    RatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchVisitRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchVisitRatings_BranchVisits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "BranchVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BranchId_WorkflowStatus_BranchManagerReadAtUtc_Sales~",
                table: "Orders",
                columns: new[] { "BranchId", "WorkflowStatus", "BranchManagerReadAtUtc", "SalesCompletedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchVisitRatings_VisitId_OrderListId",
                table: "BranchVisitRatings",
                columns: new[] { "VisitId", "OrderListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchVisits_BranchId_ArrivedAtUtc",
                table: "BranchVisits",
                columns: new[] { "BranchId", "ArrivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchVisits_OpenedByUserId_Status",
                table: "BranchVisits",
                columns: new[] { "OpenedByUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMessages_BranchId_CreatedAtUtc",
                table: "ComplaintMessages",
                columns: new[] { "BranchId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMessages_Status_CreatedAtUtc",
                table: "ComplaintMessages",
                columns: new[] { "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchVisitRatings");

            migrationBuilder.DropTable(
                name: "ComplaintMessages");

            migrationBuilder.DropTable(
                name: "BranchVisits");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BranchId_WorkflowStatus_BranchManagerReadAtUtc_Sales~",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BranchManagerReadAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SalesCompletedAtUtc",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BranchId",
                table: "Orders",
                column: "BranchId");
        }
    }
}
