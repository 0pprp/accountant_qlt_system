SELECT current_database();
SELECT "Id","RequestId","ChangeType","PublishedAtUtc","RetryCount","LastError"
FROM "SalesRequestChangeOutbox"
WHERE "PublishedAtUtc" IS NULL
ORDER BY "Id";
-- Force a stuck row then clear for recovery observation
UPDATE "SalesRequestChangeOutbox"
SET "RetryCount"=20, "LastError"='04F forced publish failure'
WHERE "Id"=13 AND "PublishedAtUtc" IS NULL;
SELECT "Id","RetryCount","LastError","PublishedAtUtc" FROM "SalesRequestChangeOutbox" WHERE "Id"=13;
-- Restore for dispatcher recovery
UPDATE "SalesRequestChangeOutbox"
SET "RetryCount"=0, "LastError"=NULL, "LastAttemptAtUtc"=NULL
WHERE "Id"=13 AND "PublishedAtUtc" IS NULL;
SELECT "Id","RetryCount","LastError","PublishedAtUtc" FROM "SalesRequestChangeOutbox" WHERE "Id"=13;
