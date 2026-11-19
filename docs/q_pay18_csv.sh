#!/bin/bash
set -eu
docker exec -i qalaat_postgres psql -U qalaatal -d qalaat-db -c "\copy (
SELECT
  ol.\"Name\" AS list_name,
  c.\"FullName\" AS customer_name,
  c.\"PhoneNumber\" AS phone,
  o.\"Id\" AS order_id,
  p.\"Id\" AS payment_id,
  p.\"Amount\" AS amount,
  p.\"Date\"::text AS payment_date,
  to_char(p.\"CreatedAt\" AT TIME ZONE 'Asia/Baghdad', 'YYYY-MM-DD HH24:MI:SS') AS created_at_baghdad,
  coalesce(u.\"FullName\", '') AS mandob_name,
  coalesce(u.\"Username\", '') AS mandob_user,
  coalesce(p.\"ClientPaymentId\"::text, '') AS client_payment_id
FROM \"InstallmentPayments\" p
JOIN \"Orders\" o ON o.\"Id\"=p.\"OrderId\"
JOIN \"Customers\" c ON c.\"Id\"=o.\"CustomerId\"
LEFT JOIN \"OrderLists\" ol ON ol.\"Id\"=o.\"OrderListId\"
LEFT JOIN \"Users\" u ON u.\"Id\"=ol.\"MandobId\"
WHERE o.\"BranchId\"=2
  AND p.\"Date\" = '2026-11-18'
ORDER BY ol.\"Name\", p.\"CreatedAt\", c.\"FullName\"
) TO STDOUT WITH CSV HEADER" > /tmp/najaf-payments-2026-11-18.csv
wc -l /tmp/najaf-payments-2026-11-18.csv
head -2 /tmp/najaf-payments-2026-11-18.csv