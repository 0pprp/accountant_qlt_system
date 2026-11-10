INSERT INTO "SalesEmployeeAssignments" ("UserId","BranchId","StartsAtUtc","EndsAtUtc","CreatedAtUtc","CreatedByUserId","Note")
SELECT 5,1,NOW()-INTERVAL '1 hour',NULL,NOW(),1,'restore B'
WHERE NOT EXISTS (
  SELECT 1 FROM "SalesEmployeeAssignments" WHERE "UserId"=5 AND "BranchId"=1 AND "EndsAtUtc" IS NULL
);
