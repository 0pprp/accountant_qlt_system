-- بيانات تجريبية بشكل فرع النجف — وهمية بالكامل، لا تُشتق من الإنتاج.
-- الهدف: تشغيل تبويب القاصة على حجم واقعي: ١٠٠ زبون، ٤ قوائم، تسديدات خلال آخر ٣٠ يوماً.
-- كل صف مَوسوم بـ ImportKey = 'seed|najaf-like|...' ليمكن حذفه كله دفعة واحدة.
-- idempotent: إعادة التشغيل لا تضاعف شيئاً.
--
-- الترتيب: هذا الملف أولاً، ثم إعادة تشغيل الـAPI ليُنشئ SafeProvisioner الخزائن،
-- ثم seed_najaf_like_openings.sql لترصيد الخزائن.

BEGIN;

-- ١) المحافظة والفرع ------------------------------------------------------
INSERT INTO "Provinces" ("Name")
SELECT 'النجف'
WHERE NOT EXISTS (SELECT 1 FROM "Provinces" WHERE "Name" = 'النجف');

INSERT INTO "Branches" ("Name", "ProvinceId", "CreatedAt", "LastUpdatedAt", "CreatedBy", "LastUpdatedBy")
SELECT 'فرع النجف', p."Id", now(), now(), 0, 0
FROM "Provinces" p
WHERE p."Name" = 'النجف'
  AND NOT EXISTS (SELECT 1 FROM "Branches" WHERE "Name" = 'فرع النجف');

-- ٢) الكادر ---------------------------------------------------------------
-- كلمة السر لكل هؤلاء هي نفس كلمة سر حساب ahmed، منسوخة كهاش جاهز.
INSERT INTO "Users" (
    "FullName", "MotherName", "Username", "PasswordHash", "NationalCode", "BirthDate",
    "PhoneNumber", "SecurityStamp", "CreationStep", "CreatedAt", "CreatedBy",
    "LastUpdatedAt", "LastUpdatedBy", "Address", "UndeliveredCashAmount", "PermissionsVersion")
SELECT
    s.full_name,
    'أم ' || s.full_name,
    s.username,
    (SELECT "PasswordHash" FROM "Users" WHERE "Username" = 'ahmed'),
    '199' || lpad(s.seq::text, 8, '0'),
    DATE '1990-01-01' + (s.seq * 97),
    '0770' || lpad((1000000 + s.seq * 37)::text, 7, '0'),
    gen_random_uuid(),
    2,
    now(), 0, now(), 0,
    'النجف - حي ' || s.seq,
    0, 1
FROM (VALUES
    ('مندوب النجف ١', 'naj_mandob1', 1),
    ('مندوب النجف ٢', 'naj_mandob2', 2),
    ('مندوب النجف ٣', 'naj_mandob3', 3),
    ('مندوب النجف ٤', 'naj_mandob4', 4),
    ('متابع النجف ١', 'naj_motaba1', 5),
    ('متابع النجف ٢', 'naj_motaba2', 6),
    ('متابع النجف ٣', 'naj_motaba3', 7),
    ('موظف مبيعات النجف ١', 'naj_sales1', 8),
    ('موظف مبيعات النجف ٢', 'naj_sales2', 9)
) AS s(full_name, username, seq)
WHERE NOT EXISTS (SELECT 1 FROM "Users" u WHERE u."Username" = s.username);

-- الأدوار: المندوبون وموظفو المبيعات Mandob، المتابعون Motaba.
INSERT INTO "UserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "Users" u
JOIN "Roles" r ON r."Name" = CASE
        WHEN u."Username" LIKE 'naj_motaba%' THEN 'Motaba'
        ELSE 'Mandob'
    END
WHERE u."Username" LIKE 'naj\_%'
  AND NOT EXISTS (SELECT 1 FROM "UserRoles" ur WHERE ur."UserId" = u."Id" AND ur."RoleId" = r."Id");

INSERT INTO "UserBranches" ("UserId", "BranchId")
SELECT u."Id", b."Id"
FROM "Users" u
CROSS JOIN "Branches" b
WHERE u."Username" LIKE 'naj\_%'
  AND b."Name" = 'فرع النجف'
  AND NOT EXISTS (SELECT 1 FROM "UserBranches" ub WHERE ub."UserId" = u."Id" AND ub."BranchId" = b."Id");

-- موظفو المبيعات يُعرَّفون بالتكليف النشط وحده، وهو ما يقرأه SafeProvisioner.
INSERT INTO "SalesEmployeeAssignments" ("UserId", "BranchId", "StartsAtUtc", "EndsAtUtc", "CreatedAtUtc", "CreatedByUserId", "Note")
SELECT u."Id", b."Id", now() - interval '90 days', NULL, now(), NULL, 'بيانات تجريبية'
FROM "Users" u
CROSS JOIN "Branches" b
WHERE u."Username" LIKE 'naj\_sales%'
  AND b."Name" = 'فرع النجف'
  AND NOT EXISTS (
      SELECT 1 FROM "SalesEmployeeAssignments" a
      WHERE a."UserId" = u."Id" AND a."BranchId" = b."Id");

