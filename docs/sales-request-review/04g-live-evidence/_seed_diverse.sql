SELECT current_database();

-- طلب متنوع للمراجعة البصرية: ملاحظة طويلة + مصدر + موظف ناقص/موجود
WITH c AS (
  SELECT "Id" FROM "Customers" WHERE "PhoneNumber"='07701112233' ORDER BY "Id" DESC LIMIT 1
)
INSERT INTO "SalesRequests" (
  "CustomerName","CustomerPhone","SourceType","Status","BranchId",
  "ExistingCustomerId","TargetEmployeeId","IsDeleted","ConcurrencyStamp",
  "CreatedBy","CreatedAt","LastStatusNote","StatusChangedAtUtc"
) SELECT
  '04G Diverse Card — اسم طويل للمراجعة البصرية',
  '07701112233',
  2, -- Follower
  1, -- Assigned
  1,
  c."Id",
  3, -- emp_a
  false,
  0,
  1,
  NOW(),
  'ملاحظة طويلة جداً لأغراض مراجعة التصميم 04G: العميل طلب تأجيل الكشف بسبب ظرف عائلي طارئ مع الإبقاء على نفس الموظف والمصدر، ويجب أن تظهر هذه الملاحظة كاملة في التفاصيل دون قص الكارت.',
  NOW()
FROM c
RETURNING "Id","CustomerName","SourceType","Status","TargetEmployeeId","LastStatusNote";
