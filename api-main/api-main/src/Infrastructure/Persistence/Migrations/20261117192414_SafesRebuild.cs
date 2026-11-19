using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SafesRebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // تمهيد الترحيل: القاصة الرئيسية (BranchId IS NULL) لا مكان لها في النموذج الجديد.
            // إن كان رصيدها صفراً تُحذف، وإلا تُسنَد لأول فرع كي لا يضيع رصيدها ولا ينكسر المفتاح الأجنبي.
            migrationBuilder.Sql(@"
                DELETE FROM ""Safes""
                WHERE ""BranchId"" IS NULL AND COALESCE(""RemainingCashAmount"", 0) = 0;

                UPDATE ""Safes""
                SET ""BranchId"" = (SELECT MIN(""Id"") FROM ""Branches"")
                WHERE ""BranchId"" IS NULL;

                DELETE FROM ""Safes"" WHERE ""BranchId"" IS NULL;
            ");

            migrationBuilder.DropTable(
                name: "SafeTransferTransactions");

            migrationBuilder.DropTable(
                name: "SellerCashDeliveryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Safes_BranchId",
                table: "Safes");

            migrationBuilder.DropColumn(
                name: "CreditAmount",
                table: "Safes");

            migrationBuilder.DropColumn(
                name: "DebitAmount",
                table: "Safes");

            migrationBuilder.RenameColumn(
                name: "RemainingCashAmount",
                table: "Safes",
                newName: "Balance");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Safes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "Safes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // الخزائن القائمة تبقى عاملة — الافتراضي false كان سيوقفها كلها
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Safes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "Safes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "Kind",
                table: "Safes",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "OrderListId",
                table: "Safes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Safes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportKey",
                table: "Orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "InstallmentPayments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "InstallmentPayments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportKey",
                table: "InstallmentPayments",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportKey",
                table: "Customers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SafeMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SafeId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<byte>(type: "smallint", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    BalanceAfter = table.Column<double>(type: "double precision", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PerformedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CounterpartySafeId = table.Column<int>(type: "integer", nullable: true),
                    TransferGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationKey = table.Column<Guid>(type: "uuid", nullable: true),
                    InstallmentPaymentId = table.Column<int>(type: "integer", nullable: true),
                    ExpenseId = table.Column<int>(type: "integer", nullable: true),
                    PurchaseId = table.Column<int>(type: "integer", nullable: true),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    ImportKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafeMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafeMovements_Safes_CounterpartySafeId",
                        column: x => x.CounterpartySafeId,
                        principalTable: "Safes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SafeMovements_Safes_SafeId",
                        column: x => x.SafeId,
                        principalTable: "Safes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Safes_Branch_Kind",
                table: "Safes",
                columns: new[] { "BranchId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_Safes_UserId",
                table: "Safes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_Safes_Employee",
                table: "Safes",
                columns: new[] { "BranchId", "UserId" },
                unique: true,
                filter: "\"UserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Safes_ExpensesSafe",
                table: "Safes",
                column: "BranchId",
                unique: true,
                filter: "\"Kind\" = 1");

            migrationBuilder.CreateIndex(
                name: "UX_Safes_OrderList",
                table: "Safes",
                column: "OrderListId",
                unique: true,
                filter: "\"OrderListId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Orders_ImportKey",
                table: "Orders",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPayments_CustomerId",
                table: "InstallmentPayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UX_InstallmentPayments_ImportKey",
                table: "InstallmentPayments",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Customers_ImportKey",
                table: "Customers",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SafeMovements_CounterpartySafeId",
                table: "SafeMovements",
                column: "CounterpartySafeId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeMovements_Safe_Id",
                table: "SafeMovements",
                columns: new[] { "SafeId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SafeMovements_TransferGroup",
                table: "SafeMovements",
                column: "TransferGroupId",
                filter: "\"TransferGroupId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_SafeMovements_ImportKey",
                table: "SafeMovements",
                column: "ImportKey",
                unique: true,
                filter: "\"ImportKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_SafeMovements_OperationKey",
                table: "SafeMovements",
                column: "OperationKey",
                unique: true,
                filter: "\"OperationKey\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InstallmentPayments_Customers_CustomerId",
                table: "InstallmentPayments",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Safes_OrderLists_OrderListId",
                table: "Safes",
                column: "OrderListId",
                principalTable: "OrderLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Safes_Users_UserId",
                table: "Safes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ===== ترحيل البيانات =====
            // 1) خزائن الفروع القائمة تُوسم قاصة فرع نظامية.
            migrationBuilder.Sql(@"
                UPDATE ""Safes"" SET ""Kind"" = 0, ""IsSystem"" = true, ""IsActive"" = true
                WHERE ""OrderListId"" IS NULL AND ""UserId"" IS NULL;
            ");

            // 2) سجل صرفيات لكل فرع — يستقبل كل سحب لاحقاً.
            migrationBuilder.Sql(@"
                INSERT INTO ""Safes"" (""Name"", ""Kind"", ""BranchId"", ""Balance"", ""IsSystem"", ""IsActive"", ""CreatedBy"", ""LastUpdatedBy"")
                SELECT 'سجل الصرفيات — ' || b.""Name"", 1, b.""Id"", 0, true, true, -1, -1
                FROM ""Branches"" b
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Safes"" s WHERE s.""BranchId"" = b.""Id"" AND s.""Kind"" = 1
                );
            ");

            // 3) حفظ التاريخ: صفوف Transactions القديمة تُنقل إلى السجل الجديد بمفتاح استيراد.
            //    الإشارة من Direction (0=داخل، 1=خارج). BalanceAfter يُصفَّر لأن التسلسل التاريخي غير معروف.
            migrationBuilder.Sql(@"
                INSERT INTO ""SafeMovements""
                    (""SafeId"", ""Kind"", ""Amount"", ""BalanceAfter"", ""Note"", ""PerformedByUserId"",
                     ""ExpenseId"", ""PurchaseId"", ""ImportKey"", ""CreatedAt"", ""CreatedBy"", ""LastUpdatedBy"")
                SELECT
                    t.""SafeId"",
                    CASE WHEN t.""Direction"" = 0 THEN 9 ELSE 7 END,
                    CASE WHEN t.""Direction"" = 0 THEN ABS(t.""Amount"") ELSE -ABS(t.""Amount"") END,
                    0,
                    'حركة تاريخية مُرحَّلة من Transactions',
                    COALESCE(t.""CreatedBy"", -1),
                    t.""ExpenseId"",
                    t.""PurchaseId"",
                    'legacy|Transaction|' || t.""Id"",
                    t.""CreatedAt"",
                    COALESCE(t.""CreatedBy"", -1),
                    COALESCE(t.""LastUpdatedBy"", -1)
                FROM ""Transactions"" t
                WHERE t.""Status"" = 1
                  AND EXISTS (SELECT 1 FROM ""Safes"" s WHERE s.""Id"" = t.""SafeId"")
                  AND NOT EXISTS (
                      SELECT 1 FROM ""SafeMovements"" m
                      WHERE m.""ImportKey"" = 'legacy|Transaction|' || t.""Id""
                  );
            ");

            // 4) رصيد افتتاحي يوازن الفرق بين الرصيد المخزون ومجموع الحركات المُرحَّلة،
            //    فيصبح Balance = SUM(Amount) لكل خزينة ويمرّ التحقق الحسابي.
            migrationBuilder.Sql(@"
                INSERT INTO ""SafeMovements""
                    (""SafeId"", ""Kind"", ""Amount"", ""BalanceAfter"", ""Note"", ""PerformedByUserId"",
                     ""ImportKey"", ""CreatedAt"", ""CreatedBy"", ""LastUpdatedBy"")
                SELECT
                    s.""Id"", 0,
                    s.""Balance"" - COALESCE(m.""Total"", 0),
                    s.""Balance"",
                    'رصيد افتتاحي عند تحويل نموذج الخزائن',
                    -1,
                    'legacy|Opening|' || s.""Id"",
                    now(),
                    -1, -1
                FROM ""Safes"" s
                LEFT JOIN (
                    SELECT ""SafeId"", SUM(""Amount"") AS ""Total"" FROM ""SafeMovements"" GROUP BY ""SafeId""
                ) m ON m.""SafeId"" = s.""Id""
                WHERE ABS(s.""Balance"" - COALESCE(m.""Total"", 0)) > 0.001
                  AND NOT EXISTS (
                      SELECT 1 FROM ""SafeMovements"" o
                      WHERE o.""ImportKey"" = 'legacy|Opening|' || s.""Id""
                  );
            ");

            // 5) إعادة ضبط BalanceAfter تصاعدياً حسب ترتيب المعرّف حتى يتسق التدقيق.
            migrationBuilder.Sql(@"
                WITH ordered AS (
                    SELECT ""Id"", ""SafeId"",
                           SUM(""Amount"") OVER (PARTITION BY ""SafeId"" ORDER BY ""Id""
                                                ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS ""Running""
                    FROM ""SafeMovements""
                )
                UPDATE ""SafeMovements"" m
                SET ""BalanceAfter"" = o.""Running""
                FROM ordered o
                WHERE o.""Id"" = m.""Id"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstallmentPayments_Customers_CustomerId",
                table: "InstallmentPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Safes_OrderLists_OrderListId",
                table: "Safes");

            migrationBuilder.DropForeignKey(
                name: "FK_Safes_Users_UserId",
                table: "Safes");

            migrationBuilder.DropTable(
                name: "SafeMovements");

            migrationBuilder.DropIndex(
                name: "IX_Safes_Branch_Kind",
                table: "Safes");

            migrationBuilder.DropIndex(
                name: "IX_Safes_UserId",
                table: "Safes");

            migrationBuilder.DropIndex(
                name: "UX_Safes_Employee",
                table: "Safes");

            migrationBuilder.DropIndex(
                name: "UX_Safes_ExpensesSafe",
                table: "Safes");

            migrationBuilder.DropIndex(
                name: "UX_Safes_OrderList",
                table: "Safes");

            migrationBuilder.DropIndex(
                name: "UX_Orders_ImportKey",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_InstallmentPayments_CustomerId",
                table: "InstallmentPayments");

            migrationBuilder.DropIndex(
                name: "UX_InstallmentPayments_ImportKey",
                table: "InstallmentPayments");

            migrationBuilder.DropIndex(
                name: "UX_Customers_ImportKey",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Safes");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "Safes");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Safes");

            migrationBuilder.DropColumn(
                name: "OrderListId",
                table: "Safes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Safes");

            migrationBuilder.DropColumn(
                name: "ImportKey",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "InstallmentPayments");

            migrationBuilder.DropColumn(
                name: "ImportKey",
                table: "InstallmentPayments");

            migrationBuilder.DropColumn(
                name: "ImportKey",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "Safes",
                newName: "RemainingCashAmount");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Safes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "Safes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<double>(
                name: "CreditAmount",
                table: "Safes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DebitAmount",
                table: "Safes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "InstallmentPayments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SafeTransferTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DestinationSafeId = table.Column<int>(type: "integer", nullable: false),
                    SourceSafeId = table.Column<int>(type: "integer", nullable: true),
                    TransactionId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafeTransferTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafeTransferTransactions_Safes_DestinationSafeId",
                        column: x => x.DestinationSafeId,
                        principalTable: "Safes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SafeTransferTransactions_Safes_SourceSafeId",
                        column: x => x.SourceSafeId,
                        principalTable: "Safes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SafeTransferTransactions_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SellerCashDeliveryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SellerId = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerCashDeliveryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerCashDeliveryTransactions_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SellerCashDeliveryTransactions_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Safes_BranchId",
                table: "Safes",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SafeTransferTransactions_DestinationSafeId",
                table: "SafeTransferTransactions",
                column: "DestinationSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeTransferTransactions_SourceSafeId",
                table: "SafeTransferTransactions",
                column: "SourceSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeTransferTransactions_TransactionId",
                table: "SafeTransferTransactions",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerCashDeliveryTransactions_SellerId",
                table: "SellerCashDeliveryTransactions",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerCashDeliveryTransactions_TransactionId",
                table: "SellerCashDeliveryTransactions",
                column: "TransactionId",
                unique: true);
        }
    }
}
