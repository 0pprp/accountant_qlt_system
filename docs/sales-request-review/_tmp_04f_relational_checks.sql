-- Relational checks on isolated qalaat_04f_sales_verify
SELECT current_database() AS db, inet_server_addr() AS addr;

-- 1) Unique partial index on LinkedOrderId
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'SalesRequests' AND indexname = 'IX_SalesRequests_LinkedOrderId';

-- 2) FK LinkedOrderId -> Orders ON DELETE RESTRICT
SELECT conname, confdeltype
FROM pg_constraint
WHERE conname = 'FK_SalesRequests_Orders_LinkedOrderId';
-- confdeltype r = restrict

-- 3) Outbox table exists with EventId unique
SELECT indexname FROM pg_indexes
WHERE tablename = 'SalesRequestChangeOutbox' AND indexname = 'IX_SalesRequestChangeOutbox_EventId';

-- 4) 04E columns present
SELECT column_name
FROM information_schema.columns
WHERE table_name = 'SalesRequests'
  AND column_name IN (
    'ExceptionConsumedAtUtc','DeleteReason','DeletedAtUtc',
    'LinkedOrderId','ConcurrencyStamp','StatusChangedAtUtc'
  )
ORDER BY 1;

-- 5) Insert probe request (branch 1 must exist) then verify timeline FK cascade readiness
INSERT INTO "SalesRequests" (
  "CustomerName","CustomerPhone","SourceType","Status","BranchId","IsDeleted","ConcurrencyStamp","CreatedBy"
) VALUES (
  '04F-Probe-Customer','07000000404',0,1,
  (SELECT "Id" FROM "Branches" ORDER BY "Id" LIMIT 1),
  false,0,0
) RETURNING "Id";
