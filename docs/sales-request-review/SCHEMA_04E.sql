-- 04E SalesRequest extra views: exception consume + soft delete audit (manual, no EF migration).
-- Apply on an isolated PostgreSQL only AFTER SCHEMA_REQUIRED.md + SCHEMA_REQUIRED_04A.md.
-- The ModelSnapshot gap remains (same policy as 04A/04D). No migration file generated.
-- Entity SalesRequest now maps these columns: entity-loading queries fail until this script is applied.

ALTER TABLE "SalesRequests"
  ADD COLUMN IF NOT EXISTS "ExceptionDecidedByUserId" integer NULL,
  ADD COLUMN IF NOT EXISTS "ExceptionDecidedAtUtc" timestamp with time zone NULL,
  ADD COLUMN IF NOT EXISTS "ExceptionConsumedAtUtc" timestamp with time zone NULL,
  ADD COLUMN IF NOT EXISTS "DeletedAtUtc" timestamp with time zone NULL,
  ADD COLUMN IF NOT EXISTS "DeletedByUserId" integer NULL,
  ADD COLUMN IF NOT EXISTS "DeleteReason" character varying(500) NULL;

-- Deleted view: soft-deleted rows per branch, newest first.
CREATE INDEX IF NOT EXISTS "IX_SalesRequests_BranchId_DeletedAtUtc"
  ON "SalesRequests" ("BranchId", "DeletedAtUtc")
  WHERE "IsDeleted" = true;

-- Exceptions view: rows carrying an exception hold.
CREATE INDEX IF NOT EXISTS "IX_SalesRequests_BranchId_ExceptionHoldStatus"
  ON "SalesRequests" ("BranchId", "ExceptionHoldStatus")
  WHERE "ExceptionHoldStatus" IS NOT NULL;

-- Semantics (no data backfill needed):
--   ExceptionHoldStatus: 0 Pending, 1 Approved, 2 Rejected.
--   A hold is LIVE when Pending, Rejected, or Approved with "ExceptionConsumedAtUtc" IS NULL.
--   Approved + "ExceptionConsumedAtUtc" IS NOT NULL = consumed (used once by assign/reassign); not reusable.
--   Existing Approved rows keep ExceptionConsumedAtUtc NULL, i.e. remain live (unused) after the upgrade.
--
-- SalesRequestTimelineEntries."EventType" smallint gains values (no DDL):
--   13 ExceptionApproved, 14 ExceptionRejected, 15 ExceptionConsumed, 16 SoftDeleted, 17 Restored.
--
-- Permissions are seeded by the application (Permissions.SalesRequest.OutsideAll -> Admin role):
--   SalesRequest.TabSent, TabExceptions, TabDeleted, ExceptionDecide, SoftDelete, Restore.
--
-- Optional verification:
--   SELECT "Id","ExceptionHoldStatus","ExceptionConsumedAtUtc" FROM "SalesRequests"
--   WHERE "ExceptionHoldStatus" = 1 AND "ExceptionConsumedAtUtc" IS NULL;
--   SELECT "Id","DeletedAtUtc","DeleteReason" FROM "SalesRequests" WHERE "IsDeleted" = true;
