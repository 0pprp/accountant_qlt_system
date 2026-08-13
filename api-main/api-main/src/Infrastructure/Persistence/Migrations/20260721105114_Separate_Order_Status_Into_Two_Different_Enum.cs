using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Separate_Order_Status_Into_Two_Different_Enum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Orders",
                newName: "ExecutionStatus");

            migrationBuilder.AddColumn<byte>(
                name: "ApprovalStatus",
                table: "Orders",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ExecutionStatus",
                table: "Orders",
                newName: "Status");
        }
    }
}
