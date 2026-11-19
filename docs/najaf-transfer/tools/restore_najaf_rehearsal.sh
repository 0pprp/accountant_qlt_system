#!/bin/bash
set -euo pipefail
BACKUP_PATH="${BACKUP_PATH:-/root/najaf-transfer-BluaWosX/najaf-master.bak}"
EXPECTED_SHA="3dfc133f8a4192dd6fa74ac2741b7c3be792e90e84a20caeb5516173ddf3040d"
INSTANCE="najaf-rehearsal-bluawosx"
SQL18="/opt/mssql-tools18/bin/sqlcmd"
SQL17="/opt/mssql-tools/bin/sqlcmd"
# Backup was taken on SQL Server 2025 (version 998); 2022 (958) cannot restore it.
IMAGE="${MSSQL_IMAGE:-mcr.microsoft.com/mssql/server:2025-latest}"

ACTUAL_SHA="$(sha256sum "$BACKUP_PATH" | awk '{print $1}')"
[[ "$ACTUAL_SHA" == "$EXPECTED_SHA" ]] || { echo "bad checksum $ACTUAL_SHA"; exit 1; }
echo "Checksum OK"

docker rm -f "$INSTANCE" 2>/dev/null || true

export MSSQL_SA_PASSWORD="Qa!$(openssl rand -hex 16)"
# Contabo/KVM: SQL 2025 crashes without --privileged (sqlpal errno 11).
docker run -d --name "$INSTANCE" --privileged --memory 8g --cpus 3 \
  -e ACCEPT_EULA=Y -e MSSQL_PID=Developer -e "MSSQL_SA_PASSWORD=$MSSQL_SA_PASSWORD" \
  "$IMAGE"

pick_sql() {
  if docker exec "$INSTANCE" test -x "$SQL18" 2>/dev/null; then echo "$SQL18"
  elif docker exec "$INSTANCE" test -x "$SQL17" 2>/dev/null; then echo "$SQL17"
  else echo "$SQL18"; fi
}

ready=0
for i in $(seq 1 120); do
  SQLBIN="$(pick_sql)"
  if docker exec -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" "$INSTANCE" "$SQLBIN" -S localhost -C -I -U sa -b -l 3 -Q "SELECT 1" >/dev/null 2>&1; then ready=1; break; fi
  # detect early crash
  if ! docker ps --format '{{.Names}}' | grep -qx "$INSTANCE"; then
    echo "SQL container exited early"; docker logs "$INSTANCE" --tail 80; exit 1
  fi
  sleep 3
done
[[ "$ready" == "1" ]] || { echo "SQL not ready"; docker logs "$INSTANCE" --tail 80; exit 1; }
SQLBIN="$(pick_sql)"
sql() { docker exec -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" "$INSTANCE" "$SQLBIN" -S localhost -C -I -U sa -b -l 5 -Q "$1"; }

docker exec -u 0 "$INSTANCE" bash -lc 'mkdir -p /var/opt/mssql/backup && chown mssql:mssql /var/opt/mssql/backup'
docker cp "$BACKUP_PATH" "${INSTANCE}:/var/opt/mssql/backup/najaf.bak"
docker exec -u 0 "$INSTANCE" bash -lc 'chown mssql:mssql /var/opt/mssql/backup/najaf.bak && chmod 640 /var/opt/mssql/backup/najaf.bak'

sql "RESTORE FILELISTONLY FROM DISK='/var/opt/mssql/backup/najaf.bak'"
sql "RESTORE VERIFYONLY FROM DISK='/var/opt/mssql/backup/najaf.bak' WITH CHECKSUM"
sql "RESTORE DATABASE NajafRehearsal FROM DISK='/var/opt/mssql/backup/najaf.bak' WITH CHECKSUM, MOVE 'DatabaseCompanyNajaf' TO '/var/opt/mssql/data/NajafRehearsal.mdf', MOVE 'DatabaseCompanyNajaf_log' TO '/var/opt/mssql/data/NajafRehearsal.ldf'"
sql "ALTER DATABASE NajafRehearsal SET READ_ONLY; DBCC CHECKDB('NajafRehearsal') WITH PHYSICAL_ONLY, NO_INFOMSGS;"
sql "USE NajafRehearsal; SELECT COUNT(*) Customers, SUM(CAST(AmountRemaining AS decimal(28,2))) Net, SUM(CASE WHEN AmountRemaining>0 THEN CAST(AmountRemaining AS decimal(28,2)) ELSE 0 END) Debts, SUM(CASE WHEN AmountRemaining<0 THEN -CAST(AmountRemaining AS decimal(28,2)) ELSE 0 END) Credit FROM dbo.View_CustomersRemaining;"
echo REHEARSAL_READY
