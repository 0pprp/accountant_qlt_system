using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Rename_Incoming_And_Outgoing_Amount_In_Safe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutgoingAmount",
                table: "Safes",
                newName: "DebitAmount");

            migrationBuilder.RenameColumn(
                name: "IncomingAmount",
                table: "Safes",
                newName: "CreditAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DebitAmount",
                table: "Safes",
                newName: "OutgoingAmount");

            migrationBuilder.RenameColumn(
                name: "CreditAmount",
                table: "Safes",
                newName: "IncomingAmount");
        }
    }
}
