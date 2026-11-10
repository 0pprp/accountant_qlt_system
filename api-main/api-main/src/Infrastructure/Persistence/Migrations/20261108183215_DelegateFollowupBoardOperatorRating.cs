using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DelegateFollowupBoardOperatorRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectorBoardOperatorListRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderListId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    RatingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    RatedByUserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectorBoardOperatorListRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectorBoardOperatorListRatings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DirectorBoardOperatorListRatings_OrderLists_OrderListId",
                        column: x => x.OrderListId,
                        principalTable: "OrderLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectorBoardOperatorListRatings_BranchId_RatingDate",
                table: "DirectorBoardOperatorListRatings",
                columns: new[] { "BranchId", "RatingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DirectorBoardOperatorListRatings_OrderListId_RatingDate",
                table: "DirectorBoardOperatorListRatings",
                columns: new[] { "OrderListId", "RatingDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectorBoardOperatorListRatings");
        }
    }
}
