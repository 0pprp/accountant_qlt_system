-- ترصيد خزائن البيانات التجريبية بحركة «رصيد افتتاحي» واحدة لكل خزينة.
-- يُشغَّل بعد seed_najaf_like.sql وبعد إعادة تشغيل الـAPI (ليكون SafeProvisioner قد أنشأ الخزائن).
--
-- التوزيع هو نفسه الذي ستنتجه الدفعة الخامسة للتحصيلات الجديدة:
--   قائمة يجبيها مندوبها            → خزينة القائمة
--   قائمة مفعّل عليها تحصيل المتابع → خزينة المتابع
--   مقدمة البيع                      → خزينة موظف المبيعات البائع
--
-- idempotent: ImportKey يمنع التكرار، وتحديث الرصيد لا يقع إلا على الحركات المُدرَجة فعلاً.

BEGIN;

WITH attribution AS (
    -- تحصيل المندوب → خزينة القائمة
    SELECT s."Id" AS safe_id, SUM(p."Amount") AS amount
    FROM "Safes" s
    JOIN "OrderLists" l ON l."Id" = s."OrderListId"
    JOIN "Orders" o ON o."OrderListId" = l."Id"
    JOIN "InstallmentPayments" p ON p."OrderId" = o."Id"
    WHERE s."Kind" = 2
      AND l."FollowerCollectionEnabled" = false
      AND p."ImportKey" LIKE 'seed|najaf-like|pay|%'
    GROUP BY s."Id"

    UNION ALL

    -- تحصيل المتابع → خزينة المتابع
    SELECT s."Id", SUM(p."Amount")
    FROM "Safes" s
    JOIN "OrderLists" l ON l."MotabaId" = s."UserId" AND l."BranchId" = s."BranchId"
    JOIN "Orders" o ON o."OrderListId" = l."Id"
    JOIN "InstallmentPayments" p ON p."OrderId" = o."Id"
    WHERE s."Kind" = 3
      AND l."FollowerCollectionEnabled" = true
      AND p."ImportKey" LIKE 'seed|najaf-like|pay|%'
    GROUP BY s."Id"

    UNION ALL

    -- المقدمة → خزينة موظف المبيعات
    SELECT s."Id", SUM(o."PrepaymentAmount")
    FROM "Safes" s
    JOIN "Orders" o ON o."SellerId" = s."UserId" AND o."BranchId" = s."BranchId"
    WHERE s."Kind" = 3
      AND o."ImportKey" LIKE 'seed|najaf-like|order|%'
    GROUP BY s."Id"
),
totals AS (
    SELECT safe_id, SUM(amount) AS amount
    FROM attribution
    GROUP BY safe_id
    HAVING SUM(amount) > 0
),
inserted AS (
    INSERT INTO "SafeMovements" (
        "SafeId", "Kind", "Amount", "BalanceAfter", "Note", "PerformedByUserId",
        "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy", "ImportKey")
    SELECT
        t.safe_id,
        0,                                   -- SafeMovementKind.Opening
        t.amount,
        s."Balance" + t.amount,
        'رصيد افتتاحي — بيانات تجريبية',
        (SELECT "Id" FROM "Users" WHERE "Username" = 'ahmed'),
        now(),
        (SELECT "Id" FROM "Users" WHERE "Username" = 'ahmed'),
        now(),
        (SELECT "Id" FROM "Users" WHERE "Username" = 'ahmed'),
        'seed|najaf-like|opening|' || t.safe_id
    FROM totals t
    JOIN "Safes" s ON s."Id" = t.safe_id
    WHERE NOT EXISTS (
        SELECT 1 FROM "SafeMovements" m
        WHERE m."ImportKey" = 'seed|najaf-like|opening|' || t.safe_id)
    RETURNING "SafeId", "Amount"
)
UPDATE "Safes" s
SET "Balance" = s."Balance" + i."Amount"
FROM inserted i
WHERE s."Id" = i."SafeId";

COMMIT;

-- تحقق: الرصيد المخزون = مجموع السجل، لكل خزينة في فرع النجف.
SELECT
    s."Id",
    s."Name",
    s."Kind",
    s."Balance" AS stored,
    COALESCE((SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId" = s."Id"), 0) AS ledger,
    CASE
        WHEN abs(s."Balance" - COALESCE((SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId" = s."Id"), 0)) < 0.001
        THEN 'OK' ELSE 'MISMATCH'
    END AS verdict
FROM "Safes" s
WHERE s."BranchId" = (SELECT "Id" FROM "Branches" WHERE "Name" = 'فرع النجف')
ORDER BY s."Kind", s."Id";
