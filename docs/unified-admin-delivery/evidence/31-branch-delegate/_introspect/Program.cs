using Npgsql;
var cs="Host=localhost;Port=5432;Database=qalaat_22_23_iso;Username=postgres;Password=postgres";
await using var c=new NpgsqlConnection(cs); await c.OpenAsync();
async Task Dump(string sql,string title){ Console.WriteLine("==="+title+"==="); await using var cmd=c.CreateCommand(); cmd.CommandText=sql; await using var r=await cmd.ExecuteReaderAsync(); while(await r.ReadAsync()){ var parts=new List<string>(); for(int i=0;i<r.FieldCount;i++) parts.Add(Convert.ToString(r.GetValue(i))??""); Console.WriteLine(string.Join("\t",parts)); } }
await Dump("SELECT column_name,is_nullable,data_type FROM information_schema.columns WHERE table_name='Branches' ORDER BY ordinal_position","BRANCH_COLS");
await Dump("SELECT column_name,is_nullable,data_type FROM information_schema.columns WHERE table_name='UserBranches' ORDER BY ordinal_position","UB_COLS");
