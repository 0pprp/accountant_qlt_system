using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LegalFollowupFieldDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ShopLatitude",
                table: "LegalFollowupCases",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShopLongitude",
                table: "LegalFollowupCases",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LegalFollowupCaseDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaseId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "integer", nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalFollowupCaseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalFollowupCaseDocuments_LegalFollowupCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "LegalFollowupCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalFollowupCaseDocuments_CaseId_DocumentType",
                table: "LegalFollowupCaseDocuments",
                columns: new[] { "CaseId", "DocumentType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegalFollowupCaseDocuments");

            migrationBuilder.DropColumn(
                name: "ShopLatitude",
                table: "LegalFollowupCases");

            migrationBuilder.DropColumn(
                name: "ShopLongitude",
                table: "LegalFollowupCases");
        }
    }
}
