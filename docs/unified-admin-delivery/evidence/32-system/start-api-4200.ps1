$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ConnectionStrings__Postgres='Host=localhost;Port=5432;Database=qalaat_22_23_iso;Username=postgres;Password=postgres'
$env:ASPNETCORE_URLS='http://127.0.0.1:4200'
Set-Location 'C:\GitHub\accountant_qlt_system\api-main\api-main'
dotnet run --project src/Api/Api.csproj --no-launch-profile
