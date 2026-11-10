using Npgsql;
var mode = args.ElementAtOrDefault(0) ?? "status";
var cs = args.ElementAtOrDefault(1) ?? "Host=localhost;Port=5432;Database=qalaat_22_23_iso;Username=postgres;Password=postgres";
await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();
Console.WriteLine($"db={conn.Database} mode={mode}");
async Task<object?> Scalar(string sql){ await using var cmd=conn.CreateCommand(); cmd.CommandText=sql; return await cmd.ExecuteScalarAsync(); }
if(mode=="status"){
 Console.WriteLine("migrations="+await Scalar("SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
 Console.WriteLine("latest="+await Scalar("SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 1"));
 Console.WriteLine("users="+await Scalar("SELECT COUNT(*) FROM \"Users\""));
 Console.WriteLine("DONE"); return;
}
if(mode=="sell"){ Console.WriteLine("sell="+await Scalar("SELECT \"SellAmount\" FROM \"Orders\" WHERE \"Id\"=1")); return; }
if(mode=="dump"){
 async Task Dump(string t,string sql){ Console.WriteLine("--- "+t); await using var cmd=conn.CreateCommand(); cmd.CommandText=sql; await using var r=await cmd.ExecuteReaderAsync(); while(await r.ReadAsync()){ var p=new object[r.FieldCount]; r.GetValues(p); Console.WriteLine(string.Join("\t", p.Select(x=>x?.ToString()??"null"))); } }
 await Dump("USERS","SELECT u.\"Id\", u.\"Username\", COALESCE(string_agg(r.\"Name\",','),'') FROM \"Users\" u LEFT JOIN \"UserRoles\" ur ON ur.\"UserId\"=u.\"Id\" LEFT JOIN \"Roles\" r ON r.\"Id\"=ur.\"RoleId\" GROUP BY u.\"Id\",u.\"Username\" ORDER BY u.\"Id\"");
 await Dump("ORDERS","SELECT \"Id\",\"CustomerId\",\"SellAmount\",\"BranchId\" FROM \"Orders\" ORDER BY \"Id\"");
 return;
}
Console.WriteLine("unknown");
