UPDATE "SalesEmployeeAssignments" SET "EndsAtUtc"=NOW() WHERE "UserId"=5 AND "EndsAtUtc" IS NULL;
