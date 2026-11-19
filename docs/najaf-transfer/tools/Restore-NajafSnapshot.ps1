param([Parameter(Mandatory=$true)][string]$BackupPath)
$ErrorActionPreference = 'Stop'
$backup = Get-Item -LiteralPath $BackupPath
$expected = '3dfc133f8a4192dd6fa74ac2741b7c3be792e90e84a20caeb5516173ddf3040d'
if ((Get-FileHash -LiteralPath $backup.FullName).Hash -ne $expected) { throw 'Unexpected backup checksum' }
$instance = 'najaf-rehearsal-bluawosx'
$sql = '/opt/mssql-tools18/bin/sqlcmd'
$env:MSSQL_SA_PASSWORD = 'Qa!' + [guid]::NewGuid().ToString('N')
try {
    docker run -d --name $instance --network none --memory 4g --cpus 2 -e ACCEPT_EULA=Y -e MSSQL_PID=Developer -e MSSQL_SA_PASSWORD mcr.microsoft.com/mssql/server:2025-latest
    if ($LASTEXITCODE) { throw 'Cannot create isolated instance; check whether this rehearsal already exists.' }
} finally { Remove-Item Env:MSSQL_SA_PASSWORD }
function Invoke-RehearsalSql([string]$Query) {
    docker exec $instance sh -c 'export SQLCMDPASSWORD="$MSSQL_SA_PASSWORD"; exec "$1" -S localhost -C -I -U sa -b -l 5 -Q "$2"' sh $sql $Query
    if ($LASTEXITCODE) { throw 'Rehearsal SQL command failed.' }
}
$ready = $false
for ($i=0; $i -lt 60; $i++) {
    try { Invoke-RehearsalSql 'SELECT 1'; $ready = $true; break } catch { Start-Sleep -Seconds 2 }
}
if (!$ready) { throw 'SQL instance did not become ready' }
docker cp $backup.FullName "${instance}:/tmp/najaf.bak"
if ($LASTEXITCODE) { throw 'Could not copy snapshot to isolated instance' }
Invoke-RehearsalSql "RESTORE VERIFYONLY FROM DISK='/tmp/najaf.bak' WITH CHECKSUM"
Invoke-RehearsalSql "RESTORE DATABASE NajafRehearsal FROM DISK='/tmp/najaf.bak' WITH CHECKSUM, MOVE 'DatabaseCompanyNajaf' TO '/var/opt/mssql/data/NajafRehearsal.mdf', MOVE 'DatabaseCompanyNajaf_log' TO '/var/opt/mssql/data/NajafRehearsal.ldf'"
Invoke-RehearsalSql "ALTER DATABASE NajafRehearsal SET READ_ONLY; DBCC CHECKDB('NajafRehearsal') WITH PHYSICAL_ONLY, NO_INFOMSGS;"
Invoke-RehearsalSql "USE NajafRehearsal; SELECT COUNT(*) Customers, SUM(CAST(AmountRemaining AS decimal(28,2))) Net, SUM(CASE WHEN AmountRemaining>0 THEN CAST(AmountRemaining AS decimal(28,2)) ELSE 0 END) Debts, SUM(CASE WHEN AmountRemaining<0 THEN -CAST(AmountRemaining AS decimal(28,2)) ELSE 0 END) Credit FROM dbo.View_CustomersRemaining;"
