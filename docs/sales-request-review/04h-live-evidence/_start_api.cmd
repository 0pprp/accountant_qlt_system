set ASPNETCORE_ENVIRONMENT=Development
set ConnectionStrings__Postgres=Host=localhost;Port=5432;Database=qalaat_04f_sales_verify;Username=postgres;Password=postgres
set Phase02LocalVerify__AllowPendingModelChanges=true
set Phase02LocalVerify__AllowedDatabase=qalaat_04f_sales_verify
set ASPNETCORE_URLS=http://127.0.0.1:4200
set SeqUrl=
dotnet run --project "C:\GitHub\accountant_qlt_system\api-main\api-main\src\Api\Api.csproj" --no-launch-profile > "C:\GitHub\accountant_qlt_system\docs\sales-request-review\04h-live-evidence\api4200.out.log" 2> "C:\GitHub\accountant_qlt_system\docs\sales-request-review\04h-live-evidence\api4200.err.log"
