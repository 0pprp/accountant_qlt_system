INSERT INTO "SalesRequests" (
  "CustomerName", "CustomerPhone", "Address", "RequestedProduct",
  "SourceType", "BranchId", "Status", "IsDeleted",
  "ConcurrencyStamp", "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy",
  "SubmissionKey", "SubmissionFingerprint", "CreatedByUserId"
) VALUES (
  'زبون قانونية 2', '07701234568', 'العنوان الكامل للتحقق', 'منتج',
  0, 1, 0, false,
  1, NOW(), 1, NOW(), 1,
  gen_random_uuid(), 'iso-fp-2', 1
) RETURNING "Id";
