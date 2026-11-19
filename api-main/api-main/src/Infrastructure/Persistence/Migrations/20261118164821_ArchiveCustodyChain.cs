using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveCustodyChain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveBundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    SalesEmployeeUserId = table.Column<int>(type: "integer", nullable: false),
                    SalesEmployeeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PeriodIndex = table.Column<byte>(type: "smallint", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    SentCount = table.Column<int>(type: "integer", nullable: false),
                    ArchivedCount = table.Column<int>(type: "integer", nullable: false),
                    OpenReturnCount = table.Column<int>(type: "integer", nullable: false),
                    FirstSentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveBundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveBundles_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchiveBundles_Users_SalesEmployeeUserId",
                        column: x => x.SalesEmployeeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveChangeOutboxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<int>(type: "integer", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TargetStage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    SalesEmployeeUserId = table.Column<int>(type: "integer", nullable: false),
                    FollowerUserId = table.Column<int>(type: "integer", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveChangeOutboxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FollowerUserId = table.Column<int>(type: "integer", nullable: true),
                    PeriodSourceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodSource = table.Column<byte>(type: "smallint", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    ConcurrencyStamp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsVoided = table.Column<bool>(type: "boolean", nullable: false),
                    VoidReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VoidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HasOpenReturn = table.Column<bool>(type: "boolean", nullable: false),
                    ReturnCount = table.Column<int>(type: "integer", nullable: false),
                    LastReturnReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastReturnedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SentToBranchAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedByBranchAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedByBranchUserId = table.Column<int>(type: "integer", nullable: true),
                    SentToFollowerAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedByFollowerAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedByFollowerUserId = table.Column<int>(type: "integer", nullable: true),
                    SentToArchiveAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveTransactions_ArchiveBundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "ArchiveBundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchiveTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchiveTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchiveTransactions_Users_FollowerUserId",
                        column: x => x.FollowerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveTransactionEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionId = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<byte>(type: "smallint", nullable: false),
                    PreviousStatus = table.Column<byte>(type: "smallint", nullable: false),
                    NewStatus = table.Column<byte>(type: "smallint", nullable: false),
                    Stage = table.Column<byte>(type: "smallint", nullable: true),
                    ActorUserId = table.Column<int>(type: "integer", nullable: false),
                    ActorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveTransactionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveTransactionEvents_ArchiveTransactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "ArchiveTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveBundles_BranchId_PeriodYear_PeriodMonth_PeriodIndex",
                table: "ArchiveBundles",
                columns: new[] { "BranchId", "PeriodYear", "PeriodMonth", "PeriodIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveBundles_SalesEmployeeUserId",
                table: "ArchiveBundles",
                column: "SalesEmployeeUserId");

            migrationBuilder.CreateIndex(
                name: "UX_ArchiveBundles_Employee_Period",
                table: "ArchiveBundles",
                columns: new[] { "BranchId", "SalesEmployeeUserId", "PeriodYear", "PeriodMonth", "PeriodIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveChangeOutboxes_EventId",
                table: "ArchiveChangeOutboxes",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveChangeOutboxes_PublishedAtUtc_Id",
                table: "ArchiveChangeOutboxes",
                columns: new[] { "PublishedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTransactionEvents_OperationId",
                table: "ArchiveTransactionEvents",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTransactionEvents_TransactionId_AtUtc",
                table: "ArchiveTransactionEvents",
                columns: new[] { "TransactionId", "AtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTransactions_BundleId_Status",
                table: "ArchiveTransactions",
                columns: new[] { "BundleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTransactions_CustomerId",
                table: "ArchiveTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTransactions_FollowerUserId_Status",
                table: "ArchiveTransactions",
                columns: new[] { "FollowerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_ArchiveTransactions_OrderId",
                table: "ArchiveTransactions",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveChangeOutboxes");

            migrationBuilder.DropTable(
                name: "ArchiveTransactionEvents");

            migrationBuilder.DropTable(
                name: "ArchiveTransactions");

            migrationBuilder.DropTable(
                name: "ArchiveBundles");
        }
    }
}
