using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MobileAppReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MobileAppReleases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LatestVersionName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LatestVersionCode = table.Column<int>(type: "integer", nullable: false),
                    MinimumSupportedVersionCode = table.Column<int>(type: "integer", nullable: false),
                    ForceUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    ApkUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReleaseNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    UpdatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileAppReleases", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MobileAppReleases_AppKey",
                table: "MobileAppReleases",
                column: "AppKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileAppReleases");
        }
    }
}
