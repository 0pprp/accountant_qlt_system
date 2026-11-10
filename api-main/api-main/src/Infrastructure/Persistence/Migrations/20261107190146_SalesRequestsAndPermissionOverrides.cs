using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SalesRequestsAndPermissionOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PermissionsVersion",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SalesRequestChangeOutbox",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    TargetEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    PreviousTargetEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesRequestChangeOutbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestedProduct = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SubmissionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SubmissionKey = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmissionFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceType = table.Column<byte>(type: "smallint", nullable: false),
                    OriginalExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    TargetEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ProvinceId = table.Column<int>(type: "integer", nullable: true),
                    ExistingCustomerId = table.Column<int>(type: "integer", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ExceptionHoldStatus = table.Column<byte>(type: "smallint", nullable: true),
                    RejectReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastStatusNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExceptionDecidedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ExceptionDecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExceptionConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "integer", nullable: true),
                    DeleteReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkedOrderId = table.Column<int>(type: "integer", nullable: true),
                    ViewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesRequests_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesRequests_Customers_ExistingCustomerId",
                        column: x => x.ExistingCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesRequests_Orders_LinkedOrderId",
                        column: x => x.LinkedOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesRequests_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesRequests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesRequests_Users_TargetEmployeeId",
                        column: x => x.TargetEmployeeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissionDenials",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissionDenials", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermissionDenials_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPermissionDenials_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesRequestTimelineEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesRequestId = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<byte>(type: "smallint", nullable: false),
                    ActorUserId = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreviousStatus = table.Column<byte>(type: "smallint", nullable: true),
                    NewStatus = table.Column<byte>(type: "smallint", nullable: true),
                    PreviousEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    NewEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesRequestTimelineEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesRequestTimelineEntries_SalesRequests_SalesRequestId",
                        column: x => x.SalesRequestId,
                        principalTable: "SalesRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequestChangeOutbox_EventId",
                table: "SalesRequestChangeOutbox",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequestChangeOutbox_PublishedAtUtc_Id",
                table: "SalesRequestChangeOutbox",
                columns: new[] { "PublishedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequestChangeOutbox_RequestId_Revision",
                table: "SalesRequestChangeOutbox",
                columns: new[] { "RequestId", "Revision" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_BranchId_DeletedAtUtc",
                table: "SalesRequests",
                columns: new[] { "BranchId", "DeletedAtUtc" },
                filter: "\"IsDeleted\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_BranchId_ExceptionHoldStatus",
                table: "SalesRequests",
                columns: new[] { "BranchId", "ExceptionHoldStatus" },
                filter: "\"ExceptionHoldStatus\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_BranchId_Status_TargetEmployeeId",
                table: "SalesRequests",
                columns: new[] { "BranchId", "Status", "TargetEmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_CreatedByUserId_SubmissionKey",
                table: "SalesRequests",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_CustomerName",
                table: "SalesRequests",
                column: "CustomerName");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_CustomerPhone",
                table: "SalesRequests",
                column: "CustomerPhone");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_ExistingCustomerId",
                table: "SalesRequests",
                column: "ExistingCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_LinkedOrderId",
                table: "SalesRequests",
                column: "LinkedOrderId",
                unique: true,
                filter: "\"LinkedOrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_OriginalExternalId",
                table: "SalesRequests",
                column: "OriginalExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_ProvinceId",
                table: "SalesRequests",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequests_TargetEmployeeId",
                table: "SalesRequests",
                column: "TargetEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequestTimelineEntries_OperationId",
                table: "SalesRequestTimelineEntries",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRequestTimelineEntries_SalesRequestId_OccurredAtUtc",
                table: "SalesRequestTimelineEntries",
                columns: new[] { "SalesRequestId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionDenials_PermissionId",
                table: "UserPermissionDenials",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesRequestChangeOutbox");

            migrationBuilder.DropTable(
                name: "SalesRequestTimelineEntries");

            migrationBuilder.DropTable(
                name: "UserPermissionDenials");

            migrationBuilder.DropTable(
                name: "SalesRequests");

            migrationBuilder.DropColumn(
                name: "PermissionsVersion",
                table: "Users");
        }
    }
}
