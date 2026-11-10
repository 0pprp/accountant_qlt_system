SELECT column_name, is_nullable, data_type
FROM information_schema.columns
WHERE table_name = 'Orders'
ORDER BY ordinal_position;
SELECT "Id","Username","FullName" FROM "Users" ORDER BY "Id";
SELECT "Id","Name" FROM "Branches";
