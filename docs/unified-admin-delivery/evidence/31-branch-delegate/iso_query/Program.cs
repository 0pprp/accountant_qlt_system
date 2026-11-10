using Npgsql;

var cs = args.Length > 0
    ? args[0]
    : "Host=localhost;Port=5432;Database=qalaat_22_23_iso;Username=postgres;Password=postgres";

await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();
Console.WriteLine("OK connected " + conn.Database);

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        SELECT u."Id", u."Username", u."FullName",
               COALESCE(string_agg(DISTINCT r."Name", ','), '') AS roles,
               COALESCE(string_agg(DISTINCT ub."BranchId"::text, ','), '') AS branches
        FROM "Users" u
        LEFT JOIN "UserRoles" ur ON ur."UserId" = u."Id"
        LEFT JOIN "Roles" r ON r."Id" = ur."RoleId"
        LEFT JOIN "UserBranches" ub ON ub."UserId" = u."Id"
        GROUP BY u."Id", u."Username", u."FullName"
        ORDER BY u."Id"
        LIMIT 50;
        """;
    await using var r = await cmd.ExecuteReaderAsync();
    Console.WriteLine("===USERS===");
    while (await r.ReadAsync())
        Console.WriteLine($"{r.GetInt32(0)}\t{r.GetString(1)}\t{r.GetString(2)}\t{r.GetString(3)}\t{r.GetString(4)}");
}

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        SELECT l."Id", l."Name", l."BranchId", l."MandobId"
        FROM "OrderLists" l
        ORDER BY l."Id" LIMIT 30;
        """;
    await using var r = await cmd.ExecuteReaderAsync();
    Console.WriteLine("===ORDERLISTS===");
    while (await r.ReadAsync())
        Console.WriteLine($"{r.GetInt32(0)}\t{r.GetString(1)}\t{r.GetInt32(2)}\t{(r.IsDBNull(3) ? "null" : r.GetInt32(3).ToString())}");
}

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        SELECT column_name FROM information_schema.columns
        WHERE table_name = 'Orders' ORDER BY ordinal_position;
        """;
    await using var r = await cmd.ExecuteReaderAsync();
    Console.WriteLine("===ORDERS_COLS===");
    while (await r.ReadAsync()) Console.WriteLine(r.GetString(0));
}

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        SELECT o."Id", o."CustomerId", o."OrderListId", o."BranchId"
        FROM "Orders" o
        ORDER BY o."Id" DESC LIMIT 20;
        """;
    await using var r = await cmd.ExecuteReaderAsync();
    Console.WriteLine("===ORDERS===");
    while (await r.ReadAsync())
        Console.WriteLine($"{r.GetInt32(0)}\t{r.GetInt32(1)}\t{(r.IsDBNull(2) ? "null" : r.GetInt32(2).ToString())}\t{r.GetInt32(3)}");
}

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """SELECT current_database(), (SELECT count(*) FROM "OrderLists"), (SELECT count(*) FROM "Customers"), (SELECT count(*) FROM "BranchVisits"), (SELECT count(*) FROM "WrongSaleNotices");""";
    await using var r = await cmd.ExecuteReaderAsync();
    Console.WriteLine("===COUNTS===");
    while (await r.ReadAsync())
        Console.WriteLine($"db={r.GetString(0)} lists={r.GetInt64(1)} customers={r.GetInt64(2)} visits={r.GetInt64(3)} wrong={r.GetInt64(4)}");
}

await using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        SELECT "MigrationId" FROM "__EFMigrationsHistory"
        WHERE "MigrationId" LIKE '%Branch31%' OR "MigrationId" LIKE '%WrongSale%'
        ORDER BY "MigrationId";
        """;
    await using var r = await cmd.ExecuteReaderAsync();
    Console.WriteLine("===MIGRATIONS31===");
    while (await r.ReadAsync())
        Console.WriteLine(r.GetString(0));
}
