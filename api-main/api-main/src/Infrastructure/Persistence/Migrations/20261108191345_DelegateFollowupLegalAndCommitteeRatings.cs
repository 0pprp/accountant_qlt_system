using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DelegateFollowupLegalAndCommitteeRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLegal",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FollowerDelegateDayRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderListId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    FollowerUserId = table.Column<int>(type: "integer", nullable: false),
                    RatingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowerDelegateDayRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowerDelegateDayRatings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FollowerDelegateDayRatings_OrderLists_OrderListId",
                        column: x => x.OrderListId,
                        principalTable: "OrderLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FollowerDelegateDayRatings_Users_FollowerUserId",
                        column: x => x.FollowerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListSupervisionDayRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderListId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    RatingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    RatedByUserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListSupervisionDayRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListSupervisionDayRatings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListSupervisionDayRatings_OrderLists_OrderListId",
                        column: x => x.OrderListId,
                        principalTable: "OrderLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesEmployeeListDayRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderListId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeUserId = table.Column<int>(type: "integer", nullable: false),
                    RatingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesEmployeeListDayRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesEmployeeListDayRatings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesEmployeeListDayRatings_OrderLists_OrderListId",
                        column: x => x.OrderListId,
                        principalTable: "OrderLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesEmployeeListDayRatings_Users_EmployeeUserId",
                        column: x => x.EmployeeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerDelegateDayRatings_BranchId",
                table: "FollowerDelegateDayRatings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowerDelegateDayRatings_FollowerUserId_OrderListId_Ratin~",
                table: "FollowerDelegateDayRatings",
                columns: new[] { "FollowerUserId", "OrderListId", "RatingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowerDelegateDayRatings_OrderListId_RatingDate",
                table: "FollowerDelegateDayRatings",
                columns: new[] { "OrderListId", "RatingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ListSupervisionDayRatings_BranchId_RatingDate",
                table: "ListSupervisionDayRatings",
                columns: new[] { "BranchId", "RatingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ListSupervisionDayRatings_OrderListId_RatingDate",
                table: "ListSupervisionDayRatings",
                columns: new[] { "OrderListId", "RatingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesEmployeeListDayRatings_BranchId",
                table: "SalesEmployeeListDayRatings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesEmployeeListDayRatings_EmployeeUserId_OrderListId_Rati~",
                table: "SalesEmployeeListDayRatings",
                columns: new[] { "EmployeeUserId", "OrderListId", "RatingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesEmployeeListDayRatings_OrderListId_RatingDate",
                table: "SalesEmployeeListDayRatings",
                columns: new[] { "OrderListId", "RatingDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowerDelegateDayRatings");

            migrationBuilder.DropTable(
                name: "ListSupervisionDayRatings");

            migrationBuilder.DropTable(
                name: "SalesEmployeeListDayRatings");

            migrationBuilder.DropColumn(
                name: "IsLegal",
                table: "Customers");
        }
    }
}
