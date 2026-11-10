using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WaiversAndCustomerDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFakeSale",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CustomerDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<int>(type: "integer", nullable: false),
                    ActorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DecisionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    WeekPaid = table.Column<double>(type: "double precision", nullable: true),
                    AmountTotalSales = table.Column<double>(type: "double precision", nullable: true),
                    PaidPercent = table.Column<double>(type: "double precision", nullable: true),
                    SnoozeUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RelatedWaiverRequestId = table.Column<int>(type: "integer", nullable: true),
                    RelatedLegalConversionId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDecisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerWaiverRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    RequesterUserId = table.Column<int>(type: "integer", nullable: false),
                    RequesterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<int>(type: "integer", nullable: true),
                    DecidedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApprovedAmount = table.Column<double>(type: "double precision", nullable: true),
                    AppliedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AppliedByUserId = table.Column<int>(type: "integer", nullable: true),
                    AppliedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AppliedAmount = table.Column<double>(type: "double precision", nullable: true),
                    Applied = table.Column<bool>(type: "boolean", nullable: false),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerWaiverRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalConversionRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    RequesterUserId = table.Column<int>(type: "integer", nullable: false),
                    RequesterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<int>(type: "integer", nullable: true),
                    DecidedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalConversionRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDecisions_BranchId_CreatedAtUtc_Id",
                table: "CustomerDecisions",
                columns: new[] { "BranchId", "CreatedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDecisions_CustomerId_CreatedAtUtc",
                table: "CustomerDecisions",
                columns: new[] { "CustomerId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDecisions_DecisionType",
                table: "CustomerDecisions",
                column: "DecisionType");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWaiverRequests_BranchId_Status",
                table: "CustomerWaiverRequests",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWaiverRequests_CustomerId",
                table: "CustomerWaiverRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWaiverRequests_Status_CreatedAtUtc_Id",
                table: "CustomerWaiverRequests",
                columns: new[] { "Status", "CreatedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_LegalConversionRequests_BranchId",
                table: "LegalConversionRequests",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalConversionRequests_CustomerId_Status",
                table: "LegalConversionRequests",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LegalConversionRequests_Status_CreatedAtUtc_Id",
                table: "LegalConversionRequests",
                columns: new[] { "Status", "CreatedAtUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDecisions");

            migrationBuilder.DropTable(
                name: "CustomerWaiverRequests");

            migrationBuilder.DropTable(
                name: "LegalConversionRequests");

            migrationBuilder.DropColumn(
                name: "IsFakeSale",
                table: "Customers");
        }
    }
}