-- ٣) القوائم --------------------------------------------------------------
-- القائمة الرابعة عليها تحصيل المتابع، فتُختبر الحالتان: المندوب يجبي والمتابع يجبي.
INSERT INTO "OrderLists" ("Name", "MandobId", "MotabaId", "BranchId", "CreatedAt", "LastUpdatedAt",
                          "CreatedBy", "LastUpdatedBy", "FollowerCollectionEnabled", "FollowerCollectionAssignedAtUtc")
SELECT
    s.list_name,
    (SELECT "Id" FROM "Users" WHERE "Username" = s.mandob),
    (SELECT "Id" FROM "Users" WHERE "Username" = s.motaba),
    b."Id",
    now(), now(), 0, 0,
    s.follower_collection,
    CASE WHEN s.follower_collection THEN now() - interval '20 days' ELSE NULL END
FROM (VALUES
    ('قائمة النجف المركز',   'naj_mandob1', 'naj_motaba1', false),
    ('قائمة النجف الكوفة',   'naj_mandob2', 'naj_motaba1', false),
    ('قائمة النجف الحيدرية', 'naj_mandob3', 'naj_motaba2', false),
    ('قائمة النجف المناذرة', 'naj_mandob4', 'naj_motaba3', true)
) AS s(list_name, mandob, motaba, follower_collection)
CROSS JOIN "Branches" b
WHERE b."Name" = 'فرع النجف'
  AND NOT EXISTS (SELECT 1 FROM "OrderLists" ol WHERE ol."Name" = s.list_name);

-- ٤) ١٠٠ زبون -------------------------------------------------------------
INSERT INTO "Customers" (
    "FullName", "MotherName", "NationalCode", "BirthDate", "PhoneNumber", "WhatsAppPhoneNumber",
    "Business_Address", "Business_Name", "Business_NearestKnownLocation",
    "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy", "BranchId",
    "Business_GpsLatitude", "Business_GpsLongitude", "ImportKey")
SELECT
    (ARRAY['حسين','علي','محمد','كرار','مرتضى','زيد','أحمد','مصطفى','حيدر','باقر',
           'فاطمة','زينب','رقية','نور','سجى','أمل','هدى','سارة','مريم','دعاء'])[1 + (g.n % 20)]
      || ' ' ||
    (ARRAY['الجبوري','العامري','الخفاجي','الفتلاوي','الشمري','البديري','الحسناوي','المياحي','الركابي','الزيادي'])[1 + (g.n % 10)],
    'أم ' || (ARRAY['حسين','علي','محمد','كرار','زينب','نور'])[1 + (g.n % 6)],
    '198' || lpad((400000 + g.n)::text, 8, '0'),
    DATE '1975-01-01' + (g.n * 83),
    '0781' || lpad((2000000 + g.n * 13)::text, 7, '0'),
    '0781' || lpad((2000000 + g.n * 13)::text, 7, '0'),
    'النجف - ' || (ARRAY['حي السلام','حي الأمير','حي النصر','الكوفة','المشراق','حي الوفاء'])[1 + (g.n % 6)],
    (ARRAY['بقالة','محل مواد غذائية','ورشة تصليح','محل ألبسة','مطعم','صيدلية','محل عطور'])[1 + (g.n % 7)]
      || ' ' || g.n,
    (ARRAY['قرب جامع الكوفة','مقابل المدرسة','قرب السوق الكبير','جانب الصيدلية','مقابل الحسينية'])[1 + (g.n % 5)],
    now() - (g.n || ' days')::interval,
    0,
    now(), 0,
    (SELECT "Id" FROM "Branches" WHERE "Name" = 'فرع النجف'),
    31.9950 + (g.n % 40) * 0.0012,
    44.3150 + (g.n % 37) * 0.0014,
    'seed|najaf-like|customer|' || g.n
FROM generate_series(1, 100) AS g(n)
WHERE NOT EXISTS (
    SELECT 1 FROM "Customers" c WHERE c."ImportKey" = 'seed|najaf-like|customer|' || g.n);

-- ٥) الطلبات — طلب لكل زبون، موزّعة على القوائم الأربع ---------------------
INSERT INTO "Orders" (
    "BuyAmount", "SellAmount", "PrepaymentAmount", "DailyInstallmentAmount",
    "CreationAddress", "SaleDate", "SaleTime", "Step", "SellerId", "CustomerId", "OrderListId",
    "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy",
    "ExecutionStatus", "BranchId", "ApprovalStatus", "WorkflowStatus", "SalesCompletedAtUtc", "ImportKey")
