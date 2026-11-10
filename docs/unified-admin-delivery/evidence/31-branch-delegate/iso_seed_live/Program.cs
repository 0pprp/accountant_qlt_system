using Npgsql;

var cs = "Host=localhost;Port=5432;Database=qalaat_22_23_iso;Username=postgres;Password=postgres";
await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();

async Task Exec(string sql)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    var n = await cmd.ExecuteNonQueryAsync();
    Console.WriteLine($"SQL ok affected={n}");
}

async Task<object?> Scalar(string sql)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    return await cmd.ExecuteScalarAsync();
}

Console.WriteLine("db=" + await Scalar("SELECT current_database()"));

await Exec("""
UPDATE "Orders" SET
  "WorkflowStatus" = 5,
  "SalesCompletedAtUtc" = COALESCE("SalesCompletedAtUtc", NOW()),
  "BranchManagerReadAtUtc" = NULL
WHERE "Id" = 1;
""");

await Exec("""
UPDATE "Customers" SET
  "FullName" = 'احمد تحقق الفرع',
  "PhoneNumber" = CASE WHEN COALESCE("PhoneNumber",'')='' THEN '07801112233' ELSE "PhoneNumber" END
WHERE "Id" = 1;
""");

await Exec("""
INSERT INTO "InstallmentPayments" (
  "OrderId", "Amount", "Date", "Description", "CreatedAt", "CreatedBy", "LastUpdatedBy"
)
SELECT 1, 25000, (CURRENT_DATE - 2), 'iso-live-31', NOW(), 1, 1
WHERE NOT EXISTS (
  SELECT 1 FROM "InstallmentPayments" WHERE "OrderId" = 1 AND "Description" = 'iso-live-31'
);
""");

var bmExists = Convert.ToInt64(await Scalar("""SELECT COUNT(*) FROM "Users" WHERE "Username"='bm_iso'"""));
if (bmExists == 0)
{
    await Exec("""
    INSERT INTO "Users" (
      "FullName","MotherName","Username","PasswordHash","NationalCode","BirthDate",
      "PhoneNumber","Address","CreationStep","CreatedAt","CreatedBy","SecurityStamp",
      "UndeliveredCashAmount","PermissionsVersion","LastUpdatedBy"
    )
    SELECT
      'مدير فرع تحقق','أم','bm_iso',
      (SELECT "PasswordHash" FROM "Users" WHERE "Id"=1),
      'bm-iso-nc', DATE '1990-01-01',
      '0700999888','بغداد', 3, NOW(), 1, gen_random_uuid(),
      0, 0, 1;
    """);
}

var bmId = Convert.ToInt32(await Scalar("""SELECT "Id" FROM "Users" WHERE "Username"='bm_iso'"""));
var bmRoleId = Convert.ToInt32(await Scalar("""SELECT "Id" FROM "Roles" WHERE "Name"='BranchManager'"""));
await Exec($"""
INSERT INTO "UserRoles" ("UserId","RoleId")
SELECT {bmId}, {bmRoleId}
WHERE NOT EXISTS (SELECT 1 FROM "UserRoles" WHERE "UserId"={bmId} AND "RoleId"={bmRoleId});
""");
await Exec($"""
INSERT INTO "UserBranches" ("UserId","BranchId")
SELECT {bmId}, 1
WHERE NOT EXISTS (SELECT 1 FROM "UserBranches" WHERE "UserId"={bmId} AND "BranchId"=1);
""");

await Exec("""
INSERT INTO "Branches" ("Name","ProvinceId","CreatedAt","CreatedBy","LastUpdatedBy")
SELECT 'فرع ثانٍ تحقق', 1, NOW(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM "Branches" WHERE "Name"='فرع ثانٍ تحقق');
""");
var branch2 = Convert.ToInt32(await Scalar("""SELECT "Id" FROM "Branches" WHERE "Name"='فرع ثانٍ تحقق'"""));
await Exec($"""
INSERT INTO "UserBranches" ("UserId","BranchId")
SELECT 1, {branch2}
WHERE NOT EXISTS (SELECT 1 FROM "UserBranches" WHERE "UserId"=1 AND "BranchId"={branch2});
""");

// Grant Mandob InstallmentPayment.Create if missing via role (usually present)
Console.WriteLine($"bmId={bmId} branch2={branch2}");
Console.WriteLine("order=" + await Scalar("""SELECT "Id"||'|ws='||"WorkflowStatus"::text||'|soldAt='||COALESCE("SalesCompletedAtUtc"::text,'') FROM "Orders" WHERE "Id"=1"""));
Console.WriteLine("customer=" + await Scalar("""SELECT "FullName" FROM "Customers" WHERE "Id"=1"""));
Console.WriteLine("payments=" + await Scalar("""SELECT COUNT(*), COALESCE(SUM("Amount"),0) FROM "InstallmentPayments" WHERE "OrderId"=1"""));
Console.WriteLine("DONE");
