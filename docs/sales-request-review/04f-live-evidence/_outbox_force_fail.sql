SELECT current_database();
-- Insert unpublished outbox event then mark exhausted (simulates publish failure after commit)
INSERT INTO "SalesRequestChangeOutbox" (
  "EventId","RequestId","Revision","OccurredAtUtc","ChangeType",
  "BranchId","TargetEmployeeId","PreviousTargetEmployeeId",
  "PublishedAtUtc","RetryCount","LastAttemptAtUtc","LastError"
) VALUES (
  gen_random_uuid(), 3, 999, NOW(), 'Invalidation',
  1, 4, 3,
  NULL, 20, NOW(), '04F simulated publish failure after commit'
) RETURNING "Id","RetryCount","PublishedAtUtc","LastError";

-- Recovery: reset retry so dispatcher can publish
UPDATE "SalesRequestChangeOutbox"
SET "RetryCount"=0, "LastError"=NULL, "LastAttemptAtUtc"=NULL
WHERE "RequestId"=3 AND "Revision"=999 AND "PublishedAtUtc" IS NULL
RETURNING "Id","RetryCount","PublishedAtUtc";
