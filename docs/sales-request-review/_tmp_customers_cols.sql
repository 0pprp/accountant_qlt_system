SELECT column_name, is_nullable, column_default, data_type
FROM information_schema.columns
WHERE table_name = 'Customers'
ORDER BY ordinal_position;
SELECT COUNT(*) AS customers FROM "Customers";
