SELECT current_database() AS db, inet_server_addr() AS addr, inet_server_port() AS port;

SELECT table_name FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN (
    'SalesRequests','SalesRequestTimelineEntries','SalesRequestChangeOutbox'
  )
ORDER BY 1;

SELECT column_name FROM information_schema.columns
WHERE table_name = 'SalesRequests'
  AND column_name IN (
    'LinkedOrderId','LastStatusNote','ViewedAtUtc','StatusChangedAtUtc','ConcurrencyStamp',
    'ExceptionHoldStatus','ExceptionDecidedByUserId','ExceptionDecidedAtUtc','ExceptionConsumedAtUtc',
    'DeletedAtUtc','DeletedByUserId','DeleteReason','IsDeleted'
  )
ORDER BY 1;

SELECT indexname FROM pg_indexes
WHERE tablename IN ('SalesRequests','SalesRequestChangeOutbox')
  AND indexname IN (
    'IX_SalesRequests_LinkedOrderId',
    'IX_SalesRequestChangeOutbox_EventId',
    'IX_SalesRequests_BranchId_DeletedAtUtc',
    'IX_SalesRequests_BranchId_ExceptionHoldStatus'
  )
ORDER BY 1;
