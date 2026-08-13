using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_Relationship_Between_Order_And_Purchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Orders_OrderId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_OrderId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Purchases",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "PurchaseItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ForeignProductName",
                table: "PurchaseItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_OrderId",
                table: "Purchases",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Orders_OrderId",
                table: "Purchases",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Orders_OrderId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_OrderId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ForeignProductName",
                table: "PurchaseItems");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "PurchaseItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrderId",
                table: "Transactions",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Orders_OrderId",
                table: "Transactions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
