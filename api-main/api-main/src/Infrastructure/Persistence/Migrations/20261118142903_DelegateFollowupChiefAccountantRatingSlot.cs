using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DelegateFollowupChiefAccountantRatingSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DirectorBoardOperatorListRatings_OrderListId_RatingDate",
                table: "DirectorBoardOperatorListRatings");

            migrationBuilder.AddColumn<string>(
                name: "RaterSlot",
                table: "DirectorBoardOperatorListRatings",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "head");

            migrationBuilder.CreateIndex(
                name: "IX_DirectorBoardOperatorListRatings_OrderListId_RatingDate_Rat~",
                table: "DirectorBoardOperatorListRatings",
                columns: new[] { "OrderListId", "RatingDate", "RaterSlot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DirectorBoardOperatorListRatings_OrderListId_RatingDate_Rat~",
                table: "DirectorBoardOperatorListRatings");

            migrationBuilder.DropColumn(
                name: "RaterSlot",
                table: "DirectorBoardOperatorListRatings");

            migrationBuilder.CreateIndex(
                name: "IX_DirectorBoardOperatorListRatings_OrderListId_RatingDate",
                table: "DirectorBoardOperatorListRatings",
                columns: new[] { "OrderListId", "RatingDate" },
                unique: true);
        }
    }
}
