using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FollowerFieldFollowups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowerFieldFollowups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerUserId = table.Column<int>(type: "integer", nullable: false),
                    ListId = table.Column<int>(type: "integer", nullable: false),
                    ListName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompletionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowerFieldFollowups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerFieldFollowups_Follower_Status",
                table: "FollowerFieldFollowups",
                columns: new[] { "FollowerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerFieldFollowups_List_Status",
                table: "FollowerFieldFollowups",
                columns: new[] { "FollowerUserId", "ListId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerFieldFollowups_PendingLookup",
                table: "FollowerFieldFollowups",
                columns: new[] { "FollowerUserId", "ListId", "CustomerId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowerFieldFollowups");
        }
    }
}