SELECT
    900000 + (seq % 7) * 100000,                 -- BuyAmount
    1500000 + (seq % 7) * 150000,                -- SellAmount
    (50 + (seq % 6) * 25) * 1000,                -- PrepaymentAmount: ٥٠ إلى ١٧٥ ألفاً
    10000,                                       -- DailyInstallmentAmount
    c."Business_Address",
    (CURRENT_DATE - (40 + (seq % 20)))::date,
    TIME '10:30:00',
    2,                                           -- Step.Completed
    seller."Id",
    c."Id",
    lst.list_id,
    now() - ((40 + (seq % 20)) || ' days')::interval,
    0, now(), 0,
    1,                                           -- ExecutionStatus.InProgress
    (SELECT "Id" FROM "Branches" WHERE "Name" = 'فرع النجف'),
    1,                                           -- ApprovalStatus.Approved
    5,                                           -- SalesWorkflowStatus.Sold
    now() - ((40 + (seq % 20)) || ' days')::interval,
    'seed|najaf-like|order|' || seq
FROM (
    SELECT c.*, (regexp_replace(c."ImportKey", '^.*\|', ''))::int AS seq
    FROM "Customers" c
    WHERE c."ImportKey" LIKE 'seed|najaf-like|customer|%'
) c
JOIN LATERAL (
    SELECT ol."Id" AS list_id
    FROM "OrderLists" ol
    WHERE ol."Name" IN ('قائمة النجف المركز','قائمة النجف الكوفة','قائمة النجف الحيدرية','قائمة النجف المناذرة')
    ORDER BY ol."Id"
    OFFSET (c.seq % 4) LIMIT 1
) lst ON true
JOIN LATERAL (
    SELECT u."Id"
    FROM "Users" u
    WHERE u."Username" LIKE 'naj\_sales%'
    ORDER BY u."Id"
    OFFSET (c.seq % 2) LIMIT 1
) seller ON true
WHERE NOT EXISTS (
    SELECT 1 FROM "Orders" o WHERE o."ImportKey" = 'seed|najaf-like|order|' || c.seq);

-- ٦) التسديدات — كلها داخل آخر ٣٠ يوماً ------------------------------------
-- المُحصِّل = مندوب القائمة، إلا القائمة المفعّل عليها تحصيل المتابع فالمتابع.
INSERT INTO "InstallmentPayments" (
    "Amount", "Date", "Description", "OrderId", "CustomerId",
    "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy", "ClientPaymentId", "ImportKey")
SELECT
    o."DailyInstallmentAmount",
    (CURRENT_DATE - ((g.n - 1) * 2 + (o.seq % 2)))::date,
    'تسديد تجريبي',
    o."Id",
    o."CustomerId",
    now() - (((g.n - 1) * 2 + (o.seq % 2)) || ' days')::interval,
    o.collector,
    now(),
    o.collector,
    gen_random_uuid(),
    'seed|najaf-like|pay|' || o.seq || '|' || g.n
FROM (
    SELECT
        o."Id", o."CustomerId", o."DailyInstallmentAmount",
        (regexp_replace(o."ImportKey", '^.*\|', ''))::int AS seq,
        CASE WHEN ol."FollowerCollectionEnabled" THEN ol."MotabaId" ELSE ol."MandobId" END AS collector
    FROM "Orders" o
    JOIN "OrderLists" ol ON ol."Id" = o."OrderListId"
    WHERE o."ImportKey" LIKE 'seed|najaf-like|order|%'
) o
CROSS JOIN LATERAL generate_series(1, 3 + (o.seq % 10)) AS g(n)
WHERE NOT EXISTS (
    SELECT 1 FROM "InstallmentPayments" p
    WHERE p."ImportKey" = 'seed|najaf-like|pay|' || o.seq || '|' || g.n);

COMMIT;

-- تقرير ------------------------------------------------------------------
SELECT 'branch'    AS entity, count(*) FROM "Branches"  WHERE "Name" = 'فرع النجف'
UNION ALL SELECT 'staff',      count(*) FROM "Users"     WHERE "Username" LIKE 'naj\_%'
UNION ALL SELECT 'lists',      count(*) FROM "OrderLists" WHERE "Name" LIKE 'قائمة النجف%'
UNION ALL SELECT 'customers',  count(*) FROM "Customers" WHERE "ImportKey" LIKE 'seed|najaf-like|customer|%'
UNION ALL SELECT 'orders',     count(*) FROM "Orders"    WHERE "ImportKey" LIKE 'seed|najaf-like|order|%'
UNION ALL SELECT 'payments',   count(*) FROM "InstallmentPayments" WHERE "ImportKey" LIKE 'seed|najaf-like|pay|%'
UNION ALL SELECT 'payments_last_30d', count(*) FROM "InstallmentPayments"
    WHERE "ImportKey" LIKE 'seed|najaf-like|pay|%' AND "Date" >= CURRENT_DATE - 30;
