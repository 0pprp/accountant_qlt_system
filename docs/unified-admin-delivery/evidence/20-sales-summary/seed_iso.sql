-- Isolated seed for batch 20/21 live verification (qalaat_20_21_iso only)
SELECT "Id", "Username" FROM "Users";
SELECT "Id", "Name" FROM "Permissions" WHERE "Name" LIKE 'SalesSummary%';
SELECT COUNT(*) AS branch_count FROM "Branches";
SELECT COUNT(*) AS province_count FROM "Provinces";
\d "Users"
