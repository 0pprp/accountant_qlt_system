using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FollowerDelegateNotesAndCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectionToggleVersion",
                table: "OrderLists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FollowerCollectionAssignedAtUtc",
                table: "OrderLists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FollowerCollectionEnabled",
                table: "OrderLists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FollowerDelegateNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderListId = table.Column<int>(type: "integer", nullable: false),
                    MandobUserId = table.Column<int>(type: "integer", nullable: false),
                    NoteText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedByRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowerDelegateNotes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerDelegateNotes_CreatedByUserId_CreatedAtUtc",
                table: "FollowerDelegateNotes",
                columns: new[] { "CreatedByUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerDelegateNotes_OrderListId_CreatedAtUtc",
                table: "FollowerDelegateNotes",
                columns: new[] { "OrderListId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowerDelegateNotes");

            migrationBuilder.DropColumn(
                name: "CollectionToggleVersion",
                table: "OrderLists");

            migrationBuilder.DropColumn(
                name: "FollowerCollectionAssignedAtUtc",
                table: "OrderLists");

            migrationBuilder.DropColumn(
                name: "FollowerCollectionEnabled",
                table: "OrderLists");
        }
    }
}
