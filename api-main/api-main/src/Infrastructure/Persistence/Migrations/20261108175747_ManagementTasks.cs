using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ManagementTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagementTaskChangeOutboxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    CurrentHolderUserId = table.Column<int>(type: "integer", nullable: false),
                    AssigneeUserId = table.Column<int>(type: "integer", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementTaskChangeOutboxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagementTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    Priority = table.Column<byte>(type: "smallint", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    AssigneeUserId = table.Column<int>(type: "integer", nullable: false),
                    AssigneeDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CurrentHolderUserId = table.Column<int>(type: "integer", nullable: false),
                    CurrentHolderDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RejectNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TransferredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BranchCompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssigneeCompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementTasks_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementTasks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementTasks_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementTasks_Users_AssigneeUserId",
                        column: x => x.AssigneeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementTasks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementTasks_Users_CurrentHolderUserId",
                        column: x => x.CurrentHolderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManagementTaskAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "integer", nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementTaskAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementTaskAttachments_ManagementTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "ManagementTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManagementTaskEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<byte>(type: "smallint", nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<int>(type: "integer", nullable: true),
                    ActorDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TargetUserId = table.Column<int>(type: "integer", nullable: true),
                    TargetDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PreviousStatus = table.Column<byte>(type: "smallint", nullable: true),
                    NewStatus = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementTaskEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementTaskEvents_ManagementTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "ManagementTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskAttachments_TaskId",
                table: "ManagementTaskAttachments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskChangeOutboxes_EventId",
                table: "ManagementTaskChangeOutboxes",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskChangeOutboxes_PublishedAtUtc_Id",
                table: "ManagementTaskChangeOutboxes",
                columns: new[] { "PublishedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskChangeOutboxes_TaskId_Revision",
                table: "ManagementTaskChangeOutboxes",
                columns: new[] { "TaskId", "Revision" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTaskEvents_TaskId_AtUtc",
                table: "ManagementTaskEvents",
                columns: new[] { "TaskId", "AtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTasks_AssigneeUserId",
                table: "ManagementTasks",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTasks_BranchId_Status",
                table: "ManagementTasks",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTasks_CreatedByUserId",
                table: "ManagementTasks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTasks_CurrentHolderUserId_Status",
                table: "ManagementTasks",
                columns: new[] { "CurrentHolderUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTasks_CustomerId",
                table: "ManagementTasks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTasks_DueDate",
                table: "ManagementTasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementTasks_OrderId",
                table: "ManagementTasks",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagementTaskAttachments");

            migrationBuilder.DropTable(
                name: "ManagementTaskChangeOutboxes");

            migrationBuilder.DropTable(
                name: "ManagementTaskEvents");

            migrationBuilder.DropTable(
                name: "ManagementTasks");
        }
    }
}
