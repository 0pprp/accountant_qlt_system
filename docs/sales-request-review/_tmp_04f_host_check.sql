SELECT current_database() AS db, inet_server_addr() AS addr, inet_server_port() AS port;
SELECT EXISTS (
  SELECT 1 FROM information_schema.tables WHERE table_name = 'SalesRequests'
) AS has_sales_requests;
SELECT EXISTS (
  SELECT 1 FROM information_schema.tables WHERE table_name = 'SalesRequestOutboxMessages'
) AS has_outbox;
SELECT column_name FROM information_schema.columns
WHERE table_name = 'SalesRequests'
  AND column_name IN ('ExceptionState','ExceptionConsumedAtUtc','DeleteReason','IsDeleted','LinkedOrderId','ConcurrencyStamp')
ORDER BY 1;
